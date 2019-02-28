using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using AwardsServer.BugReport;

namespace AwardsServer
{
    public class SocketHandler
    {
        public static readonly object LockClient = new object(); // should prevent cross-thread related errors..
        // it should..
        // but it doesnt..
        public static List<SocketConnection> CurrentClients = new List<SocketConnection>(); // current students actually voting
        public static List<SocketConnection> ClientQueue = new List<SocketConnection>(); // students waiting to vote

        public static List<SocketConnection> AdminClients
        {
            get
            {
                return AllClients.Where(x => x.Authentication > Authentication.Student).ToList();
            }
        }

        public static List<SocketConnection> AllClients {  get
            {
                List<SocketConnection> list = new List<SocketConnection>();
                lock (LockClient)
                {
                    list.AddRange(CurrentClients);
                    list.AddRange(ClientQueue);
                }
                return list;
            } }

        public static List<Kick> PriorKickedUsers = new List<Kick>();
        public static Kick GetKick(SocketHandler.SocketConnection connection)
        {
            foreach(var kick in PriorKickedUsers)
            {
                if (kick.Match(connection))
                    return kick;
            }
            return null;
        }

        /// <summary>
        /// We cache the IP everyone connects to, for purposes..
        /// </summary>
        public static Dictionary<string, string> CachedKnownIPs = new Dictionary<string, string>();


        public enum Authentication
        {
            None     = 0,
            Student  = 1,
            Sysop    = 2,
            Sysadmin = 3
        }
        public class SocketConnection
        {
            public TcpClient Client;
            public IPEndPoint IPEnd;
            public string UserName; // can be different from AccountName, eg when same person joins twice
                                    // this will be randomly generated a suffix of 3 digits
            public User User;

            public bool Listening = true; // for the while loop below

            public string IPAddress;

            public Authentication Authentication = Authentication.Student;

            public void ReSendAuthentication()
            {
                Authentication = Authentication.Student;
                if (User.Flags.Contains(Flags.Automatic_Sysop))
                    this.Authentication = Authentication.Sysop;
                if (IPEnd.Address.ToString() == "127.0.0.1" || IPEnd.Address.ToString() == Program.GetLocalIPAddress() || IPEnd.Address.ToString() == "192.168.1.1")
                    this.Authentication = Authentication.Sysadmin;
                this.Send("Auth:" + ((int)Authentication).ToString());
            }

            public DateTime StartedTime;

            Thread listenThread;

            public SocketConnection(TcpClient client, string name)
            {
                Client = client;
                UserName = name;
                if(Program.TryGetUser(name, out User)) {
                    // nothing (already sets variable so..)
                    IPEndPoint ipEnd = client.Client.RemoteEndPoint as IPEndPoint;
                    IPEnd = ipEnd;
                    var ip = ipEnd.Address.ToString();
                    this.ReSendAuthentication();
                    if (Program.Options.WebSever_Enabled)
                    {
                        if(CachedKnownIPs.ContainsKey(User.AccountName)) {
                            Logging.Log(Logging.LogSeverity.Warning, $"User {User.ToString("AN FN")} was connected via {CachedKnownIPs[User.AccountName]} but now has connected via {ip}");
                            CachedKnownIPs[User.AccountName] = ip;
                        } else
                        {
                            CachedKnownIPs.Add(User.AccountName, ip);
                        }
                    }
                    User.Connection = this;
                } else
                { // this is handled in the newclient thread thingy
                    throw new ArgumentException("User not found: '" + name + "'");
                }
            }

            /// <summary>
            /// Indicates that it should begin to listen because it's been moved from the queue
            /// </summary>
            public void AcceptFromQueue(bool bypassed = false)
            {
                StartedTime = DateTime.Now;
                Logging.Log(Logging.LogSeverity.Warning, $"Bringing {this.User} from queue. {(bypassed ? " (user bypassed queue)" : "")}");
                Send("Ready:" + this.User.FirstName);
                Send("NumCat:" + Program.Database.AllCategories.Count);
                listenThread = new Thread(Listen);
                listenThread.Start();
            }

            public List<User> QueryStudent(string message)
            {
                List<User> responses = new List<User>();
                int count = 0;
                foreach (var student in Program.Database.AllStudents.Values)
                {
                    bool shouldGo = false; // shouldGo: does the name match the query? if so, SHOULD we GO and send it
                                           // yes i know its not best naming but /shrug
                    message = message.ToLower();
                    if (student.Flags.Contains(Flags.Disallow_Vote_Staff))
                        continue; // disallow for staff to be voted for, even if they are in the database.
                    if (student.ToString().ToLower().StartsWith(message))
                    {
                        shouldGo = true;
                    }
                    else if (student.LastName.ToLower().StartsWith(message)) //?? - essentially looking to see if the name contains the query, ignoring any case
                    { // it is actually just returning an index of where the query string is within the student's name (same as like list.Indexof)
                      // the >=0 is because if it does not contain ^, then it returns -1 instead
                        shouldGo = true;
                    }
                    else if (student.FirstName.ToLower().StartsWith(message))
                    {
                        shouldGo = true;
                    }
                    if (shouldGo)
                    {
                        count++;
                        if (count >= Program.Options.Maximum_Query_Response)
                        {
                            break;
                        }
                        responses.Add(student); //add the student's name + properties to a list of names to send to the client
                    }
                }
                return responses;
            }

            private void HandleMessage(string message) //when a the server receives a message from the client
            {
                if(message .StartsWith("GET_CATE:"))
                { // asking for specific category
                    string response = "";
                    message = message.Replace("GET_CATE:", "");
                    if(int.TryParse(message, out int id))
                    {
                        if(Program.Database.AllCategories.TryGetValue(id, out Category cat))
                        {
                            response = $"{cat.ID}:{cat.Prompt}";
                            this.Send("Cat:" + response);
                        }
                    }
                    // dont respond if it is out of range.
                }/* else if (message.StartsWith("SET_CATE"))
                {
                    message = message.Replace("SET_CATE", "");
                    // expecting: ID:Username

                    if(message.Contains(":"))
                    {
                        string[] options = message.Split(':');
                        if(int.TryParse(options[0], out int id))
                        {
                            if(Program.TryGetUser(options[1], out User voted))
                            {
                                Program.Database.AddVoteFor(id, voted, this.User);
                            }
                        }
                    }
                }*/ else if(message.StartsWith("SUBMIT:"))
                { // submit all votes.
                    message = message.Replace("SUBMIT:", ""); //?? what is the format? SUBMIT:category;thing#category;thing ?
                    // SUBMIT:male;female#male;female#male;female ....
                    // the male;female pairs are in order, so we should just be able to increment a counter as we go thorugh each
                    string rejectedReason = "";
                    UserVoteSubmit vote = new UserVoteSubmit(this.User);
                    try
                    {
                        string[] cats = message.Split('#'); //categories
                        for(int index = 0; index < cats.Length; index++) //go through every category
                        {
                            string thing = cats[index]; //?? a pair of male:female winners
                            if (string.IsNullOrWhiteSpace(thing))
                                continue; // but if we havnt given a winner for this category, it may be empty
                            string[] winners = thing.Split(';'); // these are "male;female", so yes
                            string maleWin = winners[0];
                            string femaleWin = winners[1];
                            Program.TryGetUser(maleWin, out User firstWinner);
                            Program.TryGetUser(femaleWin, out User secondWinner);
                            if ((firstWinner?.AccountName ?? ",") == (secondWinner?.AccountName ?? ""))
                            {
                                rejectedReason = "Rejected:Duplicate";
                                return; // break out
                            }
                            if (firstWinner != null)
                            {
                                if (firstWinner.AccountName == this.User.AccountName) //trying to vote for themself
                                    rejectedReason = "Rejected:Self";
                            }
                            if(secondWinner != null)
                            {
                                if (secondWinner.AccountName == this.User.AccountName)
                                    rejectedReason = "Rejected:Self";
                            }
                            if(string.IsNullOrWhiteSpace(rejectedReason))
                            {
                                vote.AddVote(index + 1, firstWinner, secondWinner);
                            }
                        }
                    } catch (Exception ex)
                    {
                        rejectedReason = "Rejected:Errored";
                        Logging.Log($"{UserName}/Submit", ex);
                    } finally
                    {
                        if (string.IsNullOrWhiteSpace(rejectedReason))
                        {
                            var now = DateTime.Now;
                            var ts = now - this.StartedTime;
                            Program.Database.AlreadyVotedNames.Add(this.User.AccountName);
                            vote.Submit();
                            this.Send("Accepted");
                            Logging.Log(Logging.LogSeverity.Warning, $"User has voted (took: {ts})", this.User.AccountName);
                        }
                        else
                        {
                            this.Send(rejectedReason);
                        }
                        this.Close("Submitted");
                    }
                } else if(message.StartsWith("QUERY")) //?? querying for what? - when user types in someone's name, query any student that contains that name
                {
                    message = message.Replace("QUERY:", "");
                    string response = "";
                    // format:
                    // ENTERED_TEXT
                    // its substring(2) '2' because we need to ignore first M/F and the :
                    var students = QueryStudent(message);
                    foreach(var student in students) { response += student.ToString("AN:FN:LN:TT") + "#"; }
                    this.Send("Q_RES:" + response);
                } else if(message.StartsWith("QUES:"))
                {
                    try
                    {
                        message = message.Substring(5);
                    } catch { }
                    Logging.Log(Logging.LogSeverity.Severe, "Category: " + message, this.UserName);
                    try
                    {
                        System.IO.File.AppendAllText($@"..\..\..\CategorySuggestions.txt", $"{this.UserName} - {message}\r\n");
                    } catch (Exception ex)
                    {
                        Logging.Log("SuggestFile", ex);
                    }
                } else if(message.StartsWith("REPORT:"))
                {
                    var report = BugReport.BugReport.Parse(message, this.User);
                    Logging.Log(Logging.LogSeverity.Warning,
                        $"NEW: {report.Primary ?? report.Additional}{(string.IsNullOrWhiteSpace(report.Primary) ? "" : " " + report.Additional)} @ {report.LogFile}"
                        , $"Bugs/{report.Reporter.AccountName}");
                    Program.BugReports.Add(report);
                    Program.SaveBugs();
                } else if(message.StartsWith("/"))
                {
                    // admin message
                    message = message.Substring(1);
                    if(message.StartsWith("CHAT:"))
                    {
                        message = message.Substring(5);
                        Program.SendAdminChat(new AdminMessage(this, message));
                    } else if(message.StartsWith("QUEUE"))
                    {
                        var str = "/AQU:";
                        int num = 0;
                        foreach(var s in ClientQueue)
                        {
                            num += 1;
                            str += $"{s.User.AccountName}:{s.User.FirstName}:{s.User.LastName}:{s.User.Tutor}:{num}:{s.IPAddress}#";
                        }
                        Send(str);
                    } else if(message.StartsWith("VOTERS"))
                    {
                        var str = "/AVT:";
                        int num = 0;
                        foreach (var s in CurrentClients)
                        {
                            num += 1;
                            str += $"{s.User.AccountName}:{s.User.FirstName}:{s.User.LastName}:{s.User.Tutor}:{(int)s.Authentication}:{s.IPAddress}#";
                        }
                        Send(str);
                    } else if (message.StartsWith("KICK:"))
                    {
                        message = message.Substring("KICK:".Length);
                        var split = message.Split(':');
                        if(Program.TryGetUser(split[0], out User user))
                        {
                            var conn = AllClients.FirstOrDefault(x => x.User.AccountName == user.AccountName);
                            if(conn != null)
                            {
                                if (conn.Authentication >= this.Authentication || conn.UserName == this.UserName)
                                    return; // prevent kicking self or those with higher 'auth'
                                var kick = new Kick(conn, this.User, split[1]);
                                if(Program.Options.Perm_Block_Kicked_Users)
                                    PriorKickedUsers.Add(kick);
                                conn.Send("Kicked:" + kick.Reason);
                                AdminMessage msg = new AdminMessage("Server", Authentication.Sysadmin, $"[STATUS] {kick.Kicked.AccountName} was kicked by {kick.Admin.AccountName} for {kick.Reason}");
                                Program.SendAdminChat(msg);
                                conn.Close($"Kicked by {kick.Admin.AccountName} with reason {kick.Reason}");
                            }
                        }
                    } else if(message.StartsWith("MANR:"))
                    {
                        message = message.Replace("MANR:", "");
                        if(Program.TryGetUser(message, out User user))
                        {
                            string response = "/MANRD:";
                            foreach(var category in Program.Database.AllCategories.Values)
                            {
                                var votes = category.GetVotesBy(user);
                                response += $"{votes.Item1?.ToString("AN:FN:LN:TT") ?? ""};{votes.Item2?.ToString("AN:FN:LN:TT") ?? ""}#";
                            }
                            Send(response);
                        }
                    } else if(message.StartsWith("MANVOTE:"))
                    {
                        message = message.Replace("MANVOTE:", "");
                        var split = message.Split(':');
                        if(Program.Database.AllStudents.TryGetValue(split.ElementAt(0), out User user))
                        {
                            int categoryId = 1;
                            var votes = split.ElementAt(1).Split('#').Where(x => !string.IsNullOrWhiteSpace(x));
                            foreach(string vote in votes)
                            {
                                var each = vote.Split(';');
                                Program.TryGetUser(each.ElementAt(0), out User first);
                                Program.TryGetUser(each.ElementAt(1), out User second);
                                if(first != null)
                                    Program.Database.AddVoteFor(categoryId, first, user);
                                if(second != null)
                                    Program.Database.AddVoteFor(categoryId, second, user);
                                categoryId++;
                            }
                        }

                    } else if (message.StartsWith("QUERY:"))
                    {
                        message = message.Replace("QUERY:", "");
                        var split = message.Split(':').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

                        var rowIndex = int.Parse(split[0]);
                        var colIndex = int.Parse(split[1]);
                        var queryT = split[2];
                        var students = QueryStudent(queryT);
                        if(students.Count == 1)
                        {
                            Send($"/QUERY:{rowIndex}:{colIndex}:{students[0].ToString("AN;FN;LN;TT")}");
                        }
                    }
                }
            }

            /// <summary>
            /// Closes the connection with a client, logs a reason, and updates the queue.
            /// </summary>
            /// <param name="reason">Reason to disconnect the client</param>
            public void Close(string reason = "unknown")
            {
                Logging.Log(Logging.LogSeverity.Warning, $"Closing {UserName}({this.User?.ToString() ?? "n/a"}) due to {reason}");
                try
                {
                    this.User.Connection = null;
                } catch { }
                try
                {
                    Client.Close();
                }
                catch { }
                Listening = false;
                try
                {
                    listenThread.Abort();
                }
                catch { }
                lock(LockClient)
                {
                    CurrentClients.Remove(this);
                    ClientQueue.Remove(this);
                    while(CurrentClients.Count < Program.Options.Maximum_Concurrent_Connections)
                    {//accepting the next people in the queue
                        if (ClientQueue.Count == 0)
                            break;
                        ClientQueue[0].AcceptFromQueue();
                        CurrentClients.Add(ClientQueue[0]);
                        ClientQueue.RemoveAt(0);
                    }
                }
            }

            /// <summary>
            /// Listens to any incoming messages from the client connection
            /// </summary>
            private void Listen()
            {
                while(Listening)
                {
                    try
                    {
                        NetworkStream stream = Client.GetStream();
                        Byte[] bytesFrom = new Byte[Client.ReceiveBufferSize];
                        string data;
                        stream.Read(bytesFrom, 0, Client.ReceiveBufferSize);
                        data = Encoding.UTF8.GetString(bytesFrom).Trim().Replace("\0", "");
                        if (string.IsNullOrWhiteSpace(data))
                            continue;

                        foreach(var tempMsg in data.Split('%')) //loops through received messages
                        {
                            if (string.IsNullOrWhiteSpace(tempMsg))
                                continue;
                            var message = tempMsg.Substring(0, tempMsg.LastIndexOf("`"));
                            if (string.IsNullOrWhiteSpace(message))
                                continue;
                            Logging.Log(Logging.LogSeverity.Debug, message, $"{UserName}/Rec");
                            HandleMessage(message);
                        }
                    } catch (ThreadAbortException)
                    {
                        // thread is closing, already logged - no need to catch again
                    }
                    catch (Exception ex)
                    {
                        Logging.Log($"{UserName}/Rec", ex);
                        Close("Errored");
                    }
                }
            }

            /// <summary>
            /// Sends the message, but with zero error checking
            /// </summary>
            /// <param name="client">Client to recieve the message</param>
            /// <param name="message">Message to send</param>
            public static void WriteConnection(TcpClient client, string message)
            {
                message = $"%{message}`";
                NetworkStream stream = client.GetStream();
                Byte[] broadcastBytes = Encoding.UTF8.GetBytes(message);
                stream.Write(broadcastBytes, 0, broadcastBytes.Length);
            }

            /// <summary>
            /// Sends a message to the connection, with error checking built in.
            /// </summary>
            /// <param name="message">Message to try to send</param>
            public void Send(string message)
            {
                try
                {
                    WriteConnection(this.Client, message);
                    Logging.Log( Logging.LogSeverity.Debug, message, $"{UserName}/Send");
                } catch (Exception ex)
                {
                    Logging.Log($"{UserName}/Send", ex);
                    this.Close("SendErrored");
                }
            }
public static string GetLocalIPAddress()
{
    var host = Dns.GetHostEntry(Dns.GetHostName());
    foreach (var ip in host.AddressList)
    {
        if (ip.AddressFamily == AddressFamily.InterNetwork)
        {
            return ip.ToString();
        }
    }
    throw new Exception("No network adapters with an IPv4 address in the system!");
}


            /// <summary>
            /// Sends message to ensure the client is still connected.
            /// If it is not.. then an error is raised and the client is disconnected
            /// </summary>
            public void Heartbeat()
            {
                this.Send("hi");
            }
        }

        public bool Listening = false;
        // Handles listening to, recieving information from, and sending information to
        // any clients (ie, the programs) that attempt to communicate.
        private TcpListener ServerListener;
        
        public SocketHandler()
        {
            try
            {
                ServerListener = new TcpListener(IPAddress.Any, 56567);
                ServerListener.Start();
                Listening = true;
                Logging.Log(Logging.LogSeverity.Info, $"Listening to new connections at {((IPEndPoint)ServerListener.LocalEndpoint).Address.ToString()}:{((IPEndPoint)ServerListener.LocalEndpoint).Port}");
                Thread newThread = new Thread(NewConnections);
                newThread.Start();
                                        

            } catch (Exception ex)
            {
                Logging.Log("Server", ex);
            }
        }
        private static Random rnd = new Random();
        private void NewConnections()
        {
            // todo: would accept new connections from clients and things.
            TcpClient clientSocket = new TcpClient();
            while(Listening)
            {
                try
                {
                    clientSocket = ServerListener.AcceptTcpClient();
                    Byte[] bytesFrom = new Byte[clientSocket.ReceiveBufferSize];
                    string dataFromClient;
                    NetworkStream netStream = clientSocket.GetStream();
                    try
                    {
                        netStream.Read(bytesFrom, 0, Convert.ToInt32(clientSocket.ReceiveBufferSize));
                    }
                    catch (Exception ex)
                    {
                        Logging.Log("NewConRead", ex);
                        continue;
                    }
                    dataFromClient = Encoding.UTF8.GetString(bytesFrom).Trim().Replace("\0", "");
                    IPEndPoint ipEnd = clientSocket.Client.RemoteEndPoint as IPEndPoint;
                    if (string.IsNullOrWhiteSpace(dataFromClient))
                    {
                        clientSocket.Close();
                        continue;
                    }
                    dataFromClient = dataFromClient.Substring(1, dataFromClient.LastIndexOf("`")-1);
                    SocketConnection user = null;
                    try
                    {
                        user = new SocketConnection(clientSocket, dataFromClient.ToLower());
                    }catch (ArgumentException ex)
                    { // user not found
                        SocketConnection.WriteConnection(clientSocket, "UnknownUser");
                        Logging.Log("UnknUser", ex);
                        continue;
                    } catch (Exception ex)
                    {
                        Logging.Log("NewSock", ex);
                        continue;
                    }
                    // as soon as we get a connection, we should disable the ability to edit user info from UI
                    if(!Program.Options.Allow_Modifications_When_Voting)
                        Program.ServerUIForm.PermittedStudentEdits(ServerUI.UIForm.EditCapabilities.None);
                    lock(LockClient)
                    {
                        if (CurrentClients.Select(x => x.UserName).Contains(dataFromClient) || ClientQueue.Select(x => x.UserName).Contains(dataFromClient))
                        {
                            if (!Program.Options.Simultaneous_Session_Allowed)
                            {
                                user.Send("REJECT:Already");
                                user.Close("Already Connected");
                            } else
                            { // considering we shouldnt have a person connected multiple times, this shouldnt really be called anyway
                                user.UserName += rnd.Next(0, 999).ToString("000"); // technically possible for this to collide.. but unlikely
                            }
                        }
                    }
                    user.IPAddress = ipEnd.Address.ToString();
                    Logging.Log(Logging.LogSeverity.Warning, "New connection: " + ipEnd.Address.ToString() + " -> " + user.User.ToString(), "NewCon");
                    if(Program.Database.AlreadyVotedNames.Contains(user.User.AccountName))
                    {
                        Logging.Log(Logging.LogSeverity.Warning, "Refusing connection: " + user.User.ToString() + ", already voted.");
                        user.Send("REJECT:Voted");
                        user.Close("Prior Vote");
                        continue;
                    }
                    if (user.User.Flags.Contains(Flags.Disallow_Vote_Staff))
                    {
                        user.Send("REJECT:Blocked-Online");
                        user.Close("Blocked from voting, online-only account");
                        continue;
                    }
                    var getKick = GetKick(user);
                    if (getKick != null)
                    {
                        user.Send("REJECT:Kicked:" + getKick.Reason);
                        user.Close("prior kicked, by " + getKick.Admin.AccountName);
                        continue;
                    }
                    lock (LockClient)
                    {
                        ClientQueue.Add(user);
                        user.Send("QUEUE:" + ClientQueue.Count);
                        while(CurrentClients.Count < Program.Options.Maximum_Concurrent_Connections)
                        {
                            if (ClientQueue.Count == 0)
                                break;
                            ClientQueue[0].AcceptFromQueue();
                            CurrentClients.Add(ClientQueue[0]);
                            ClientQueue.RemoveAt(0);
                        }
                        var adminsInQueue = ClientQueue.Where(x => x.Authentication > Authentication.Student).ToList();
                        foreach(var adm in adminsInQueue)
                        {
                            ClientQueue.RemoveAll(x => x.UserName == adm.UserName);
                            CurrentClients.Add(adm);
                            adm.AcceptFromQueue(true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logging.Log("NewConn", ex);
                }
            }
        }
    }


    public class AdminMessage
    {
        public string From;
        public SocketHandler.Authentication FromAuth;
        public string Content;

        public AdminMessage(string from, SocketHandler.Authentication auth, string content)
        {
            From = from;
            FromAuth = auth;
            Content = content;
        }

        public AdminMessage(SocketHandler.SocketConnection connection, string content) : this(connection.User.AccountName, connection.Authentication, content)
        {
        }

        public string ToSend()
        {
            return $"/CHAT:{From}^{(int)FromAuth}^{Content}";
        }

        public static AdminMessage Parse(string message)
        {
            if (message.StartsWith("/"))
                message = message.Substring(1);
            if (message.StartsWith("CHAT:"))
                message = message.Substring(4);
            var split = message.Split('^').ToList();
            var from = split[0];
            var auth = (SocketHandler.Authentication)int.Parse(split[1]);
            split.RemoveRange(0, 2);
            var content = string.Join("", split);
            return new AdminMessage(from, auth, content);
        }
    }

    public class Kick
    {
        public User Kicked;
        public User Admin;
        public string Reason;
        public List<string> IPAddresses;
        public bool Match(SocketHandler.SocketConnection user)
        {
            if(!Program.Options.Perm_Block_Kicked_Users)
                return false;

            if (user.User.AccountName == Kicked.AccountName)
            {
                if (!IPAddresses.Contains(user.IPAddress))
                    IPAddresses.Add(user.IPAddress);
                return true;
            }
            foreach (var ip in IPAddresses)
                if (ip == user.IPAddress || ip == user.IPEnd.Address.ToString())
                    return true;
            return false;
        }
        public Kick(User kicked, User admin, string reason, string ip)
        {
            Kicked = kicked;
            Admin = admin;
            Reason = reason;
            IPAddresses = new List<string>() { ip };
        }
        public Kick(SocketHandler.SocketConnection connection, User admin, string reason)
        {
            Kicked = connection.User;
            Admin = admin;
            Reason = reason;
            IPAddresses = new List<string>() { connection.IPAddress };
        }

    }

}

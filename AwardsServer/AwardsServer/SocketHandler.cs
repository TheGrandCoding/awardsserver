using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace AwardsServer
{
    public class SocketHandler
    {
        public static readonly object LockClient = new object(); // should prevent cross-thread related errors..
        // it should..
        // but it doesnt..
        public static List<SocketConnection> CurrentClients = new List<SocketConnection>(); // current students actually voting
        public static List<SocketConnection> ClientQueue = new List<SocketConnection>(); // students waiting to vote

        /// <summary>
        /// We cache the IP everyone connects to, for purposes..
        /// </summary>
        public static Dictionary<string, IPAddress> CachedKnownIPs = new Dictionary<string, IPAddress>();

        public class SocketConnection
        {
            public TcpClient Client;
            public string UserName; // can be different from AccountName, eg when same person joins twice
                                    // this will be randomly generated a suffix of 3 digits
            public User User;

            public bool Listening = true; // for the while loop below

            public string IPAddress;

            public DateTime StartedTime;

            Thread listenThread;

            public SocketConnection(TcpClient client, string name)
            {
                Client = client;
                UserName = name;
                if(Program.TryGetUser(name, out User)) {
                    // nothing (already sets variable so..)
                    IPEndPoint ipEnd = client.Client.RemoteEndPoint as IPEndPoint;
                    if(CachedKnownIPs.ContainsKey(User.AccountName)) {
                        Logging.Log(Logging.LogSeverity.Warning, $"User {User.ToString("AN FN")} was connected via {CachedKnownIPs[User.AccountName]} but now has connected via {ipEnd.Address}");
                        CachedKnownIPs[User.AccountName] = ipEnd.Address;
                    } else
                    {
                        CachedKnownIPs.Add(User.AccountName, ipEnd.Address);
                    }
                } else
                { // this is handled in the newclient thread thingy
                    throw new ArgumentException("User not found: '" + name + "'");
                }
            }

            /// <summary>
            /// Indicates that it should begin to listen because it's been moved from the queue
            /// </summary>
            public void AcceptFromQueue()
            {
                StartedTime = DateTime.Now;
                Logging.Log(Logging.LogSeverity.Warning, "Bringing " + this.User.ToString() + " from queue.");
                Send("Ready:" + this.User.FirstName);
                Send("NumCat:" + Program.Database.AllCategories.Count);
                listenThread = new Thread(Listen);
                listenThread.Start();
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
                            User firstWinner;
                            User secondWinner;
                            Program.TryGetUser(maleWin, out firstWinner);
                            Program.TryGetUser(femaleWin, out secondWinner);
                            if ((firstWinner?.AccountName ?? ",") == (secondWinner?.AccountName ?? ""))
                            {
                                rejectedReason = "Rejected:Duplicate";
                                return; // break out
                            }
                            if (firstWinner != null)
                            {
                                if (firstWinner.AccountName == this.User.AccountName) //trying to vote for themself
                                {
                                    rejectedReason = "Rejected:Self";
                                }
                                else
                                {
                                    Program.Database.AddVoteFor(index + 1, firstWinner, this.User);
                                }
                            }
                            if(secondWinner != null)
                            {
                                if (secondWinner.AccountName == this.User.AccountName)
                                {
                                    rejectedReason = "Rejected:Self";
                                }
                                else
                                {
                                    Program.Database.AddVoteFor(index + 1, secondWinner, this.User);
                                }
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
                    int count = 0; //what would this count ?? - allows us to limit number of names to respond with (so we dont crash the network)
                    foreach (var student in Program.Database.AllStudents.Values)
                    {
                        bool shouldGo = false; // shouldGo: does the name match the query? if so, SHOULD we GO and send it
                        // yes i know its not best naming but /shrug
                        message = message.ToLower();
                        if(student.ToString().ToLower().StartsWith(message)) 
                        {
                            shouldGo = true;
                        }
                        else if (student.LastName.ToLower().StartsWith(message)) //?? - essentially looking to see if the name contains the query, ignoring any case
                        { // it is actually just returning an index of where the query string is within the student's name (same as like list.Indexof)
                            // the >=0 is because if it does not contain ^, then it returns -1 instead
                            shouldGo = true;
                        } else if(student.FirstName.ToLower().StartsWith(message))
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
                            response += student.ToString("AN:FN:LN:TT") + "#"; //add the student's name + properties to a list of names to send to the client
                        }
                    }
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
                    } catch (Exception ex)
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
                Logging.Log(Logging.LogSeverity.Warning, $"Listening to new connections at {((IPEndPoint)ServerListener.LocalEndpoint).Address.ToString()}:{((IPEndPoint)ServerListener.LocalEndpoint).Port}");
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
                    lock(LockClient)
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
                    }
                }
                catch (Exception ex)
                {
                    Logging.Log("NewConn", ex);
                }
            }
        }
    }
}

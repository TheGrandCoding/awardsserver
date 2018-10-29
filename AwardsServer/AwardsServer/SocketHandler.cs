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

        public class SocketConnection
        {
            public TcpClient Client;
            public string UserName; // can be different from AccountName, eg when same person joins twice
                                    // this will be randomly generated a suffix of 3 digits
            public User User;

            public bool Listening = true; // for the while loop below

            public string IPAddress;

            Thread listenThread;

            public SocketConnection(TcpClient client, string name)
            {
                Client = client;
                UserName = name;
                if(Program.TryGetUser(name, out User)) {
                    // nothing
                } else
                {
                    throw new ArgumentException("User not found: '" + name + "'");
                }
            }

            /// <summary>
            /// Indicates that it should begin to listen because it's been moved from the queue
            /// </summary>
            public void AcceptFromQueue()
            {
                Logging.Log(Logging.LogSeverity.Warning, "Bringing " + this.User.ToString() + " from queue.");
                Send("Ready:" + this.User.FirstName);
                Send("NumCat:" + Program.Database.AllCategories.Count);
                listenThread = new Thread(Listen);
                listenThread.Start();
            }

            private void HandleMessage(string message)
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
                    message = message.Replace("SUBMIT:", "");
                    string rejectedReason = "";
                    try
                    {
                        string[] cats = message.Split('#');
                        for(int index = 0; index < cats.Length; index++)
                        {
                            string[] catSplit = cats[index].Split(';');
                            string maleWin = catSplit[0];
                            string femaleWin = catSplit[1];
                            if(Program.TryGetUser(maleWin, out User target))
                            {
                                if(target.AccountName == this.User.AccountName)
                                {
                                    rejectedReason = "Rejected:Self";
                                } else
                                {
                                    Program.Database.AddVoteFor(index+1, target, this.User);
                                }
                            }
                            if(Program.TryGetUser(femaleWin, out User ftarget))
                            {
                                if (ftarget.AccountName == this.User.AccountName)
                                {
                                    rejectedReason = "Rejected:Self";
                                }
                                else
                                {
                                    Program.Database.AddVoteFor(index+1, ftarget, this.User);
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
                            Program.Database.AlreadyVotedNames.Add(this.User.AccountName);
                            this.Send("Accepted");
                            Logging.Log(Logging.LogSeverity.Warning, "User has voted", this.User.AccountName);
                        }
                        else
                        {
                            this.Send(rejectedReason);
                        }
                        this.Close("Submitted");
                    }
                } else if(message.StartsWith("QUERY"))
                {
                    message = message.Replace("QUERY:", "");
                    string response = "";
                    char sex = char.Parse(message.Substring(0, 1));
                    message = message.Substring(2);
                    int count = 0;
                    foreach (var student in Program.Database.AllStudents.Values)
                    {
                        if (student.Sex == sex)
                        {
                            bool shouldGo = false;
                            if(student.ToString().StartsWith(message))
                            {
                                shouldGo = true;
                            }
                            else if (student.ToString().IndexOf(message, StringComparison.OrdinalIgnoreCase) >= 0)
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
                                response += student.ToString("AN-FN-LN-TT") + "#";
                            }
                        }
                    }
                    this.Send("Q_RES:" + response);
                }
            }

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
                    {
                        if (ClientQueue.Count == 0)
                            break;
                        ClientQueue[0].AcceptFromQueue();
                        CurrentClients.Add(ClientQueue[0]);
                        ClientQueue.RemoveAt(0);
                    }
                }
            }

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

                        foreach(var tempMsg in data.Split('%'))
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

            public static void WriteConnection(TcpClient client, string message)
            {
                message = $"%{message}`";
                NetworkStream stream = client.GetStream();
                Byte[] broadcastBytes = Encoding.UTF8.GetBytes(message);
                stream.Write(broadcastBytes, 0, broadcastBytes.Length);
            }

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
                            {
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

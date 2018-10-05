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

        public class SocketConnection
        {
            public TcpClient Client;
            public readonly string UserName;
            public User User;

            public bool Listening = true;

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
                Thread tt = new Thread(Listen);
                tt.Start();
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
                } else if (message.StartsWith("SET_CATE"))
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
                }
            }

            private void Listen()
            {
                NetworkStream stream = Client.GetStream();
                while(Listening)
                {
                    try
                    {
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
                        Listening = false;
                    }
                }
            }

            public void Send(string message)
            {
                try
                {
                    message = $"%{message}`";
                    NetworkStream stream = this.Client.GetStream();
                    Byte[] broadcastBytes = Encoding.UTF8.GetBytes(message);
                    stream.Write(broadcastBytes, 0, broadcastBytes.Length);
                    Logging.Log( Logging.LogSeverity.Debug, message, $"{UserName}/Send");
                } catch (Exception ex)
                {
                    Logging.Log($"{UserName}/Send", ex);
                    Listening = false;
                }
    }
        }

        public bool Listening = false;
        // Handles listening to, recieving information from, and sending information to
        // any clients (ie, the programs) that attempt to communicate.
        private TcpListener ServerListener;

        public List<SocketConnection> Connections = new List<SocketConnection>();

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
                    var user = new SocketConnection(clientSocket, dataFromClient); // first message is username.
                    Logging.Log(Logging.LogSeverity.Warning, "New connection: " + ipEnd.Address.ToString() + " -> " + user.User.ToString(), "NewCon");
                    user.Send("Ready:" + user.User.FirstName);
                    user.Send("NumCat:" + Program.Database.AllCategories.Count);
                } catch (Exception ex)
                {
                    Logging.Log("NewConn", ex);
                }
            }
        }
    }
}

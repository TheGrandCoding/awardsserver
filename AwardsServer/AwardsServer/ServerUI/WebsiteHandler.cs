using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using AwardsServer.ServerUI;

namespace AwardsServer.ServerUI
{
    // Im going to try my best to comment this, Liliana..

    /// <summary>
    /// Handles all connections made via a browser to the website.
    /// </summary>
    public class WebsiteHandler
    {
        private static bool _started = false;
        private TcpListener WebServer;
        /// <summary>
        /// Sends the message to the client
        /// Static.. so its helpful
        /// </summary>
        public static void WriteClient(TcpClient client, string message)
        {
            message = $"%{message}`";
            NetworkStream stream = client.GetStream();
            stream.Flush();
            Byte[] broadcastBytes = Encoding.UTF8.GetBytes(message);
            stream.Write(broadcastBytes, 0, broadcastBytes.Length);
            stream.Flush();
        }

        /// <summary>
        /// Handles a connection's HTTP GET requests.
        /// </summary>
        private void HandleClientRequest(TcpClient client, IPEndPoint ipEnd, string request)
        {
            try
            {
                var newContext = new HTTPGetResponse(client, request);
                newContext.Execute();
                newContext.Client.Close(); // done.
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void ListenNewClients()
        {
            TcpClient clientSocket = new TcpClient();
            while(WebServer != null)
            {
                try
                {
                    clientSocket = WebServer.AcceptTcpClient();
                    Byte[] bytesFrom = new Byte[clientSocket.ReceiveBufferSize];
                    string dataFromClient;
                    NetworkStream netStream = clientSocket.GetStream();
                    try
                    {
                        netStream.Read(bytesFrom, 0, Convert.ToInt32(clientSocket.ReceiveBufferSize));
                    }
                    catch (Exception ex)
                    {
                        Logging.Log("Web-R", ex);
                        continue;
                    }
                    dataFromClient = Encoding.UTF8.GetString(bytesFrom).Trim().Replace("\0", "");
                    IPEndPoint ipEnd = clientSocket.Client.RemoteEndPoint as IPEndPoint;
                    if (string.IsNullOrWhiteSpace(dataFromClient))
                    {
                        WriteClient(clientSocket, "400 Bad Request");
                        clientSocket.Close();
                        continue;
                    }
                    if(dataFromClient.StartsWith("GET"))
                    { // HTTP requests may come in a few varities: we are only interested in GET requests
                        HandleClientRequest(clientSocket, ipEnd, dataFromClient);
                    } else
                    { // so, we error on any others
                        WriteClient(clientSocket, "501 - Not Implemented");
                    }
                } catch (Exception ex)
                {
                    Logging.Log("Web", ex);
                }
            }
        }

        public WebsiteHandler()
        {
            if (_started)
                throw new InvalidOperationException("Webserver has already been started");
            _started = true;
            WebServer = new TcpListener(IPAddress.Any, 80);
            WebServer.Start();
            Thread listen = new Thread(ListenNewClients);
            listen.Start();
        }

    }
}

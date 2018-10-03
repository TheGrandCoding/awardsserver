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
        // Handles listening to, recieving information from, and sending information to
        // any clients (ie, the programs) that attempt to communicate.
        private TcpListener ServerListener;

        public SocketHandler()
        {
            try
            {
                ServerListener = new TcpListener(IPAddress.Any, 56567);
                ServerListener.Start();
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
        }
    }
}

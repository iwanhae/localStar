using System.Net;
using System.Net.Sockets;
using System.Threading;
using System;
using localStar.Config;
using localStar.Logger;

namespace localStar.Server
{
    public static class HttpServer
    {
        static TcpListener Server = null;
        static Thread t = new Thread(handleClient);
        public static bool isActive { get => t.IsAlive; }

        public static Boolean Start()
        {
            Server = new TcpListener(new IPEndPoint(ConfigMgr.localHost, ConfigMgr.localPort));
            try
            {
                Server.Start();
                t.Start();
                return true;
            }
            catch (Exception e)
            {
                Log.error(e.ToString());
                return false;
            }
        }

        private static void handleClient()
        {
            Log.info("Waiting for Http Connection.....");
            while (true)
            {
                TcpClient client = Server.AcceptTcpClient();
                new Connection.ClientConnection(client);
            }
        }
    }
}
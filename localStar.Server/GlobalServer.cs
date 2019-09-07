using System.Net;
using System.Net.Sockets;
using System.Threading;
using System;
using localStar.Config;

namespace localStar.Server
{
    public static class GlobalServer
    {
        static TcpListener Server = null;
        static NetworkStream mainStream = null;
        static Thread t = new Thread(handleClient);

        public static void Start()
        {
            Server = new TcpListener(new IPEndPoint(ConfigMgr.host, ConfigMgr.globalPort));
            Server.Start();
            t.Start();
        }

        private static void handleClient()
        {
            while (true)
            {
                Console.WriteLine("Waiting for a Global Connection.....");
                TcpClient client = Server.AcceptTcpClient();
            }
        }
    }
}
using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;

using localStar.Connection;
using localStar.Nodes;
using localStar.Logger;
namespace localStar
{
    class Program
    {
        static int count = 0;
        static Thread DNSThread = new Thread(DNS.Service.Start);
        static void Main(string[] args)
        {
            Log.Init();
            Log.info("Reading Settings");
            localStar.Config.ConfigMgr.load();

            HandleLoop.Init();
            Server.NodeServer.Start();
            Server.HttpServer.Start();

            while (true)
            {
                Thread.Sleep(100);
                if (!Server.HttpServer.isActive) break;
                if (!Server.NodeServer.isActive) break;
            }
        }

        private static int pQueueLength = 0;
        private static void Test(object state)
        {
            if (count != 0) Log.debug("RequestPerSeconds: {0}", count);
            count = 0;
            int QueueLength = HandleLoop.getCount();

            if (pQueueLength != QueueLength) Log.debug("Current Connection: {0}", QueueLength);

            pQueueLength = QueueLength;
        }

        static void Test()
        {
        }
    }
}

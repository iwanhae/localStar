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
            Service s = new Service("iptime", new IPEndPoint(IPAddress.Parse("192.168.0.1"), 80));
            Service n = new Service("nginx", new IPEndPoint(IPAddress.Parse("192.168.1.1"), 3000));
            Service w = new Service("web", new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5008));
            ServiceConnectionManager.addService(s);
            ServiceConnectionManager.addService(n);
            ServiceConnectionManager.addService(w);

            Log.info("Initialinzing Worker Threads");
            HandleLoop.Init();

            Log.info("Listening on 0.0.0.0:8000");
            TcpListener tl = new TcpListener(IPAddress.Any, 8000);
            tl.Start();

            Timer t = new Timer(Test, null, 1000, 1000);

            while (true)
            {
                count++;
                TcpClient tc = tl.AcceptTcpClient();
                new ClientConnection(tc);
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

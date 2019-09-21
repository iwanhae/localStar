using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;

using localStar.Connection;
using localStar.Nodes;
namespace localStar
{
    class Program
    {
        static int count = 0;
        static Thread DNSThread = new Thread(DNS.Service.Start);
        static void Main(string[] args)
        {
            StreamPipe.Pipe.Init();
            Service s = new Service("iptime", new IPEndPoint(IPAddress.Parse("192.168.0.1"), 80));
            Service n = new Service("nginx", new IPEndPoint(IPAddress.Parse("192.168.1.1"), 3000));
            Service w = new Service("web", new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5008));
            ServiceConnectionManager.addService(s);
            ServiceConnectionManager.addService(n);
            ServiceConnectionManager.addService(w);

            TcpListener tl = new TcpListener(IPAddress.Any, 8000);
            tl.Start();

            Console.WriteLine("Press any key to stop server");

            Timer t = new Timer(Test, null, 1000, 1000);
            while (true)
            {
                count++;
                TcpClient tc = tl.AcceptTcpClient();
                new ClientConnection(tc);
            }
            // DNSThread.Start();
            Console.WriteLine("Press any key to stop server");
            Console.ReadLine();
        }

        private static void Test(object state)
        {
            Console.WriteLine(count);
            count = 0;
        }

        static void Test()
        {
        }
    }
}

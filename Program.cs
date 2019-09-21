using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace localStar
{
    class Program
    {
        static Thread DNSThread = new Thread(DNS.Service.Start);
        static void Main(string[] args)
        {
            Test();

            // DNSThread.Start();
            Console.WriteLine("Press any key to stop server");
            Console.ReadLine();
        }

        static void Test()
        {
        }
    }
}

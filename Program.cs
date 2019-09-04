using System;
using System.Threading;

namespace localStar
{
    class Program
    {
        static Thread DNSThread = new Thread(DNS.Service.Start);
        static void Main(string[] args)
        {
            DNSThread.Start();
            Console.WriteLine("Press any key to stop server");
            Console.ReadLine();
        }
    }
}

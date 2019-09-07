using Newtonsoft.Json;
using System;
using System.Net;

namespace localStar.Config
{
    public static class ConfigMgr
    {
        public static string nodeId { get; } = Environment.MachineName;
        public static IPAddress[] nameSevers { get; } = new IPAddress[] { new IPAddress(new byte[] { 8, 8, 8, 8 }) };
        public static IPAddress localPublicIP { get; } = new IPAddress(new byte[] { 127, 0, 0, 1 });


		public static IPAddress host { get; } = IPAddress.Any;
        public static int globalPort { get; } = 8081;
        public static int localPort { get; } = 8080;
        public static IPAddress globalPublicIP { get; } = new IPAddress(new byte[] { 127, 0, 0, 1 });
        public static IPEndPoint globalPublicEndPoint { get => new IPEndPoint(globalPublicIP, globalPort); }


        public static IPAddress dnsHost { get; } = IPAddress.Any;
        public static int dnsPort { get; } = 8053;
        public static IPEndPoint dnsEndPoint { get => new IPEndPoint(dnsHost, dnsPort); }
    }

}
using System;
using System.Net;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

using localStar.Logger;
using Newtonsoft.Json;

namespace localStar.Config
{
    public static class ConfigMgr
    {
        public static string nodeId { get; private set; } = Environment.MachineName;

        public static IPAddress localHost { get; private set; } = IPAddress.Parse("0.0.0.0");
        public static int localPort { get; private set; } = 9000;
        public static IPAddress localIp { get; private set; } = IPAddress.Parse("127.0.0.1");
        public static IPEndPoint localEndPoint { get => new IPEndPoint(localIp, localPort); }

        public static IPAddress globalHost { get; private set; } = IPAddress.Parse("0.0.0.0");
        public static int globalPort { get; private set; } = 9001;
        public static IPAddress globalIp { get; private set; } = IPAddress.Parse("127.0.0.1");
        public static IPEndPoint globalEndPoint { get => new IPEndPoint(globalIp, globalPort); }

        public static IPAddress dnsHost { get; private set; } = IPAddress.Parse("0.0.0.0");
        public static int dnsPort { get; private set; } = 9053;

        public static IPAddress[] nameservers { get; private set; } = new IPAddress[] { IPAddress.Parse("8.8.8.8"), IPAddress.Parse("8.8.4.4") };


        public static bool load(string path = @"./config.yaml")
        {
            if (!File.Exists(path))
            {
                Log.fatal("No Config File found. Check \"{0}\"", path);
                using (StreamWriter sw = new StreamWriter(path))
                {
                    sw.Write(Document);
                }
                return false;
            }
            Log.debug("Load Config file");

            string yaml = "";
            using (StreamReader sr = new StreamReader(path)) yaml = sr.ReadToEnd();

            LocalStarConfig config = new DeserializerBuilder().IgnoreUnmatchedProperties().Build().Deserialize<LocalStarConfig>(new StringReader(yaml));

            ConfigMgr.nodeId = config.nodeId;
            Log.info("{0}: {1}", "nodeId".PadRight(15, ' '), config.nodeId);

            ConfigMgr.localHost = IPAddress.Parse(config.localHost);
            ConfigMgr.localPort = config.localPort;
            ConfigMgr.localIp = IPAddress.Parse(config.localIp);
            Log.info("{0}: {1}", "localHost".PadRight(15, ' '), config.localHost);
            Log.info("{0}: {1}", "localPort".PadRight(15, ' '), config.localPort);
            Log.info("{0}: {1}", "localIp".PadRight(15, ' '), config.localIp);


            ConfigMgr.globalHost = IPAddress.Parse(config.globalHost);
            ConfigMgr.globalPort = config.globalPort;
            ConfigMgr.globalIp = IPAddress.Parse(config.globalIp);
            Log.info("{0}: {1}", "globalHost".PadRight(15, ' '), config.globalHost);
            Log.info("{0}: {1}", "globalPort".PadRight(15, ' '), config.globalPort);
            Log.info("{0}: {1}", "globalIp".PadRight(15, ' '), config.globalIp);

            ConfigMgr.dnsHost = IPAddress.Parse(config.dnsHost);
            ConfigMgr.dnsPort = config.dnsPort;
            Log.info("{0}: {1}", "dnsHost".PadRight(15, ' '), config.dnsHost);
            Log.info("{0}: {1}", "dnsPort".PadRight(15, ' '), config.dnsPort);

            {
                Log.info("nameservers :");
                List<IPAddress> tmp = new List<IPAddress>();
                foreach (string address in config.nameservers.addresses)
                {
                    tmp.Add(IPAddress.Parse(address));
                    Log.info("    {0}", address);
                }
                ConfigMgr.nameservers = tmp.ToArray();
            }

            foreach (var service in config.services ?? new List<ServiceConfig>())
            {
                Log.info("{0}: {1}", "Service".PadRight(15, ' '), service.name);
                Nodes.Healthcheck hc = service.healthcheck;
                foreach (var address in service.addresses)
                {
                    Nodes.Service tmp = new Nodes.Service(service.name, Tools.parseIPEndPoint(address, 80), healthcheck: hc);
                    Connection.ServiceConnectionManager.addService(tmp);
                    Log.info("    {0}:{1}", tmp.address.Address.ToString(), tmp.address.Port);
                }
            }

            foreach (var node in config.nodes ?? new List<NodeConfig>())
            {
                Log.info("{0}: {1} {2}", "TryConnecting to", node.name, node.address);
                Connection.NodeConnection nc = new Connection.NodeConnection(Tools.parseIPEndPoint(node.address, ConfigMgr.globalPort));
            }
            return true;
        }
        private const string Document = @"---
        version: '3'

        nodeId: server-1

        localHost: 0.0.0.0
        localPort: 9000
        localIp: 127.0.0.1  # 내부 프로그램이 이 노드로 접속할 때 사용할 주소

        globalHost: 0.0.0.0
        globalPort: 9001  # Node 끼리 통신하는데 사용할 포트
        globalIp : 175.193.162.44 # 외부 노드에서 이 노드로 접속할때 사용할 주소

        dnsHost: 0.0.0.0
        dnsPort: 9053

        options:
        force-match-node-name: false

        nameservers:
        addresses:
            - 8.8.8.8
            - 8.8.4.4

        services:
        - name: nginx
            addresses:
            - 127.0.0.1:5000
            - 127.0.0.1:5001
            healthcheck:
            method: GET
            interval: 10 # sec
            timeout: 10 # sec
            retries: 3

        nodes:
        - name: server1  # not important
            address: 175.192.168.44:9001
        ...";
    }

}
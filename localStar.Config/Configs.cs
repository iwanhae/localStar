using System.Collections.Generic;
namespace localStar.Config
{

    public class LocalStarConfig
    {
        public string nodeId { get; set; }

        public string localHost { get; set; }
        public int localPort { get; set; }
        public string localIp { get; set; }

        public string globalHost { get; set; }
        public int globalPort { get; set; }
        public string globalIp { get; set; }

        public string dnsHost { get; set; }
        public int dnsPort { get; set; }

        public NameserverConfig nameservers { get; set; }
        public List<ServiceConfig> services { get; set; }
        public List<NodeConfig> nodes { get; set; }
    }
    public class ServiceConfig
    {
        public string name { get; set; }
        public List<string> addresses { get; set; }
        public Nodes.Healthcheck healthcheck { get; set; }
    }
    public class NodeConfig
    {
        public string name { get; set; }
        public string address { get; set; }
    }
    public class NameserverConfig
    {
        public List<string> addresses { get; set; }
    }
}
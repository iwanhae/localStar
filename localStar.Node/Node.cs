using System.Net;
using System.Collections.Generic;
using System;
using System.Runtime.Serialization;
using localStar.Config;

namespace localStar.Node
{
    public class Node : ISerializable
    {
        // id of this node
        public String id { get; }
        // network delay from parent to this node
        public int delay { get; set; } = 0;
        // IP and Port to connect this node
        public IPEndPoint address { get; }

        // Comment: 이름과 내용.
        public Dictionary<string, Node> ConnectedNode = new Dictionary<string, Node>();
        public Dictionary<string, Service> ConnectedService = new Dictionary<string, Service>();

        public List<Node> getConnectedNode()
        {
            var list = new List<Node>();
            foreach (var pair in ConnectedNode) list.Add(pair.Value);
            return list;
        }

        public bool setNodeDelay(string id, int delay)
        {
            Node val;
            if (ConnectedNode.TryGetValue(id, out val))
            {
                ConnectedNode[id].setDelay(delay);
                return true;
            }
            else return false;
        }
        public void setDelay(int delay)
        {
            this.delay = delay;
        }
        public bool addConnectedNode(Node node, int delay)
        {
            if (ConnectedNode.ContainsKey(node)) return false;
            ConnectedNode.Add(node, delay);
            return true;
        }
        public Node(String id, IPEndPoint address)
        {
            this.id = id;
            this.address = address;
        }
        public Node(String id, IPAddress address)
            : this(id, new IPEndPoint(address, ConfigMgr.globalPort)) { }

        public Node(String id, string address)
            : this(id, IPAddress.Parse(address)) { }


        public override string ToString() { return id; }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!this.GetType().Equals(obj.GetType())) return false;
            Node n = (Node)obj;
            if (!this.id.Equals(n.id)) return false;
            return true;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("id", id);
            info.AddValue("delay", delay);

            info.AddValue("ip", address.Address.GetAddressBytes());
            info.AddValue("port", address.Port);
            info.AddValue("ConnectedNode", ConnectedNode);
            info.AddValue("ConnectedService", ConnectedService);
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public Node(SerializationInfo info, StreamingContext context)
        {
            id = info.GetString("id");
            delay = info.GetInt32("delay");
            IPAddress ip = new IPAddress((byte[])info.GetValue("ip", typeof(byte[])));
            address = new IPEndPoint(ip, info.GetInt32("port"));
            ConnectedNode = info.GetValue("ConnectedNode", typeof(SortedDictionary<Node, int>)) as SortedDictionary<Node, int>;
            ConnectedService = info.GetValue("ConnectedService", typeof(SortedDictionary<string, Service>)) as SortedDictionary<string, Service>;
        }
    }
}
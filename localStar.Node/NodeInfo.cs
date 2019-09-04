using System.Net;
using System.Collections.Generic;
using System;
using System.Runtime.Serialization;
using localStar.Config;

namespace localStar.Node
{
    public class NodeInfo : ISerializable
    {
        public String id { get; }
        public IPEndPoint address { get; }

        // Comment: Nodeinfo=노드정보, int=커넥션 속도(ms)
        public SortedDictionary<NodeInfo, int> ConnectedNode = new SortedDictionary<NodeInfo, int>();
        public SortedDictionary<string, ServiceInfo> ConnectedService = new SortedDictionary<string, ServiceInfo>();

        public bool setNodeDelay(NodeInfo node, int delay)
        {
            int val;
            if (ConnectedNode.TryGetValue(node, out val))
            {
                ConnectedNode[node] = val;
                return true;
            }
            else return false;
        }
        public bool addConnectedNode(NodeInfo node, int delay)
        {
            if (ConnectedNode.ContainsKey(node)) return false;
            ConnectedNode.Add(node, delay);
            return true;
        }
        public NodeInfo(String id, IPEndPoint address)
        {
            this.id = id;
            this.address = address;
        }
        public NodeInfo(String id, IPAddress address)
            : this(id, new IPEndPoint(address, ConfigMgr.port)) { }

        public NodeInfo(String id, string address)
            : this(id, IPAddress.Parse(address)) { }


        public override string ToString() { return id; }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!this.GetType().Equals(obj.GetType())) return false;
            NodeInfo n = (NodeInfo)obj;
            if (!this.id.Equals(n.id)) return false;
            return true;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("id", id);
            info.AddValue("ip", address.Address.GetAddressBytes());
            info.AddValue("port", address.Port);
            info.AddValue("ConnectedNode", ConnectedNode);
            info.AddValue("ConnectedService", ConnectedService);
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public NodeInfo(SerializationInfo info, StreamingContext context)
        {
            id = info.GetString("id");
            IPAddress ip = new IPAddress((byte[])info.GetValue("ip", typeof(byte[])));
            address = new IPEndPoint(ip, info.GetInt32("port"));
            ConnectedNode = info.GetValue("ConnectedNode", typeof(SortedDictionary<NodeInfo, int>)) as SortedDictionary<NodeInfo, int>;
            ConnectedService = info.GetValue("ConnectedService", typeof(SortedDictionary<string, ServiceInfo>)) as SortedDictionary<string, ServiceInfo>;
        }
    }
}
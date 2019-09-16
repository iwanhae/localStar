using System.Net;
using System.Collections.Generic;
using System;
using System.Runtime.Serialization;
using localStar.Config;

namespace localStar.Node
{
    [Serializable]
    public class Node : ISerializable
    {
        // id of this node
        public String id { get; }
        // network delay from parent to this node
        public int delay { get; set; } = 0;
        // IP and Port to connect this node
        public IPEndPoint address { get; }

        // Comment: 이름과 내용.
        private Dictionary<string, Node> ConnectedNode = new Dictionary<string, Node>();
        private Dictionary<string, Service> ConnectedService = new Dictionary<string, Service>();
        private Dictionary<string, Index> indexList = new Dictionary<string, Index>();

        public Node shortestPathTo(string serviceId)    // Node의 경우 자기 자신을 이름으로 가지는 Service를 따로 등록하므로 Node도 탐색 가능.
        {
            Index index;
            if (indexList.TryGetValue(serviceId, out index))
                return ConnectedNode[index.nodeId];
            else return null;
        }
        public void refreshIndex()
        {
            Dictionary<string, Index> indexList = new Dictionary<string, Index>();
            Index tmp;
            foreach (var pair in ConnectedService)
            {
                if (indexList.TryGetValue(pair.Key, out tmp))
                {
                    if (pair.Value.delay < tmp.delay)
                        indexList.Add(pair.Key, new Index(pair.Key, this.id, pair.Value.delay));
                }
                else
                    indexList.Add(pair.Key, new Index(pair.Key, this.id, pair.Value.delay));
            }
            foreach (var node in ConnectedNode.Values)
            {
                node.refreshIndex();
                foreach (var pair in node.indexList)
                    if (indexList.TryGetValue(pair.Key, out tmp))
                    {
                        if (pair.Value.delay + node.delay < tmp.delay)
                            indexList.Add(pair.Key, new Index(pair.Key, node.id, pair.Value.delay + node.delay));
                    }
                    else
                        indexList.Add(pair.Key, new Index(pair.Key, node.id, pair.Value.delay + node.delay));
            }
            this.indexList = indexList;
        }
        public void removeFromAllChildNodes(Node node) => removeFromAllChildNodes(node.id);
        public void removeFromAllChildNodes(string NodeId)
        {
            ConnectedNode.Remove(NodeId);
            foreach (var pair in ConnectedNode)
                pair.Value.removeFromAllChildNodes(NodeId);
        }
        public List<Node> getConnectedNode()
        {
            var list = new List<Node>();
            foreach (var pair in ConnectedNode) list.Add(pair.Value);
            return list;
        }
        public bool addConnectedNode(Node node, int delay)
        {
            if (ConnectedNode.ContainsKey(node.id)) return false;
            ConnectedNode.Add(node.id, node);
            return true;
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
        public Node(String id, IPEndPoint address)
        {
            this.id = id;
            this.address = address;
        }
        public Node(String id, IPAddress address)
            : this(id, new IPEndPoint(address, ConfigMgr.globalPort)) { }

        public Node(String id, string address)
            : this(id, IPAddress.Parse(address)) { }


        public override string ToString() { return id + ": delay " + delay + "ms"; }

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
            ConnectedNode = (Dictionary<string, Node>)info.GetValue("ConnectedNode", typeof(Dictionary<string, Node>));
            ConnectedService = (Dictionary<string, Service>)info.GetValue("ConnectedService", typeof(Dictionary<string, Service>));
        }
    }
}
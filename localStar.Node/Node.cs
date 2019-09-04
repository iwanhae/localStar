using System;
using System.Net;
using System.Collections.Generic;
using localStar.Config;

namespace localStar.Node
{
    public class Node : NodeInfo
    {
        public int delay { get; set; } = 0;
        public bool addConnectedNode(Node node, int delay) => addConnectedNode(node, delay);
        public List<Node> getConnectedNode()
        {
            List<Node> result = new List<Node>();
            foreach (var pair in this.ConnectedNode)
                result.Add(new Node(pair.Key, pair.Value));
            return result;
        }

        public Node(String id, IPEndPoint address)
            : base(id, address) { }
        public Node(String id, IPAddress address)
            : this(id, new IPEndPoint(address, ConfigMgr.port)) { }

        public Node(String id, string address)
            : this(id, IPAddress.Parse(address)) { }

        public Node(NodeInfo node)
            : this(node.id, node.address) { }

        public Node(NodeInfo node, int delay)
            : this(node.id, node.address) { this.delay = delay; }

    }
}
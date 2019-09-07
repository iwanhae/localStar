using System.Net;
using System.Collections.Generic;
using System;
using System.Runtime.Serialization;
using localStar.Config;

namespace localStar.Node
{
    public static class NodeManager
    {
        static Node CurrentNode = new Node(ConfigMgr.nodeId, ConfigMgr.globalPublicEndPoint);
        static SortedDictionary<String, Node> KnownNodes = new SortedDictionary<String, Node>();
        static SortedDictionary<String, Node> ConnectedNodes = new SortedDictionary<String, Node>();
        static SortedDictionary<String, Node> KnowServices = new SortedDictionary<String, Node>();
        static SortedDictionary<String, Node> ConnectedServices = new SortedDictionary<String, Node>();

        public static byte[] getShareInfo()
        {
            var stream = new System.IO.MemoryStream();
            var bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            var obj = new Dictionary<String, Node>();
            foreach (var d in KnownNodes) obj.Add(d.Key, d.Value);

            bf.Serialize(stream, obj);
            stream.Flush();
            return stream.ToArray();
        }
        public static bool addNode(Node node)
        {
            KnownNodes.Add(node.id, node);
            return true;
        }
        public static Node getCurrentNode()
        {
            return (Node)CurrentNode;
        }
        public static Node getNodeById(string id)
        {
            Node tmp;
            if (KnownNodes.TryGetValue(id, out tmp)) return tmp;
            else return null;
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace localStar.Node
{
    static class Tools
    {
        public static Node getNodeFromBytes(byte[] bytes)
        {
            var fm = new BinaryFormatter();
            Stream stream = new MemoryStream(bytes);
            return (Node)fm.Deserialize(stream);
        }
        public static byte[] getBytesFromNode(Node node) => getStream(node).ToArray();
        public static byte[] getBytesFromNode(String nodeId) => getBytesFromNode(NodeManager.getNodeById(nodeId));
        public static MemoryStream getStream(Object obj)
        {
            var stream = new MemoryStream();
            var bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            bf.Serialize(stream, obj);
            return stream;
        }

    }
}

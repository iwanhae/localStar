using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace localStar.Nodes
{
    static class Tools
    {
        /// <summary>
        /// URL로부터 찾아봐야할 ServiceId를 Return해줌.
        /// </summary>
        /// <param name="URL">.service .node로 끝나는 주소</param>
        /// <returns></returns>
        public static string getServiceIdFromUrl(string URL)
        {
            Uri uri = new Uri(URL);
            string[] url = uri.Host.ToLower().Split('.');
            int last = url.Length - 1;
            if (string.Compare(url[last], "service") == 0)
            {
                return url[last - 1];   // ${serviceId}.service
            }
            else if (string.Compare(url[last], "node") == 0)
            {
                if (url[last - 1] == NodeManager.getCurrentNode().id)
                {
                    if (url.Length == 4)
                        return url[last - 3]; // ${serviceId}.service.${nodeId}.node
                    else
                        return url[last - 1]; // ${nodeId}.node
                }
                else return url[last - 1]; // 이 노드를 찾는것이 아님.
            }
            return null;
        }
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

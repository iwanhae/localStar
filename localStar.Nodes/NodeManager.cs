using System.Net;
using System.Collections.Generic;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using localStar.Config;
using localStar.Connection;

namespace localStar.Nodes
{
    public static class NodeManager
    {
        static Node CurrentNode = new Node(ConfigMgr.nodeId, ConfigMgr.globalEndPoint);

        /// <summary>
        /// 해당 URL로 가기위해 어느 localId로 가야하는지 알려줌
        /// 못찾으면 null return
        /// </summary>
        /// <param name="URL">.service 혹은 .node로 끝나는 주소</param>
        /// <returns>localId</returns>
        public static IConnection shortestPathTo(string URL)
        {
            string serviceName = Tools.getServiceNameFromUrl(URL);
            if (serviceName == null) return null;

            Node node = CurrentNode.shortestPathTo(serviceName);

            if (node == null)
            {
                return null;
            }
            else if (node.id == CurrentNode.id) // 목적지는 ServiceConnectionManager 에서 찾아야함.
            {
                try
                {
                    return ServiceConnectionManager.getConnection(serviceName);
                }
                catch
                {
                    return null;
                }
            }
            else // 목적지는 NodeConnectionManager 에서 찾아야함.
            {
                try
                {
                    return NodeConnectionManager.getConnection(node.id);
                }
                catch
                {
                    return null;
                }
            }
        }

        public static bool addConnectedNode(Node node)
        {
            CurrentNode.addConnectedNode(node, node.delay);
            CurrentNode.refreshIndex();
            return true;
        }
        public static Node getCurrentNode()
        {
            return (Node)CurrentNode;
        }

        public static Node getNodeById(string nodeId)
        {
            return CurrentNode.getNodebyId(nodeId);
        }

        public static int setNodeDelay(string nodeId, int delay)
        {
            Node tmp = getNodeById(nodeId);
            if (tmp != null) tmp.delay = delay;
            else return -1;
            return delay;
        }
    }
}
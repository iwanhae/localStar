using System;
using System.Threading;
using System.IO;
using System.Net;
using System.Net.Sockets;
using localStar.Structure;
using localStar.Nodes;
using System.Linq;
using System.Collections.Generic;
using localStar.StreamPipe;

namespace localStar.Connection
{
    public static class NodeConnectionManager
    {
        static SortedDictionary<string, NodeConnection> Connections = new SortedDictionary<string, NodeConnection>();

        public static NodeConnection getConnection(string nodeId)
        {
            NodeConnection nodeConnection;
            if (!Connections.TryGetValue(nodeId, out nodeConnection)) return null;
            return nodeConnection;
        }
        public static void addNodeConnection(NodeConnection nodeConnection)
        {
            Connections[nodeConnection.nodeId] = nodeConnection;
        }
        public static void removeNodeConnection(NodeConnection nodeConnection)
        {
            NodeConnection tmp;
            if (Connections.TryGetValue(nodeConnection.nodeId, out tmp))
            {
                Connections.Remove(nodeConnection.nodeId);
            }
        }
    }
}
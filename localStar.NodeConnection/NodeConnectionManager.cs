/*
using System.Collections.Generic;
using localStar.Nodes;

namespace localStar.NodeConnection
{
    public static class NodeConnectionManager
    {
        /// <typeparam name="ushort">localId</typeparam>
        /// <typeparam name="NodeConnection">Connection</typeparam>
        static SortedDictionary<ushort, NodeConnection> NodeConnections = new SortedDictionary<ushort, NodeConnection>();

        public static ushort getLocaId(Node node)
        {
            foreach (var conn in NodeConnections.Values)
                if (conn.ConnectedNode.id == node.id) return conn.localId;

            return 0;
        }
        private static void addConnection(NodeConnection connection) => NodeConnections[connection.localId] = connection;
        public static void addConnection(System.Net.IPEndPoint address) => addConnection(new NodeConnection(address));
        public static void addConnection(System.Net.Sockets.TcpClient tc) => addConnection(new NodeConnection(tc));

    }
}
 */
using System.Collections.Generic;
using localStar.Connection.Stream;

namespace localStar.Connection
{
    public static class NodeConnectionManager
    {
        /// <typeparam name="ushort">localId</typeparam>
        /// <typeparam name="NodeConnection">Connection</typeparam>
        /// <returns></returns>
        static SortedDictionary<ushort, NodeConnection> NodeConnections = new SortedDictionary<ushort, NodeConnection>();

        private static void addConnection(NodeConnection connection) => NodeConnections[connection.localId] = connection;
        public static void addConnection(System.Net.IPEndPoint address) => addConnection(new NodeConnection(address));
        public static void addConnection(System.Net.Sockets.TcpClient tc) => addConnection(new NodeConnection(tc));

    }
}
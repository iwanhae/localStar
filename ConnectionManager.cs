using System.Collections.Generic;

using localStar.Connection;
namespace localStar
{
    public static class ConnectionManger
    {
        private static Dictionary<ushort, IConnection> dict = new Dictionary<ushort, IConnection>();

        public static IConnection GetConnection(ushort localId)
        {
            return dict[localId];
        }
        public static void Register(IConnection connection)
        {
            dict[connection.localId] = connection;
        }

        public static void DeRegister(IConnection connection)
        {
            dict.Remove(connection.localId);
        }
    }
}
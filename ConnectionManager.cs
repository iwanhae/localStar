using System;
using System.Threading;
using localStar.Connection;
using System.Collections.Generic;
using System.Collections.Concurrent;
namespace localStar
{
    public static class ConnectionManger
    {
        private static ConcurrentDictionary<int, IConnection> dict = new ConcurrentDictionary<int, IConnection>();

        public static IConnection GetConnection(int localId)
        {
            return dict[localId];
        }
        public static void Register(IConnection connection)
        {
            if (dict.ContainsKey(connection.localId))
            {
                Console.WriteLine(connection.localId);
            }
            dict.TryAdd(connection.localId, connection);
        }

        public static void DeRegister(IConnection connection)
        {
            dict.TryRemove(connection.localId, out _);
        }
    }
}
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
    public static class ServiceConnectionManager
    {
        static SortedDictionary<string, ServiceList> availableService = new SortedDictionary<string, ServiceList>();
        static SortedDictionary<int, ServiceConnection> Connections = new SortedDictionary<int, ServiceConnection>();

        public static void deRegisterConnection(ServiceConnection connection)
        {
            // Connections.Remove(connection.localId);
        }
        public static void registerConnection(ServiceConnection connection)
        {
            // Connections[connection.localId] = connection;
        }

        public static ServiceConnection getConnection(string serviceName)
        {
            ServiceList tmp;
            if (!availableService.TryGetValue(serviceName, out tmp)) return null;

            return new ServiceConnection(tmp.getOne());
        }
        public static void addService(Service service)
        {
            ServiceList tmp;
            if (availableService.TryGetValue(service.name, out tmp))
            {
                tmp.addService(service);
            }
            else
            {
                tmp = new ServiceList();
                tmp.addService(service);
                availableService[service.name] = tmp;
            }
            syncServiceListWithNode();
        }
        public static void removeService(Service service)
        {
            ServiceList tmp;
            if (availableService.TryGetValue(service.name, out tmp))
            {
                tmp.removeService(service);
                if (tmp.Length == 0) availableService.Remove(service.name);
            }
            syncServiceListWithNode();
        }
        private static void syncServiceListWithNode()
        {
            Dictionary<string, ServiceList> dict = new Dictionary<string, ServiceList>();
            foreach (var pair in availableService)
            {
                dict[pair.Key] = pair.Value;
            }
            NodeManager.getCurrentNode().setServiceList(dict);
        }
    }
}
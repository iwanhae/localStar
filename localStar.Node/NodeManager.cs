using System.Net;
using System.Collections.Generic;
using System;
using System.Runtime.Serialization;
using localStar.Config;

namespace localStar.Node
{
    public static class NodeManager
    {
        public static Node CurrentNode = new Node(ConfigMgr.nodeId, ConfigMgr.globalPublicEndPoint);
        public static List<Node> KnownNodes = new List<Node>();
        public static List<Node> ConnectedNodes = new List<Node>();
        public static List<Service> KnowServices = new List<Service>();
        public static List<Service> ConnectedServices = new List<Service>();
    }
}
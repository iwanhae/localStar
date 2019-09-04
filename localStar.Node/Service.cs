using System;
using System.Net;
using System.Collections.Generic;
using localStar.Config;

namespace localStar.Node
{
    public class Service : ServiceInfo
    {
        public Service(string name, IPEndPoint address, NodeInfo Parent = null, int delay = 0)
            : base(name, address, Parent, delay) { }
    }
}
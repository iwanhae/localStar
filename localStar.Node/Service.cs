using System.Net;
using System.Collections.Generic;
using System;
using System.Runtime.Serialization;
using localStar.Config;

namespace localStar.Node
{
    public class Service : ISerializable
    {
        public string name { get; }
        public IPEndPoint address { get; }
        public int delay { get; set; } //서비스가 소속된 노드와 서비스간의 지연
        public Node Parent { get; set; }

        public Service(string name, IPEndPoint address, Node Parent = null, int delay = 0){
            this.name = name;
            this.address = address;
            this.Parent = Parent;
            this.delay = delay;
        }

        public void setDelay(int delay) => this.delay = delay;

        public void setParent(Node Parent) => this.Parent = Parent;


        public override string ToString() { return Parent.ToString() + "/" + name; }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!this.GetType().Equals(obj.GetType())) return false;
            Service n = (Service)obj;
            if (!this.name.Equals(n.name)) return false;
            if (!this.Parent.Equals(n.Parent)) return false;
            return true;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("name", name);
            info.AddValue("ip", address.Address.GetAddressBytes());
            info.AddValue("port", address.Port);
            info.AddValue("delay", delay);
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public Service(SerializationInfo info, StreamingContext context)
        {
            name = info.GetString("name");
            IPAddress ip = new IPAddress((byte[])info.GetValue("ip", typeof(byte[])));
            address = new IPEndPoint(ip, info.GetInt32("port"));
            delay = info.GetInt32("delay");
        }
    }
}
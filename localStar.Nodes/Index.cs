using System;
using System.Collections;

namespace localStar.Nodes
{
    class Index : IComparable
    {
        public string name { get; }
        public string nodeId { get; }
        public int delay { get; }

        public Index(string name, string nodeId, int delay)
        {
            this.name = name;
            this.nodeId = nodeId;
            this.delay = delay;
        }

        public int CompareTo(object obj)
        {
            if (obj == null && obj.GetType() != typeof(Index)) throw new NotSupportedException();
            Index tmp = (Index)obj;
            if (tmp.name != this.name) throw new NotSupportedException();

            return tmp.delay < this.delay ? 1 : -1;
        }
    }
}
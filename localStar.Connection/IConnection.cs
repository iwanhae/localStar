using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using localStar.Structure;

namespace localStar.Connection
{
    public interface IConnection : IComparable
    {
        int localId { get; }
        void Send(Message message);
        void Close();
    }
    public abstract class Connection : IConnection, IDisposable
    {
        public int localId { get => this.GetHashCode(); }
        protected TcpClient tcpClient;


        public abstract void Send(Message message);
        public abstract void Close();

        public virtual void Dispose()
        { }

        public int CompareTo(object obj)
        {
            IConnection tmp = obj as IConnection;
            return this.localId < tmp.localId ? -1 : tmp.localId == this.localId ? 0 : 1;
        }
    }
}
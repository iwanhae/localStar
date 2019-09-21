using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using localStar.Structure;

namespace localStar.Connection
{
    public interface IConnection
    {
        int localId { get; }
        void Send(Message message);
        void Close();
    }
    public abstract class Connection : IConnection, IDisposable
    {
        public int localId { get => this.GetHashCode(); }
        protected Thread readThread;
        protected TcpClient tcpClient;


        protected abstract void handleRead();
        protected virtual void registerConnection()
        {
            ConnectionManger.Register(this);
        }

        protected virtual void deRegisterConnection()
        {
            ConnectionManger.DeRegister(this);
        }
        public abstract void Send(Message message);
        public abstract void Close();

        public virtual void Dispose()
        {
            try
            {
                readThread.Interrupt();
            }
            catch { }
        }
    }
}
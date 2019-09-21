using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using localStar.Structure;

namespace localStar.Connection
{
    public interface IConnection
    {
        ushort localId { get; }
        void Send(Message message);
        void Close();
    }
    public abstract class Connection : IConnection, IDisposable
    {
        public ushort localId { get => (ushort)((IPEndPoint)tcpClient.Client.RemoteEndPoint).Port; }
        protected Thread readThread;
        protected TcpClient tcpClient;


        protected abstract void handleRead();
        private void registerConnection()
        {
            ConnectionManger.Register(this);
        }
        public abstract void Send(Message message);
        public void Close()
        {
            ConnectionManger.DeRegister(this);
            this.Dispose();
        }

        public void Dispose()
        {
            readThread.Interrupt();
        }
    }
}
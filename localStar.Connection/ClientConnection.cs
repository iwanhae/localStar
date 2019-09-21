using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;
using localStar;
using localStar.Structure;
using localStar.Nodes;

namespace localStar.Connection
{
    public class ClientConnection : Connection
    {
        private HttpHeader header;
        private IConnection connection;
        private ushort localTo = 0;

        public string destinedService = "";
        public ClientConnection(TcpClient tcpClient)
        {
            this.tcpClient = tcpClient;
            try { readHeader(tcpClient); }
            catch { return; }

            ushort? localTo = NodeManager.shortestPathTo(destinedService);
            if (localTo == null)
            {
                header.closeWithNotFound();
                return;
            }
            else { this.localTo = (ushort)localTo; }

            ConnectionManger.Register(this);
            try
            {
                this.connection = ConnectionManger.GetConnection(this.localTo);
            }
            catch { header.closeWithError(); return; }

            Message message = new Message
            {
                LocalFrom = this.localId,
                LocalTo = this.localTo,
                Type = MessageType.NewConnection,
                URL = this.destinedService,
                data = header.getBytes(),
            };


            readThread = new Thread(handleRead);
            readThread.Start();
        }
        private void readHeader(TcpClient tcpClient)
        {
            HttpHeader header = new HttpHeader(tcpClient.GetStream());
            try
            {
                if (!header.isValid) throw new Exception();
                destinedService = header.KVSet["Host"];
                if (!header.KVSet.ContainsKey("Localstar-Through")) header.KVSet["Localstar-Through"] = "";
                header.KVSet["Localstar-Through"] += "," + Config.ConfigMgr.nodeId;
            }
            catch
            {
                throw new Exception("Can Not Parse Header");
            }
            this.header = header;
        }
        protected override void handleRead()
        {

        }
        public override void Send(Message message)
        {
            throw new System.NotImplementedException();
        }
    }
}
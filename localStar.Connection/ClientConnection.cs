using localStar.Nodes;
using localStar.Structure;
using System;
using System.Net.Sockets;
using System.Threading;

namespace localStar.Connection
{
    public class ClientConnection : Connection
    {
        private IConnection connection;
        private int localTo = 0;
        private bool onClosing = false;

        public string destinedService = "";
        public ClientConnection(TcpClient tcpClient)
        {
            HttpHeader header;

            this.tcpClient = tcpClient;
            try { header = readHeader(tcpClient); }
            catch { return; }

            connection = NodeManager.shortestPathTo(destinedService);
            if (connection == null)
            {
                header.closeWithNotFound();
                return;
            }

            Message message = new Message
            {
                From = this,
                Type = MessageType.NewConnection,
                URL = this.destinedService,
                data = header.getBytes(),
            };

            connection.Send(message);

            readThread = new Thread(handleRead);
            readThread.Start();
        }
        protected async override void handleRead()
        {
            try
            {
                NetworkStream stream = tcpClient.GetStream();
                stream.ReadTimeout = 1000;
                byte[] buffer = new byte[ushort.MaxValue];
                while (tcpClient.Connected)
                {
                    int len = await stream.ReadAsync(buffer);
                    if (len != 0)
                    {
                        byte[] tmp = new byte[len];
                        Buffer.BlockCopy(buffer, 0, tmp, 0, len);
                        connection.Send(new Message
                        {
                            From = this,
                            Type = MessageType.NormalConnection,
                            data = tmp
                        });
                    }
                    else
                    {
                        sendConnectionEnd();
                        break;
                    }
                }
            }
            catch
            {
                try
                {
                    sendConnectionEnd();
                }
                catch { }
            }
            Close();
        }
        private HttpHeader readHeader(TcpClient tcpClient)
        {
            HttpHeader header = new HttpHeader(tcpClient.GetStream());
            try
            {
                if (!header.isValid) throw new Exception();
                destinedService = header.KVSet["Host"].Split(':')[0];
                if (!header.KVSet.ContainsKey("Localstar-Through")) header.KVSet["Localstar-Through"] = "";
                header.KVSet["Localstar-Through"] += "," + Config.ConfigMgr.nodeId;
            }
            catch
            {
                throw new Exception("Can Not Parse Header");
            }
            return header;
        }

        public override void Send(Message message)
        {
            if (message.Type == MessageType.EndConnection)
            {
                onClosing = true;
                try
                {
                    tcpClient.GetStream().Write(new byte[0], 0, 0);
                    tcpClient.Close();
                }
                finally
                {
                    Close();
                }
            }
            else
            {
                try { tcpClient.GetStream().Write(message.data); }
                catch { sendConnectionEnd(); Close(); }
            }
        }
        public override void Close()
        {
            if (tcpClient.Connected)
            {
                try { tcpClient.GetStream().Write(new byte[0], 0, 0); }
                catch { }
                try { tcpClient.Close(); }
                catch { }
            }
            if (readThread.ThreadState == ThreadState.Running)
            {
                try { readThread.Interrupt(); }
                catch { }
            }
        }
        private void sendConnectionEnd()
        {
            if (!onClosing)
            {
                try
                {
                    connection.Send(new Message
                    {
                        From = this,
                        Type = MessageType.EndConnection,
                        data = new byte[0]
                    });
                }
                catch { }
            }
        }
    }
}
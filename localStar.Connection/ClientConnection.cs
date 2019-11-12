using localStar.Nodes;
using localStar.Structure;
using localStar.Logger;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text;

namespace localStar.Connection
{
    public class ClientConnection : Connection
    {
        public string destinedService = "";

        private IConnection connection;
        private bool onClosing = false;
        private Task<int> readTask = null;
        private byte[] buffer = new byte[ushort.MaxValue];
        private DateTime lastActivity = DateTime.Now;
        public ClientConnection(TcpClient tcpClient)
        {
            HttpHeader header;

            this.tcpClient = tcpClient;
            try { header = readHeader(tcpClient); }
            catch { return; }

            Log.debug("Client {0} : New Connection, Routing to {1}", this.localId, destinedService);

            connection = NodeManager.shortestPathTo(destinedService);
            if (connection == null)
            {
                Log.error("Client {0} : Can not make new connection to {1}", this.localId, destinedService);
                header.closeWithNotFound();
                return;
            }

            Message message = new Message
            {
                From = this,
                Type = MessageType.NewConnection,
                URL = this.destinedService,
                data = Encoding.UTF8.GetBytes(this.destinedService),
            };

            connection.Send(message);

            message = new Message
            {
                From = this,
                Type = MessageType.NormalConnection,
                URL = this.destinedService,
                data = header.getBytes(),
            };
            connection.Send(message);

            HandleLoop.addJob(handleRead);

        }
        public JobStatus handleRead()
        {
            try
            {
                if (!tcpClient.Connected) return JobStatus.Failed;
                else if (readTask == null)
                {
                    readTask = tcpClient.GetStream().ReadAsync(this.buffer).AsTask();
                    return JobStatus.Pending;
                }
                else if (readTask.IsCompleted)
                {
                    int len = readTask.Result;
                    readTask = tcpClient.GetStream().ReadAsync(this.buffer).AsTask();

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
                        lastActivity = DateTime.Now;
                        return JobStatus.Good;
                    }
                    else
                    {
                        Log.debug("Client {0} : Client send 0 length data. Connection closed", this.localId);
                        sendConnectionEnd();
                        Close();
                        return JobStatus.Failed;
                    }
                }
                else
                {
                    if (readTask.IsFaulted || readTask.IsCanceled)
                    {
                        Log.debug("Client {0} : Some error has occured. Connection closed", this.localId);
                        sendConnectionEnd();
                        Close();
                        return JobStatus.Failed;
                    }
                    else if ((DateTime.Now - lastActivity).Seconds > 3)
                    {
                        Log.debug("Client {0} : Connection timeout. connection closed", this.localId);
                        sendConnectionEnd();
                        Close();
                        return JobStatus.Failed;
                    }
                    return JobStatus.Pending;
                }
            }
            catch
            {
                Log.debug("Client {0} : Unkown Error occured. connection closed", this.localId);
                try { sendConnectionEnd(); } catch { }
                Close();
                return JobStatus.Failed;
            }
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
                Log.debug("Client {0} : Receive Connection closed message. connection closed", this.localId);

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
                Log.debug("Client {0} : Send {1} bytes of data", this.localId, message.Length);

                try { tcpClient.GetStream().Write(message.data); }
                catch { sendConnectionEnd(); Close(); }
                lastActivity = DateTime.Now;
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
            Log.debug("Client {0} Disconnected", this.localId);
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
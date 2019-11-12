using localStar.Nodes;
using localStar.Structure;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using localStar.Logger;
/*
TODO:   Close할때 ServiceConnectionManager에 등록 해제하기
 */
namespace localStar.Connection
{
    public class ServiceConnection : Connection
    {
        private IConnection connection = null;
        private Service service;
        private bool onClosing = false;
        private byte[] buffer = new byte[ushort.MaxValue];

        public ServiceConnection(Service service)
        {
            Log.debug("Service {0} : Making New Connection, {1}", this.localId, service.name);

            tcpClient = new TcpClient();
            var task = tcpClient.ConnectAsync(service.address.Address, service.address.Port);
            task.Wait(1000);
            if (!task.IsCompleted)
            {
                Log.error("Service {0} : Can not Make a new connection with {1}", this.localId, service.name);
                throw new Exception("Can not connect to service " + service.name);
            }
            Log.debug("Service {0} : Connection Established, {1}", this.localId, service.name);

            this.service = service;
        }
        public JobStatus handleRead()
        {
            try
            {
                if (!tcpClient.Connected) return JobStatus.Failed;
                else
                {
                    if (!tcpClient.GetStream().DataAvailable)
                    {
                        tcpClient.GetStream().Write(new byte[0]);
                        return JobStatus.Pending;
                    }
                    int len = tcpClient.GetStream().Read(this.buffer);

                    if (len != 0)
                    {
                        Log.debug("Service {0} : Received {1} bytes of data.", this.localId, len);
                        byte[] tmp = new byte[len];
                        Buffer.BlockCopy(buffer, 0, tmp, 0, len);
                        connection.Send(new Message
                        {
                            From = this,
                            Type = MessageType.NormalConnection,
                            data = tmp
                        });
                        return JobStatus.Good;
                    }
                    else
                    {
                        Log.debug("Service {0} : Service send 0 byte data. Connection closed", this.localId);
                        sendConnectionEnd();
                        Close();
                        return JobStatus.Failed;
                    }
                }
            }
            catch
            {
                Log.debug("Service {0} : Some Error occured. Connection closed", this.localId);
                try { sendConnectionEnd(); } catch { }
                Close();
                return JobStatus.Failed;
            }
        }
        public override void Send(Message message)
        {
            if (message.Type == MessageType.NormalConnection)
            {
                Log.debug("Service {0} : Send {1} bytes of data", this.localId, message.Length);
                try { tcpClient.GetStream().Write(message.data); }
                catch { sendConnectionEnd(); Close(); }
            }
            else if (message.Type == MessageType.EndConnection)
            {
                Log.debug("Service {0} : Receive Connection closed message. connection closed", this.localId);
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
            else if (message.Type == MessageType.NewConnection)
            {
                connection = message.From;
                HandleLoop.addJob(handleRead);
                // try { tcpClient.GetStream().Write(message.data); }
                // catch { sendConnectionEnd(); Close(); }
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
            Log.debug("Service Connection {0} Closed", this.localId);
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
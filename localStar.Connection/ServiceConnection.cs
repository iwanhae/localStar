using localStar.Nodes;
using localStar.Structure;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;
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
        private Task<int> readTask = null;
        private byte[] buffer = new byte[ushort.MaxValue];

        public ServiceConnection(Service service)
        {
            tcpClient = new TcpClient();
            var task = tcpClient.ConnectAsync(service.address.Address, service.address.Port);
            task.Wait(1000);
            if (!task.IsCompleted) throw new Exception("Can not connect to service " + service.name);

            this.service = service;
        }
        public JobStatus handleRead()
        {
            try
            {
                if (!tcpClient.Connected) return JobStatus.Failed ;
                /*
                else if (readTask == null)
                {
                    readTask = tcpClient.GetStream().ReadAsync(this.buffer).AsTask();
                    return true;
                }
                */
                else // if (readTask.IsCompleted)
                {
                    if (!tcpClient.GetStream().DataAvailable)
                    {
                        tcpClient.GetStream().Write(new byte[0]);
                        return JobStatus.Pending;
                    }
                    int len = tcpClient.GetStream().Read(this.buffer);
                    // readTask = tcpClient.GetStream().ReadAsync(this.buffer).AsTask();
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
                        return JobStatus.Good;
                    }
                    else
                    {
                        sendConnectionEnd();
                        Close();
                        return JobStatus.Failed;
                    }
                }
            }
            catch
            {
                try { sendConnectionEnd(); } catch { }
                Close();
                return JobStatus.Failed;
            }
        }
        public override void Send(Message message)
        {
            if (message.Type == MessageType.NormalConnection)
            {
                try { tcpClient.GetStream().Write(message.data); }
                catch { sendConnectionEnd(); Close(); }
            }
            else if (message.Type == MessageType.EndConnection)
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
            else if (message.Type == MessageType.NewConnection)
            {
                connection = message.From;
                HandleLoop.addJob(handleRead);
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
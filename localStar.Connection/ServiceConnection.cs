using localStar.Nodes;
using localStar.Structure;
using System;
using System.Net.Sockets;
using System.Threading;
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
        public ServiceConnection(Service service)
        {
            tcpClient = new TcpClient();
            var task = tcpClient.ConnectAsync(service.address.Address, service.address.Port);
            task.Wait(1000);
            if (!task.IsCompleted) throw new Exception("Can not connect to service " + service.name);

            readThread = new Thread(handleRead);
            // readThread.Start(); 어떤 IConnection과 소통해야되는지 알기 전까지 지연.

            this.service = service;
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
            { try { sendConnectionEnd(); } catch { } }
            Close();
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
                readThread.Start();
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
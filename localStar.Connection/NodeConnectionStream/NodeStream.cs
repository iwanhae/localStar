using System;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using System.Net.Sockets;
using localStar.Nodes;
using localStar.Config;
using System.Text;
using localStar.Structure;

namespace localStar.Connection
{
    class NodeStream
    {
        private NetworkStream nodeStream;
        private String nodeId;
        public Node node;
        public bool isPrior { get; private set; }
        private Thread handleReceiveThread;

        public bool isAvailable { get { return nodeStream.CanWrite; } }
        public event EventHandler<MessageReceivedArgs> MessageReceived;
        private bool isSending = false;
        public bool IsSending { get => isSending; }

        public NodeStream(NetworkStream nodeStream)
        {
            nodeStream.ReadTimeout = 1000;
            nodeStream.WriteTimeout = 1000;
            this.nodeStream = nodeStream;
            var task = handShake();
            task.Wait();
            if (!task.Result) return;

            handleReceiveThread = new Thread(this.handleReceive);
            handleReceiveThread.Start();
        }

        private async void handleReceive()
        {
            try
            {
                while (this.isAvailable)
                {
                    byte[] buffer = new byte[10];
                    await nodeStream.ReadAsync(buffer, 0, 10);
                    Header header = new Header(buffer);
                    buffer = new byte[header.Length];
                    await nodeStream.ReadAsync(buffer, 0, header.Length);

                    Message msg = new Message();
                    msg.data = buffer;
                    msg.Type = header.type;
                    MessageReceivedArgs args = new MessageReceivedArgs(msg, header.connectionId);
                    MessageReceived?.Invoke(this, args);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR FROM handleReceive, Connection with {0}", this.nodeId);
                Console.WriteLine(e.ToString());
            }
        }

        public void sendMessage(Message message, short connectionId)
        {
            Header header = new Header(connectionId, message);
            MemoryStream stream = new MemoryStream(Tools.concat(header.getEncoded(), message.data), false);
            StreamPipe.Pipe.Connect(stream, nodeStream);
        }

        private async Task<bool> handShake()
        {
            if (!await exchangeId()) return false;
            Console.WriteLine("Try Connecting to {0}", this.nodeId);
            if (isDupId())
            {
                Console.WriteLine("Dup Id");
                return false;
            }
            if (!await exchangeConfig()) return false;
            Console.WriteLine("{0} : exchange settings", this.nodeId);
            if (!await shareNodeInfo()) return false;
            if (!await exchangeTimestamp()) return false;
            Console.WriteLine("{0} : Node handshake success", this.nodeId);
            Console.WriteLine("NEW NODE: {0}", node.ToString());
            return true;
        }

        private async Task<bool> shareNodeInfo()
        {
            byte[] buffer = Nodes.Tools.getBytesFromNode(NodeManager.getCurrentNode());
            await sendBytes(buffer);
            buffer = await getBytes();
            this.node = Nodes.Tools.getNodeFromBytes(buffer);
            return true;
        }
        private async Task<bool> exchangeTimestamp()
        {
            byte[] buffer;
            async Task send()
            {
                long milliseconds = DateTime.Now.Ticks;
                await sendBytes(BitConverter.GetBytes(milliseconds));
                buffer = await getBytes();
                node.setDelay((int)((DateTime.Now.Ticks - BitConverter.ToInt64(buffer)) / TimeSpan.TicksPerMillisecond));
            }
            async Task receive()
            {
                await sendBytes(await getBytes());
            }
            if (this.isPrior)
            {
                await send();
                await receive();
            }
            else
            {
                await receive();
                await send();
            }
            return true;
        }
        private async Task<bool> exchangeConfig()
        {
            // TODO
            return true;
        }
        private bool isDupId()
        {
            if (NodeManager.getNodeById(nodeId) == null) return false;
            else return true;
        }
        private async Task<bool> exchangeId()
        {
            byte[] buffer;

            // send
            await sendBytes(Encoding.UTF8.GetBytes(ConfigMgr.nodeId));

            // get
            buffer = await getBytes();
            string nodeId = Encoding.UTF8.GetString(buffer);
            this.nodeId = nodeId;

            if (String.Compare(nodeId, ConfigMgr.nodeId) > 0) this.isPrior = true;
            else this.isPrior = false;
            return true;
        }

        private async Task<bool> sendBytes(byte[] data)
        {
            if (data.Length < Byte.MaxValue) nodeStream.WriteByte((byte)data.Length);
            else
            {
                nodeStream.WriteByte(0);
                nodeStream.Write(BitConverter.GetBytes(data.Length));
            }
            await nodeStream.WriteAsync(data);
            return true;
        }
        private async Task<byte[]> getBytes()
        {
            byte[] buffer = new byte[1];
            int len;
            await nodeStream.ReadAsync(buffer);
            if (buffer[0] != 0) len = buffer[0];
            else
            {
                buffer = new byte[4]; // Int = 4 bytes
                await nodeStream.ReadAsync(buffer);
                len = BitConverter.ToInt32(buffer);
            }
            buffer = new byte[len];
            await nodeStream.ReadAsync(buffer);
            return buffer;
        }
    }
}
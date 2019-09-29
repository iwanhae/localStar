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
using localStar.Logger;

namespace localStar.Connection
{
    class NodeStream
    {
        private NetworkStream nodeStream;
        private String nodeId;
        public Node node;
        public bool isPrior { get; private set; }
        private Queue<MemoryStream> SendingQueue = new Queue<MemoryStream>();
        public struct RawMessage { public Message message; public short connectionId; };
        public Queue<RawMessage> ReceviedQueue = new Queue<RawMessage>();

        public bool isAvailable { get { return nodeStream.CanRead; } }
        private bool isSending = false;
        public bool IsSending { get => isSending; }

        public NodeStream(NetworkStream nodeStream)
        {
            nodeStream.ReadTimeout = 1000;
            nodeStream.WriteTimeout = 1000;
            this.nodeStream = nodeStream;
            handShake();

            HandleLoop.addJob(sendMessageQueue);
            HandleLoop.addJob(handleReceive);
        }

        private JobStatus handleReceive()
        {
            if (!nodeStream.DataAvailable) return JobStatus.Pending;
            try
            {
                byte[] buffer = new byte[5];
                nodeStream.Read(buffer, 0, 5);
                Header header = new Header(buffer);
                if (header.Length != 0)
                {
                    buffer = new byte[header.Length];
                    nodeStream.Read(buffer, 0, header.Length);
                }
                else buffer = new byte[0];

                Message msg = new Message();
                msg.data = buffer;
                msg.Type = header.type;

                Logger.Log.debug("Received Message {0} Bytes from {1} : {2} / ConnectionId {3}", header.Length, this.nodeId, msg.Type, header.connectionId);

                ReceviedQueue.Enqueue(new RawMessage()
                {
                    message = msg,
                    connectionId = header.connectionId
                });
                return JobStatus.Good;
            }
            catch (Exception e)
            {
                Logger.Log.error("ERROR FROM handleReceive, Connection with {0} : {1}", this.nodeId, e);
                return JobStatus.Failed;
            }
        }

        public void sendMessage(Message message, short connectionId)
        {
            Logger.Log.debug("Send Message {0} Bytes to {1} : {2} / ConnectionId {3}", message.Length, this.nodeId, message.Type, connectionId);
            Header header = new Header(connectionId, message);
            MemoryStream stream = new MemoryStream(Tools.concat(header.getEncoded(), message.data), false);
            SendingQueue.Enqueue(stream);
        }

        private JobStatus sendMessageQueue()
        {
            if (SendingQueue == null) return JobStatus.Failed;
            if (SendingQueue.Count == 0) return JobStatus.Pending;
            try
            {
                MemoryStream tmp = SendingQueue.Dequeue();
                tmp.CopyTo(nodeStream);
                return JobStatus.Good;
            }
            catch
            {
                return JobStatus.Failed;
            }
        }

        private bool handShake()
        {
            if (!exchangeId()) return false;
            Log.debug("Try Connecting to {0}", this.nodeId);
            if (isDupId())
            {
                Log.error("Dup Id");
                return false;
            }
            if (!exchangeConfig()) return false;
            Log.debug("{0} : exchange settings", this.nodeId);
            if (!shareNodeInfo()) return false;
            if (!exchangeTimestamp()) return false;
            Log.debug("{0} : Node handshake success", this.nodeId);
            Log.debug("NEW NODE: {0}", node.ToString());
            return true;
        }

        private bool shareNodeInfo()
        {
            byte[] buffer = Nodes.Tools.getBytesFromNode(NodeManager.getCurrentNode());
            sendBytes(buffer);
            buffer = getBytes();
            this.node = Nodes.Tools.getNodeFromBytes(buffer);
            return true;
        }
        private bool exchangeTimestamp()
        {
            byte[] buffer;
            void send()
            {
                long milliseconds = DateTime.Now.Ticks;
                sendBytes(BitConverter.GetBytes(milliseconds));
                buffer = getBytes();
                node.setDelay((int)((DateTime.Now.Ticks - BitConverter.ToInt64(buffer)) / TimeSpan.TicksPerMillisecond));
            }
            void receive()
            {
                sendBytes(getBytes());
            }
            if (this.isPrior)
            {
                send();
                receive();
            }
            else
            {
                receive();
                send();
            }
            return true;
        }
        private bool exchangeConfig()
        {
            // TODO
            return true;
        }
        private bool isDupId()
        {
            if (NodeManager.getNodeById(nodeId) == null) return false;
            else return true;
        }
        private bool exchangeId()
        {
            byte[] buffer;

            // send
            sendBytes(Encoding.UTF8.GetBytes(ConfigMgr.nodeId));

            // get
            buffer = getBytes();
            string nodeId = Encoding.UTF8.GetString(buffer);
            this.nodeId = nodeId;

            if (String.Compare(nodeId, ConfigMgr.nodeId) > 0) this.isPrior = true;
            else this.isPrior = false;
            return true;
        }

        private bool sendBytes(byte[] data)
        {
            if (data.Length < Byte.MaxValue) nodeStream.WriteByte((byte)data.Length);
            else
            {
                nodeStream.WriteByte(0);
                nodeStream.Write(BitConverter.GetBytes(data.Length));
            }
            nodeStream.Write(data);
            return true;
        }
        private byte[] getBytes()
        {
            byte[] buffer = new byte[1];
            int len;
            nodeStream.Read(buffer);
            if (buffer[0] != 0) len = buffer[0];
            else
            {
                buffer = new byte[4]; // Int = 4 bytes
                nodeStream.Read(buffer);
                len = BitConverter.ToInt32(buffer);
            }
            buffer = new byte[len];
            nodeStream.Read(buffer);
            return buffer;
        }

        public void Close()
        {
            if (nodeStream.DataAvailable)
            {
                try
                {
                    nodeStream.Close();
                }
                catch { }
            }

            SendingQueue = null;
            ReceviedQueue = null;
        }
    }
}
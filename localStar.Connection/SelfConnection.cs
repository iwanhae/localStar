using localStar.Nodes;
using localStar.Structure;

using System;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace localStar.Connection
{
    public class SelfConnection : Connection
    {
        NodeConnection nodeConnection;
        int delay = int.MaxValue;
        Timer timer;
        DateTime Timestamp;
        bool onHealthCheck = false;
        public SelfConnection(NodeConnection connection, int healthChkPeriod = 10)
        {
            this.nodeConnection = connection;
            HealthCheck(null);
            timer = new Timer(HealthCheck, null, healthChkPeriod * 1000, healthChkPeriod * 1000);
        }
        private void HealthCheck(object _)
        {
            if (onHealthCheck)
            {
                nodeConnection.Close();
                return;
            }
            onHealthCheck = true;
            Timestamp = DateTime.Now;
            nodeConnection.Send(new Message
            {
                Type = MessageType.RequestTimestampEcho,
                From = this,
                data = BitConverter.GetBytes(Timestamp.ToBinary())
            });
            if (nodeConnection.isPrior)
            {
                sendNodeData(MessageType.RequestNodeinfo);
            }
        }
        private void RefreshTimestamp(Message message)
        {
            DateTime now = DateTime.Now;
            onHealthCheck = false;
            DateTime then = DateTime.FromBinary(BitConverter.ToInt64(message.data));
            var gap = now - then;
            this.delay = gap.Milliseconds;
            nodeConnection.node.setDelay(this.delay);
            Logger.Log.debug("Delay with {0} is {1}ms", this.nodeConnection.node.id, this.delay);
        }
        MemoryStream dataReceived = new MemoryStream();
        long dataLength = 0;
        private void handleNodeData(Message message)
        {
            if (dataLength == 0)
            {
                dataLength = BitConverter.ToInt64(message.data);
                Logger.Log.debug("Node Data Length : {0}", dataLength);
                return;
            }
            dataReceived.Write(message.data);
            Logger.Log.debug("Received Node Data {0} bytes", message.Length);

            if (dataReceived.Length == dataLength)
            {
                Node node = Nodes.Tools.getNodeFromBytes(dataReceived.ToArray());
                Logger.Log.debug("Decode Node Data", message.Length);
                node.delay = this.delay;
                node.removeFromAllChildNodes(NodeManager.getCurrentNode());
                NodeManager.addConnectedNode(node);
                dataReceived = new MemoryStream();
                dataLength = 0;

                if (message.Type == MessageType.RequestNodeinfo)
                {
                    sendNodeData(MessageType.ResponseNodeinfo);
                }
            }
        }
        private void sendNodeData(MessageType type = MessageType.RequestNodeinfo)
        {
            MemoryStream data2Send = new MemoryStream(Nodes.Tools.getBytesFromNode(NodeManager.getCurrentNode()), false);

            long totalLength = data2Send.Length;

            nodeConnection.Send(new Message()
            {
                Type = type,
                From = this,
                data = BitConverter.GetBytes(totalLength),
            });

            while (data2Send.Position != data2Send.Length)
            {
                long len = data2Send.Length - data2Send.Position;
                ushort Length = len > ushort.MaxValue ? ushort.MaxValue : (ushort)len;
                byte[] buffer = new byte[Length];
                data2Send.Read(buffer);
                nodeConnection.Send(new Message()
                {
                    Type = type,
                    From = this,
                    data = buffer
                });
            }
            Logger.Log.debug("Send Node Data Completed");
        }

        public override void Send(Message message)
        {
            switch (message.Type)
            {
                case MessageType.RequestNodeinfo:
                    handleNodeData(message);
                    break;
                case MessageType.ResponseNodeinfo:
                    handleNodeData(message);
                    break;
                case MessageType.RequestTimestampEcho:
                    message.From.Send(new Message
                    {
                        Type = MessageType.ResponseTimestampEcho,
                        From = this,
                        data = message.data
                    });
                    break;
                case MessageType.ResponseTimestampEcho:
                    RefreshTimestamp(message);
                    break;
                default:
                    break;
            }
        }
        public override void Close()
        {
            this.timer.Dispose();
            NodeManager.getCurrentNode().removeFromAllChildNodes(this.nodeConnection.nodeId);
        }
    }
}
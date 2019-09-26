using localStar.Nodes;
using localStar.Structure;

using System;
using System.Threading;
using System.Text;

namespace localStar.Connection
{
    public class SelfConnection : Connection
    {
        NodeConnection nodeConnection;
        Timer timer;
        DateTime Timestamp;
        bool onHealthCheck = false;
        public SelfConnection(NodeConnection connection, int healthChkPeriod = 10)
        {
            this.nodeConnection = connection;
            timer = new Timer(HealthCheck, null, healthChkPeriod * 1000, healthChkPeriod * 1000);
        }
        private void HealthCheck(object _)
        {
            if (onHealthCheck) nodeConnection.Close();
            onHealthCheck = true;
            Timestamp = DateTime.Now;
            nodeConnection.Send(new Message
            {
                Type = MessageType.RequestTimestampEcho,
                From = this,
                data = BitConverter.GetBytes(Timestamp.ToBinary())
            });
        }
        private void RefreshTimestamp(Message message)
        {
            DateTime now = DateTime.Now;
            onHealthCheck = false;
            DateTime then = DateTime.FromBinary(BitConverter.ToInt64(message.data));
        }
        public override void Close() { }

        public override void Send(Message message)
        {
            switch (message.Type)
            {
                case MessageType.RequestNodeinfo:
                    break;
                case MessageType.ResponseNodeinfo:
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
                    break;
                default:
                    break;
            }
            throw new System.NotImplementedException();
        }
    }
}
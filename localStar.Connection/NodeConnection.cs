using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

using localStar.Structure;
using localStar.Nodes;

namespace localStar.Connection
{
    public class NodeConnection : Connection
    {
        private Node node;
        private NodeStream nodeStream;
        private Map<ushort, short> connectionIdMap = new Map<ushort, short>();
        private short connectionIdPtr = 0;
        private bool isPrior { get => nodeStream.isPrior; }

        private void handleReceived(object sender, MessageReceivedArgs args)
        {
            /*
            On NodeStream
                msg.Length = header.Length;
                msg.data = buffer;
                msg.Type = header.type;
             */
            Message message = args.Message;
            message.LocalFrom = this.localId;
            switch (message.Type)
            {
                case MessageType.NewConnection:
                    break;
                case MessageType.NormalConnection:
                    break;
                case MessageType.EndConnection:
                    break;
                case MessageType.RequestNodeinfo:
                    break;
                case MessageType.ResponseNodeinfo:
                    break;
                case MessageType.RequestTimestampEcho:
                    break;
                case MessageType.ResponseTimestampEcho:
                    break;
                default:
                    break;
            }
        }
        public override void Send(Message message)
        {
            if (message.Type == MessageType.NewConnection)
            {
                short connectionId = getNewConnectionId();
                connectionIdMap.Add(message.LocalFrom, connectionId);
                nodeStream.sendMessage(message, connectionId);
            }
            else if (message.Type == MessageType.NormalConnection)
            {
                short connectionId = connectionIdMap.Forward[message.LocalFrom];
                nodeStream.sendMessage(message, connectionId);
            }
            else if (message.Type == MessageType.EndConnection)
            {
                short connectionId = connectionIdMap.Forward[message.LocalFrom];
                nodeStream.sendMessage(message, connectionId);
                connectionIdMap.RemoveBackward(connectionId);
            }
            else
            {
                nodeStream.sendMessage(message, 0);
            }
        }

        private short getNewConnectionId()
        {
            if (isPrior)
            {
                while (connectionIdMap.Backward.ContainsKey(++connectionIdPtr))
                    if (connectionIdPtr == short.MaxValue) connectionIdPtr = 0;
            }
            else
            {
                while (connectionIdMap.Backward.ContainsKey(--connectionIdPtr))
                    if (connectionIdPtr == short.MinValue) connectionIdPtr = 0;
            }
            return connectionIdPtr;
        }

        protected override void handleRead() { }

        /// <param name="address">요청을 날릴 주소</param>
        public NodeConnection(IPEndPoint address)
        {
            tcpClient = new TcpClient();
            tcpClient.Connect(address);
            Init(tcpClient);
        }
        /// <param name="tcpClient">이미 통신준비가 완료된 객체</param>
        public NodeConnection(TcpClient tcpClient) => Init(tcpClient);
        private void Init(TcpClient tcpClient)
        {
            if (!tcpClient.Connected) throw new ArgumentException("사용 불가능한 연결");
            nodeStream = new NodeStream(tcpClient.GetStream());
            nodeStream.MessageReceived += handleReceived;
        }
    }
}
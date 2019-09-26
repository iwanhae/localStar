using System;
using System.Net;
using System.Net.Sockets;
using System.IO;

using localStar.Logger;
using localStar.Structure;
using localStar.Nodes;

namespace localStar.Connection
{
    public class NodeConnection : Connection
    {
        public Node node { get => nodeStream.node; }
        private NodeStream nodeStream;
        private SelfConnection selfConnection;
        private Map<IConnection, short> connectionIdMap = new Map<IConnection, short>();
        private short connectionIdPtr = 0;
        private bool isPrior { get => nodeStream.isPrior; }

        private void handleReceived(object sender, MessageReceivedArgs args)
        {
            /*
            On NodeStream
                msg.data = buffer;
                msg.Type = header.type;
             */
            Message message = args.Message;
            short connectionId = args.ConnectionId;
            message.From = this;
            switch (message.Type)
            {
                case MessageType.NewConnection:
                    makingNewConnection(message, connectionId);
                    break;
                case MessageType.NormalConnection:
                    IConnection tmp = connectionIdMap.Backward[connectionId];
                    if (tmp == null) EndConnection(connectionId);
                    else tmp.Send(message);
                    break;
                case MessageType.EndConnection:
                    connectionIdMap.Backward[connectionId].Send(message);
                    connectionIdMap.RemoveBackward(connectionId);
                    break;
                default:
                    selfConnection.Send(message);
                    break;
            }
        }

        private void makingNewConnection(Message message, short connectionId)
        {
            MemoryStream ms = new MemoryStream(message.data);
            HttpHeader header = new HttpHeader(ms);
            string destinedService;
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
            Log.debug("handle New Message From {0}, Routing to {1}", this.node.id, destinedService);

            IConnection connection = NodeManager.shortestPathTo(destinedService);

            if (connection == null)
            {
                Log.error("Can not make new connection to {0}", destinedService);
                EndConnection(connectionId);
                return;
            }

            connectionIdMap.Add(connection, connectionId);
            connection.Send(message);
        }
        private void EndConnection(short connectionId)
        {
            nodeStream.sendMessage(new Message
            {
                Type = MessageType.EndConnection,
                data = new byte[0]
            }, connectionId);
        }
        public override void Send(Message message)
        {
            if (message.Type == MessageType.NewConnection)
            {
                short connectionId = getNewConnectionId();
                connectionIdMap.Add(message.From, connectionId);
                nodeStream.sendMessage(message, connectionId);
            }
            else if (message.Type == MessageType.NormalConnection)
            {
                short connectionId = connectionIdMap.Forward[message.From];
                nodeStream.sendMessage(message, connectionId);
            }
            else if (message.Type == MessageType.EndConnection)
            {
                short connectionId = connectionIdMap.Forward[message.From];
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

        protected bool handleRead() { return false; }

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
            selfConnection = new SelfConnection(this);
            connectionIdMap.Add(selfConnection, 0);

            if (!tcpClient.Connected) throw new ArgumentException("사용 불가능한 연결");
            Log.debug("New Node Connection with {0}", ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address);
            nodeStream = new NodeStream(tcpClient.GetStream());
            nodeStream.MessageReceived += handleReceived;
        }

        public override void Close()
        {
            throw new NotImplementedException();
        }
    }
}
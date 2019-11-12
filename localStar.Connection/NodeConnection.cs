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
        public string nodeId { get => node.id; }
        private NodeStream nodeStream;
        private SelfConnection selfConnection;
        private Map<IConnection, short> connectionIdMap = new Map<IConnection, short>();
        private short connectionIdPtr = 0;
        public bool isPrior { get => nodeStream.isPrior; }
        public bool isConnected { get => nodeStream != null; }

        private JobStatus handleReceived(NodeStream.RawMessage rawMessage)
        {
            /*
            On NodeStream
                msg.data = buffer;
                msg.Type = header.type;
             */
            Message message = rawMessage.message;
            short connectionId = rawMessage.connectionId;
            message.From = this;
            IConnection tmp;

            if (connectionId == 0)
            {
                selfConnection.Send(message);
            }
            else
            {
                switch (message.Type)
                {
                    case MessageType.NewConnection:
                        makingNewConnection(message, connectionId);
                        break;
                    case MessageType.NormalConnection:
                        if (connectionIdMap.Backward.TryGetValue(connectionId, out tmp)) tmp.Send(message);
                        else TerminateConnection(connectionId);
                        break;
                    case MessageType.EndConnection:
                        if (connectionIdMap.Backward.TryGetValue(connectionId, out tmp))
                        {
                            connectionIdMap.RemoveBackward(connectionId);
                            tmp.Send(message);
                        }
                        break;
                    default:
                        break;
                }
            }
            return JobStatus.Good;
        }

        private void makingNewConnection(Message message, short connectionId)
        {
            string destinedService = System.Text.Encoding.UTF8.GetString(message.data);
            Log.debug("handle New Message From {0}, Routing to {1}", this.node.id, destinedService);

            IConnection connection = NodeManager.shortestPathTo(destinedService);

            if (connection == null)
            {
                Log.error("Can not make new connection to {0}", destinedService);
                TerminateConnection(connectionId);
                return;
            }

            Log.debug("NODE : Register new connection {0}", connectionId);
            connectionIdMap.Add(connection, connectionId);
            connection.Send(message);
        }
        private void TerminateConnection(short connectionId)
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
                Log.debug("NODE : Register new connection {0}", connectionId);
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


        /// <param name="address">요청을 날릴 주소</param>
        public NodeConnection(IPEndPoint address)
        {
            tcpClient = new TcpClient();
            var task = tcpClient.BeginConnect(address.Address, address.Port, null, null);
            task.AsyncWaitHandle.WaitOne(3000);
            if (!tcpClient.Connected)
            {
                Log.error("Can not Connect to {0}:{1}", address.Address, address.Port);
                tcpClient.Close();
                return;
            }
            Init(tcpClient);
        }
        /// <param name="tcpClient">이미 통신준비가 완료된 객체</param>
        public NodeConnection(TcpClient tcpClient) => Init(tcpClient);
        private void Init(TcpClient tcpClient)
        {
            if (!tcpClient.Connected) throw new ArgumentException("사용 불가능한 연결");
            Log.debug("New Node Connection with {0}", ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address);
            nodeStream = new NodeStream(tcpClient.GetStream(), handleReceived);

            selfConnection = new SelfConnection(this);
            connectionIdMap.Add(selfConnection, 0);

            NodeConnectionManager.addNodeConnection(this);
        }

        public override void Close()
        {
            nodeStream.Close();
            selfConnection.Close();
            connectionIdMap = null;
            Log.error("Connection Closed {0}", this.node.id);
            NodeConnectionManager.removeNodeConnection(this);
        }
    }
}
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using localStar.Connection.Stream;

namespace localStar.Connection
{
    public class NodeConnection
    {
        TcpClient tcpClient = null;
        NodeStream stream = null;

        public ushort localId { get => (ushort)((IPEndPoint)tcpClient.Client.RemoteEndPoint).Port; }
        public bool isConnected { get => tcpClient.Connected; }
        public Queue<Message> messageToSend = new Queue<Message>();
        /// <summary>
        /// localId를 ConnectionId로 바꿔주는 Table
        /// </summary>
        /// <typeparam name="ushort">localId</typeparam>
        /// <typeparam name="short">ConnectionId</typeparam>
        /// <returns></returns>
        public SortedDictionary<ushort, short> LocalId2ConnectionId = new SortedDictionary<ushort, short>();
        /// <summary>
        /// ConnectionId를 localId로 바꿔주는 Table
        /// </summary>
        /// <typeparam name="short">ConnectionId</typeparam>
        /// <typeparam name="ushort">localId</typeparam>
        /// <returns></returns>
        public SortedDictionary<short, ushort> ConnectionId2LocalId = new SortedDictionary<short, ushort>();


        public NodeConnection(IPEndPoint address)  // 연결 요청 날림
        {
            tcpClient = new TcpClient();
            tcpClient.Connect(address);
            stream = new NodeStream(tcpClient.GetStream());
        }
        public NodeConnection(TcpClient tc)    // 받은 요청 처리
        {
            tcpClient = tc;
            stream = new NodeStream(tc.GetStream());
        }

    }
}
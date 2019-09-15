using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using localStar.Connection.Stream;

namespace localStar.Connection
{
    class NodeConnection
    {
        TcpClient tcpClient = null;
        NodeStream stream = null;

        public bool isConnected { get => tcpClient.Connected; }
        public Queue<Message> messageToSend = new Queue<Message>();
        public Queue<Message> messageReceived = new Queue<Message>();


        NodeConnection(IPEndPoint address)  // 연결 요청 날림
        {
            tcpClient = new TcpClient();
            tcpClient.Connect(address);
            stream = new NodeStream(tcpClient.GetStream());
        }
        NodeConnection(TcpClient tc)    // 받은 요청 처리
        {
            tcpClient = tc;
            stream = new NodeStream(tc.GetStream());
        }

    }
}
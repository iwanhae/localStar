using System;
using localStar.Structure;

namespace localStar.Connection
{
    public class MessageReceivedArgs : EventArgs
    {
        public Message Message;
        public short ConnectionId;
        public MessageReceivedArgs(Message message, short ConnectionId)
        {
            this.Message = message;
            this.ConnectionId = ConnectionId;
        }
    }
}
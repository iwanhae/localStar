using System;
using localStar.Structure;

namespace localStar.Connection
{
    public class MessageReceivedArgs : EventArgs
    {
        public Message Message;
        public MessageReceivedArgs(Message message)
        {
            this.Message = message;
        }
    }
}
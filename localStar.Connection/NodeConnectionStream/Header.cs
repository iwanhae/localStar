using System;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Collections;
using System.Net.Sockets;
using localStar.Nodes;
using localStar.Config;
using System.Text;
using localStar.Structure;

namespace localStar.Connection
{
    public class Header
    {
        public short connectionId;
        public MessageType type;
        public ushort Length;

        /// <summary>
        /// 10 Bytes 크기의 HeaderBytes
        /// </summary>
        /// <param name="data"></param>
        public Header(byte[] data)
        {
            decode(data);
        }

        public Header(short connectionId, Message message)
        {
            this.connectionId = connectionId;
            this.type = message.Type;
            this.Length = message.Length;
        }

        public void decode(byte[] data)
        {
            connectionId = BitConverter.ToInt16(data, 0);   // 0 1
            Length = BitConverter.ToUInt16(data, 2);        // 2 3
            type = typeChecker(data[4]);                    // 4
        }
        public byte[] getEncoded()
        {
            MemoryStream stream = new MemoryStream();
            stream.Write(BitConverter.GetBytes(connectionId));  // 0 1
            stream.Write(BitConverter.GetBytes(Length));        // 2 3
            stream.WriteByte(type2Byte(type));                  // 4
            return stream.ToArray();
        }
        private MessageType typeChecker(byte data)
        {
            if (Enum.IsDefined(typeof(MessageType), data)) return (MessageType)data;
            else return MessageType.UNKOWN;
        }
        private byte type2Byte(MessageType type)
        {
            return (byte)type;
        }
    }
}
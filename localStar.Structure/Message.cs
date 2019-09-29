using localStar.Connection;

namespace localStar.Structure
{
    public struct Message
    {
        public string URL;
        public IConnection From;
        public MessageType Type;
        public ushort Length { get => (ushort)data.Length; }
        public byte[] data;
    }


}
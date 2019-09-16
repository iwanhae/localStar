namespace localStar.Connection
{
    public struct Message
    {
        public string URL;
        public short LocalTo;
        public short LocalFrom;
        public MessageType Type;
        public ushort Length;
        public byte[] data;
    }


}
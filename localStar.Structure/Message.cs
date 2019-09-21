namespace localStar.Structure
{
    public struct Message
    {
        public string URL;
        public ushort LocalTo;
        public ushort LocalFrom;
        public MessageType Type;
        public ushort Length { get => (ushort)data.Length; }
        public byte[] data;
    }


}
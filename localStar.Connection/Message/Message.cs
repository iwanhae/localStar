namespace localStar.Connection
{
    public class Message
    {
        private string url;
        private short localTo;
        private short localFrom;
        private byte[] data;

        public string URL { get => url; }
        public short LocalTo { get => localTo; }
        public short LocalFrom { get => localFrom; }
        public ConnectionType Type { get; }

        public void setLocalTo(short localTo) => this.localTo = localTo;

        Message(string url, short localTo, short localFrom, byte[] data, ConnectionType type = ConnectionType.PLAIN)
        {
            this.url = url;
            this.localTo = localTo;
            this.localFrom = localFrom;
            this.data = data;
            this.Type = type;
        }
        Message(string url, short localFrom, byte[] data)
        {
            // TODO
        }

        public byte[] getData()
        {
            return data;
        }
    }
}
namespace localStar.Connection
{
    public enum MessageType : byte
    {
        NewConnection = 1, NormalConnection = 2, EndConnection = 3,
        RequestTimestampEcho = 11, ResponseTimestampEcho = 12,
        RequestNodeinfo = 21, ResponseNodeinfo = 22,
        UNKOWN = 0
    }
}
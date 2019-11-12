namespace localStar.Nodes
{
    public class Healthcheck
    {
        public string method { get; set; }
        public int interval { get; set; }
        public int timeout { get; set; }
        public int retries { get; set; }

        public Healthcheck(string method = "get", int interval = 10, int timeout = 10, int retries = 3)
        {
            this.method = method;
            this.interval = interval;
            this.timeout = timeout;
            this.retries = retries;
        }
        public Healthcheck() { }
    }
}
using System;
using System.Net;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace localStar
{
    static class Tools
    {
        public static byte[] concat(byte[] first, byte[] second)
        {
            byte[] ret = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            return ret;
        }
        public static IPEndPoint parseIPEndPoint(string address, int defaultPort)
        {
            string[] tmp = address.Split(':');
            IPAddress ip = IPAddress.Parse(tmp[0]);
            if (tmp.Length == 2)
                defaultPort = int.Parse(tmp[1]);
            return new IPEndPoint(ip, defaultPort);
        }
    }
}

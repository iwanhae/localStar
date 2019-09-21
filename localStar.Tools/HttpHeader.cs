using System;
using System.IO;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Text;
namespace localStar
{
    public class HttpHeader
    {
        private const int HEADER_SIZE_LIMIT = 4096;

        private Stream networkStream;
        private string method = "";
        private string path = "";
        private string version = "";
        private string status = "";
        private bool isRequest = true;

        public Dictionary<string, string> KVSet = new Dictionary<string, string>();
        public string RawHeader { get => buildRawHeader(); }
        public string Host { get => KVSet["Host"]; }

        public bool isValid { get; } = false;
        public HttpHeader(Stream stream, Dictionary<string, string> additionalHeader = null)
        {
            networkStream = stream;
            try
            {
                parseHeader();
            }
            catch
            {
                closeWithError();
            }

            if (additionalHeader != null)
                foreach (var pair in additionalHeader)
                    KVSet[pair.Key.Trim()] = pair.Value?.Trim();
            isValid = true;
        }

        public byte[] getBytes() => Encoding.ASCII.GetBytes(RawHeader);

        private string buildRawHeader()
        {
            if (!isValid) throw new Exception("This is not Valid Header");
            StringBuilder builder = new StringBuilder();
            builder.Append(String.Format("{0} {1} {2}\r\n", method, path, version));
            foreach (var pair in KVSet)
            {
                if (pair.Value == null || pair.Value == "") continue;
                builder.Append(String.Format("{0}: {1}\r\n", pair.Key, pair.Value));
            }
            builder.Append("\r\n");
            return builder.ToString();
        }

        private void parseHeader()
        {
            StringBuilder builder = new StringBuilder(HEADER_SIZE_LIMIT);
            networkStream.ReadTimeout = 100;
            int canNotReadCount = 0;
            while (true)
            {
                if (canNotReadCount > 10) throw new Exception();    //0.1초
                int readbyte = networkStream.ReadByte();
                if (readbyte == -1) { Thread.Sleep(10); canNotReadCount++; continue; } // 스트림이 브라우저로 부터 읽지 못한 경우이므로 패스
                builder.Append(Convert.ToChar(readbyte));
                if (readbyte == '\r') { continue; }
                if (readbyte == '\n') { break; }
            }
            {   // 첫줄 파싱
                string[] firstLine = builder.ToString().Split(' ');
                if ("GETHEADPOSTPUTPATCHDELETECONNECTTRACEOPTIONS".IndexOf(firstLine[0].Trim()) != -1)
                {
                    isRequest = true;
                    method = firstLine[0].Trim();
                    path = firstLine[1].Trim();
                    version = firstLine[2].Trim();
                }
                else if (firstLine[0].Trim().IndexOf("HTTP") != -1)
                {
                    isRequest = false;
                    version = firstLine[0].Trim();
                    status = firstLine[1].Trim();
                }
                else if (Int32.Parse(firstLine[0].Trim()) != 0)
                {
                    isRequest = false;
                    status = firstLine[0].Trim();
                }
                else throw new Exception("Can Not Parse First Line");
            }

            bool iskey = true;
            string key = "";
            string value = "";
            bool wasNextLine = false;
            bool isValidHeader = false;
            for (int i = 0; i < HEADER_SIZE_LIMIT; i++)
            {
                if (canNotReadCount > 100) throw new Exception();    //1초
                int readbyte = networkStream.ReadByte();
                if (readbyte == -1) { Thread.Sleep(10); continue; }

                if (readbyte == '\r') continue;
                if (readbyte == ':' && iskey) { iskey = false; continue; }
                if (readbyte == '\n')
                {
                    if (wasNextLine)
                    {
                        isValidHeader = true;
                        break;
                    }
                    else
                    {
                        wasNextLine = true;
                        iskey = true;
                        KVSet.Add(key.Trim(), value.Trim());
                        key = "";
                        value = "";
                        continue;
                    }
                }
                else wasNextLine = false;
                if (iskey == true)
                {
                    key += Convert.ToChar(readbyte);
                }
                else
                {
                    value += Convert.ToChar(readbyte);
                }
            }
            if (!isValidHeader) throw new Exception("Fail to parse Header");
        }
        public void close()
        {
            try
            {
                networkStream.Dispose();
            }
            catch { }
        }
        public void closeWithNotFound()
        {
            try
            {
                string msg = String.Format("HTTP/1.1 404 Not Found\r\n\r\n");
                networkStream.Write(Encoding.ASCII.GetBytes(msg));
                networkStream.Flush();
            }
            catch { };
            this.close();
        }
        public void closeWithError()
        {
            try
            {
                string msg = String.Format("HTTP/1.1 500 Internal Server Error\r\n\r\n");
                networkStream.Write(Encoding.ASCII.GetBytes(msg));
                networkStream.Flush();
            }
            catch { };
            this.close();
        }
    }
}
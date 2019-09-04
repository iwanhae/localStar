using ARSoft.Tools.Net.Dns;
using ARSoft.Tools.Net;
using System.Net;
using System.Threading.Tasks;
using System;
using localStar.Config;

namespace localStar.DNS
{
    public class Service
    {
        public static void Start()
        {
            DnsMessage msg = new DnsMessage();

            DnsServer server = new DnsServer(new IPEndPoint(ConfigMgr.dnsHost, ConfigMgr.dnsPort), 10, 10);
            server.QueryReceived += handleQuery;
            server.Start();
            return;
        }

        static async Task handleQuery(object sender, QueryReceivedEventArgs e)
        {
            DnsMessage query = e.Query as DnsMessage;
            DnsMessage response = query.CreateResponseInstance();

            if (query == null || query.Questions.Count == 0)
            {
                e.Response = response;
                return;
            }

            DnsQuestion[] records = new DnsQuestion[query.Questions.Count];
            query.Questions.CopyTo(records);
            foreach (DnsQuestion record in records)
            {
                String[] Labels = record.Name.Labels;
                if (!Labels[Labels.Length - 1].ToLower().Equals("service")) continue; // 서비스로 안끝나면 관심없음

                String q = String.Concat(record.Name.Labels);
                response.AnswerRecords.Add(new ARecord(record.Name, 10, ConfigMgr.localPublicIP));
                query.Questions.Remove(record);
            }

            if (query.Questions.Count != 0)
            {
                DnsClient client = new DnsClient(ConfigMgr.nameSevers, 1000);
                DnsMessage q = await client.SendMessageAsync(query);
                if (q != null)
                {
                    response.AnswerRecords.AddRange(q.AnswerRecords);
                }
            }

            response.ReturnCode = ReturnCode.NoError;
            response.IsRecursionDesired = false;
            e.Response = response;
        }
    }
}
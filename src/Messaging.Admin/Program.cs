using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Messaging.Admin
{
    class Program
    {
        static void Main(string[] args)
        {
            string tenantName = "Tenant_T0001";
            string queueName = "queueDeTest";
            string tenantUrl = "http://localhost:8772"; // url of tenant
            string tenantKey1 = "0123456789";
            string tenantKey2 = "9876543210";

            // http://localhost:8772/api/Message/GetMessage
            TestGetPut1(tenantUrl, tenantKey1, queueName, "CLI Test", 50);
            Console.ReadLine();
        }


        static void TestGetPut1(string baseUrl,string tenantKey, string queueName,string clientId,int nbMessage)
        {
            string urlPut = $"{baseUrl}/api/Message/PutMessage?clientId={clientId}&queue={queueName}&payload=";

            string fullPutUrl;
            string result;
            Console.WriteLine($"Pushing {nbMessage} ...");
            Stopwatch chrono = Stopwatch.StartNew();
            WebClient wc = new WebClient();
            wc.Headers.Add("Authorization", tenantKey);
            for (int n=0;n<nbMessage;n++)
            {
                fullPutUrl = urlPut + $"Message {n} {DateTime.Now.ToLongTimeString()}";
                result = wc.DownloadString(fullPutUrl);
            }
            chrono.Stop();
            Console.WriteLine($"Ellapsed time : {chrono.ElapsedMilliseconds}ms");

            Console.WriteLine("");
            Console.WriteLine("Retrieving all messages ....");
            string urlGet = $"{baseUrl}/api/Message/GetMessage?clientId={clientId}&queue={queueName}";
            chrono = Stopwatch.StartNew();
            result = wc.DownloadString(urlGet);
            int count = 0;
            while (!string.IsNullOrEmpty(result))
            {
                count++;
                result = wc.DownloadString(urlGet);
            }
            chrono.Stop();
            Console.WriteLine($"Retrieved {count} messages");
            Console.WriteLine($"Ellapsed time : {chrono.ElapsedMilliseconds}ms");


        }

    }
}

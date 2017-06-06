using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Messaging.SimpleTests
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string tenantName = "QT-T001-API";
                string tenantUrl = "http://localhost:8772"; // url of tenant
                string tenantKey1 = "0123456789";
                string tenantKey2 = "9876543210";

                var rndQueueName = $"testQueue-NICO{Guid.NewGuid()}";

                TestCreateQueue(tenantUrl, tenantName, tenantKey2, rndQueueName);
                Console.WriteLine("Queue created. Press [Enter] key to continue");
                Console.ReadLine();

                TestSetDeleteDelay(tenantUrl, tenantName, tenantKey1, rndQueueName, 10);
                Console.WriteLine("DeleteDelay set. Press [Enter] key to continue");
                Console.ReadLine();

                TestSetRetentionDuration(tenantUrl, tenantName, tenantKey1, rndQueueName, 1 * 24 * 60 * 60);
                Console.WriteLine("RetentionDuration set. Press [Enter] key to continue");
                Console.ReadLine();

                TestGetPut1(tenantUrl, tenantName, tenantKey1, rndQueueName, "CLI Test", 20);
                Console.WriteLine("Message test passed. Press [Enter] key to continue");
                Console.ReadLine();

                TestRemoveQueue(tenantUrl, tenantName, tenantKey1, rndQueueName);
                Console.WriteLine("Queue removed. Press [Enter] key to continue");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n\nEXCEPTION : " + ex.ToString());
            }

            Console.ReadLine();
        }

        private static WebClient BuildWebClient(string tenantName, string tenantKey)
        {
            WebClient wc = new WebClient();
            wc.Headers.Add("Authorization", $"DEBUGSharedKey {tenantName}:{tenantKey}"); // required
            wc.Headers.Add("x-ms-date", DateTime.UtcNow.ToString("u")); // required
            wc.Headers.Add("x-ms-version", "2015-12-11"); // optionnal
            wc.Headers.Add("x-ms-client-request-id", Guid.NewGuid().ToString()); // optionnal
            return wc;
        }


        static void TestCreateQueue(string baseUrl, string tenantName, string tenantKey, string queueName)
        {
            string urlCreate = $"{baseUrl}/api/Queue/Create?queueName={queueName}";

            string result;
            Console.WriteLine($"Creating queue '{queueName}' ...");
            Stopwatch chrono = Stopwatch.StartNew();
            WebClient wc = BuildWebClient(tenantName, tenantKey);
            result = wc.DownloadString(urlCreate);
            chrono.Stop();
            Console.WriteLine($"Create queue ellapsed time : {chrono.ElapsedMilliseconds} ms");
        }


        static void TestSetRetentionDuration(string baseUrl, string tenantName, string tenantKey, string queueName, int secondsDuration)
        {
            string urlCreate = $"{baseUrl}/api/Queue/SetRetentionTime?queueName={queueName}&durationInSeconds={secondsDuration.ToString(CultureInfo.InvariantCulture)}";

            string result;
            Console.WriteLine($"Settings retention duration for queue'{queueName}' at {secondsDuration} ...");
            Stopwatch chrono = Stopwatch.StartNew();
            WebClient wc = BuildWebClient(tenantName, tenantKey);
            result = wc.DownloadString(urlCreate);
            chrono.Stop();
            Console.WriteLine($"SetRetentionTime : ellapsed time : {chrono.ElapsedMilliseconds} ms");
        }

        static void TestSetDeleteDelay(string baseUrl, string tenantName, string tenantKey, string queueName, int secondsDuration)
        {
            string urlCreate = $"{baseUrl}/api/Queue/SetDeleteDelay?queueName={queueName}&durationInSeconds={secondsDuration.ToString(CultureInfo.InvariantCulture)}";

            string result;
            Console.WriteLine($"Settings delete delay for queue'{queueName}' at {secondsDuration} ...");
            Stopwatch chrono = Stopwatch.StartNew();
            WebClient wc = BuildWebClient(tenantName, tenantKey);
            result = wc.DownloadString(urlCreate);
            chrono.Stop();
            Console.WriteLine($"SetDeleteDelay : ellapsed time : {chrono.ElapsedMilliseconds} ms");
        }



        static void TestGetPut1(string baseUrl, string tenantName, string tenantKey, string queueName, string clientId, int nbMessage)
        {
            string urlPut = $"{baseUrl}/api/Message/PutMessage?clientId={clientId}&queue={queueName}&payload=";

            string fullPutUrl;
            string result;
            Console.WriteLine($"Pushing {nbMessage} ...");
            Stopwatch chrono = Stopwatch.StartNew();
            WebClient wc;
            for (int n = 0; n < nbMessage; n++)
            {
                wc = BuildWebClient(tenantName, tenantKey);
                fullPutUrl = urlPut + $"Message {n} {DateTime.Now.ToLongTimeString()}";
                result = wc.DownloadString(fullPutUrl);
                Console.WriteLine($"#{n} {result}");
            }
            chrono.Stop();
            Console.WriteLine($"Ellapsed time : {chrono.ElapsedMilliseconds}ms");
            Console.WriteLine($"   Avg time   : {chrono.ElapsedMilliseconds / nbMessage} ms/push");

            Console.WriteLine("");
            Console.WriteLine("Retrieving all messages ....");

            string urlGet = $"{baseUrl}/api/Message/GetMessage?clientId={clientId}&queue={queueName}";
            chrono = Stopwatch.StartNew();
            wc = BuildWebClient(tenantName, tenantKey);
            result = wc.DownloadString(urlGet);
            int count = 0;
            while (!string.IsNullOrEmpty(result))
            {
                count++;
                Console.WriteLine($"#{count} {result}");
                result = wc.DownloadString(urlGet);
            }
            chrono.Stop();
            Console.WriteLine($"Retrieved {count} messages");
            Console.WriteLine($"Ellapsed time : {chrono.ElapsedMilliseconds}ms");
            Console.WriteLine($"   Avg time   : {chrono.ElapsedMilliseconds / count} ms/pop");
        }

        static void TestRemoveQueue(string baseUrl, string tenantName, string tenantKey, string queueName)
        {
            string urlCreate = $"{baseUrl}/api/Queue/Remove?queueName={queueName}";

            string result;
            Console.WriteLine($"Removing queue '{queueName}' ...");
            Stopwatch chrono = Stopwatch.StartNew();
            WebClient wc = BuildWebClient(tenantName, tenantKey);
            result = wc.DownloadString(urlCreate);
            chrono.Stop();
            Console.WriteLine($"Create queue ellapsed time : {chrono.ElapsedMilliseconds} ms");
        }

    }
}

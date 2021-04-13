using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
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

                //TestSequence1(tenantName, tenantUrl, tenantKey1, rndQueueName);
                TestSequencePushGetDelete(tenantName, tenantUrl, tenantKey1, rndQueueName);

                TestRemoveQueue(tenantUrl, tenantName, tenantKey1, rndQueueName);
                Console.WriteLine("Queue removed. Press [Enter] key to continue");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n\nEXCEPTION : " + ex.ToString());
            }
            
        }

#region Test Sequence PushGetDelete

        private static void PushMessage(string tenantName, string tenantUrl, string tenantKey, string qName,string message)
        {
            Console.WriteLine($"Pushing message : '{message}'");
            string urlPut = $"{tenantUrl}/api/Message/PutMessage?clientId=SimpleTests&queue={qName}&payload={message}";
            
            WebClient wc = BuildWebClient(tenantName, tenantKey);
            var result = wc.DownloadString(urlPut);
            Console.WriteLine("   result=" + result);
        }

        private static string GetMessage(string tenantName, string tenantUrl, string tenantKey, string qName)
        {
            Console.WriteLine($"Getting message ...");
            string urlGet = $"{tenantUrl}/api/Message/GetMessage?clientId=SimpleTests&queue={qName}";

            var  wc = BuildWebClient(tenantName, tenantKey);
            var result = wc.DownloadString(urlGet);
            return result;
        }

        private static void DeleteMessage(string tenantName, string tenantUrl, string tenantKey, string qName,string msgId)
        {
            Console.WriteLine($"Deleting message : '{msgId}'");
            string urlGet = $"{tenantUrl}/api/Message/DeleteMessage?clientId=SimpleTests&queue={qName}&popReceipt={msgId}";

            var wc = BuildWebClient(tenantName, tenantKey);
            var result = wc.DownloadString(urlGet);
            Console.WriteLine($"message [{msgId}] deleted with result : {result}");
        }

        private static void TestSequencePushGetDelete(string tenantName, string tenantUrl, string tenantKey, string qName)
        {
           
            TestSetDeleteDelay(tenantUrl, tenantName, tenantKey, qName, 5); // 5 seconde delay

            PushMessage(tenantName, tenantUrl, tenantKey, qName, "Message" + DateTime.Now.Ticks);
            Thread.Sleep(2000);
            var msg = GetMessage(tenantName, tenantUrl, tenantKey, qName);
            if (!string.IsNullOrEmpty(msg))
            { // HACK FOR TEST
                Console.WriteLine($"Read message : {msg}");
                var msgReceipt = msg.Substring(0, msg.IndexOf(' '));
                Thread.Sleep(2000); // < 5 sec : msg will be really deleted.
                DeleteMessage(tenantName, tenantUrl, tenantKey, qName, msgReceipt);
                msg = GetMessage(tenantName, tenantUrl, tenantKey, qName);
                Console.WriteLine($"Read message : {msg}");
            }
            else
            {
                Console.WriteLine("NO MESSAGE");
            }
            Console.WriteLine();
            Console.WriteLine("PRess [enter] to continue ...");
            Console.ReadLine();
           

        }

        #endregion

        private static void TestSequence1(string tenantName, string tenantUrl, string tenantKey1, string rndQueueName)
        {
            TestSetDeleteDelay(tenantUrl, tenantName, tenantKey1, rndQueueName, 10);
            Console.WriteLine("DeleteDelay set. Press [Enter] key to continue");
            Console.ReadLine();

            TestSetRetentionDuration(tenantUrl, tenantName, tenantKey1, rndQueueName, 1 * 24 * 60 * 60);
            Console.WriteLine("RetentionDuration set. Press [Enter] key to continue");
            Console.ReadLine();

            TestGetPut1(tenantUrl, tenantName, tenantKey1, rndQueueName, "CLI Test", 20);
            Console.WriteLine("Message test passed. Press [Enter] key to continue");
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

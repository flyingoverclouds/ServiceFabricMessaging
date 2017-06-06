using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
            try
            {
                string tenantName = "QT-T001-API";
                string tenantUrl = "http://localhost:8772"; // url of tenant
                string tenantKey1 = "0123456789";
                string tenantKey2 = "9876543210";

                var rndQueueName = $"testQueue-NICO{Guid.NewGuid()}";

                Console.WriteLine("********** NOT IMPLEMENTED ***************************************");
            }
            catch(Exception ex)
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

        
    }
}

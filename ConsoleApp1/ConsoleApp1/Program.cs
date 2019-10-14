using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
     class Program
    {

        // An example JSON object, with key/value pairs
        static string json = @"[{""DemoField1"":""DemoValue1"",""DemoField2"":""DemoValue2""},{""DemoField3"":""DemoValue3"",""DemoField4"":""DemoValue4""}]";

        // Update customerId to your Log Analytics workspace ID
        static string customerId = "Please Add WorkSpaceID Here";

        // For sharedKey, use either the primary or the secondary Connected Sources client authentication key   
        static string sharedKey = "Please add Shared Key Here";

        // LogName is name of the event type that is being submitted to Azure Monitor
        static string LogName = "Testapicall";

        // You can use an optional field to specify the timestamp from the data. If the time field is not specified, Azure Monitor assumes the time is the message ingestion time
        static string TimeStampField = "";

        static void Main()
        {
            // Create a hash for the API signature
            var datestring = DateTime.UtcNow.ToString("r");
            var jsonBytes = Encoding.UTF8.GetBytes(json);
            string stringToHash = "POST\n" + jsonBytes.Length + "\napplication/json\n" + "x-ms-date:" + datestring + "\n/api/logs";
            string hashedString = BuildSignature(stringToHash, sharedKey);
            string signature = "SharedKey " + customerId + ":" + hashedString;

            //Console.WriteLine("" + PostData(signature, datestring, json));
            PostData(signature, datestring, json);
            Console.ReadLine();
        }

        // Build the API signature
        public static string BuildSignature(string message, string secret)
        {
            var encoding = new System.Text.ASCIIEncoding();
            byte[] keyByte = Convert.FromBase64String(secret);
            byte[] messageBytes = encoding.GetBytes(message);
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hash = hmacsha256.ComputeHash(messageBytes);
                return Convert.ToBase64String(hash);
            }
        }

        // Send a request to the POST API endpoint
        public static async void PostData(string signature, string date, string json)
        {
            try
            {
                System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();
                client.DefaultRequestHeaders.Add("accept", "application/json");
                client.DefaultRequestHeaders.Add("log-type", "syncmgTest");


                // modifiying code
                string url = "https://" + customerId + ".ods.opinsights.azure.com/api/logs?api-version=2016-04-01";

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var httpmsg = new HttpRequestMessage();
                string requestId = Guid.NewGuid().ToString();
                httpmsg.Headers.Add("X-Request-ID", requestId);
                httpmsg.Headers.Add("Authorization", signature);
                httpmsg.Headers.Add("x-ms-date", date);
                httpmsg.Content = content;
                httpmsg.Method = HttpMethod.Post;
                httpmsg.RequestUri = new Uri(url);

                var response = await client.SendAsync(httpmsg);

                Console.WriteLine(response);
                Console.ReadLine();
            }
            catch (Exception excep)
            {
                Console.WriteLine("API Post Exception: " + excep.Message);
            }
        }

    }
}

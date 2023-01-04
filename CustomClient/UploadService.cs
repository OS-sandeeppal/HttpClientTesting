using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace CustomClient
{
    public class UploadService
    {
        private readonly HttpClient _client;
        public UploadService()
        {
            var handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            };
            _client = new HttpClient(handler)
            {
                //DefaultRequestVersion = HttpVersion.Version20,
                //DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact
            };
        }
        public async Task Upload(string filePath, string uploadURL, bool useSleepTime, string baseURL, bool sendDummyRequest)
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var content = new StreamContent(stream);
        
            content.Headers.ContentLength = stream.Length;
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            var client = new HttpClient();
            
            client.DefaultRequestHeaders.ConnectionClose = false;
            client.BaseAddress = new Uri(baseURL);
            client.DefaultRequestHeaders.Connection.Add("Keep-Alive");
            client.DefaultRequestHeaders.Add("Keep-Alive","timeout=300, max=100");
            
            for (var i = 0; i < 5; i++)
            {
                if (sendDummyRequest)
                {
                    await client.GetAsync("");
                    Thread.Sleep(3000);
                }
                
                var stopwatch = Stopwatch.StartNew();
                var response = await client.PutAsync(uploadURL, content);
                
                stopwatch.Stop();
                Console.WriteLine($"Time taken: {stopwatch.Elapsed.Duration()}");

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Response headers -");
                    var headers = response.Headers.GetEnumerator();
                    while (headers.MoveNext())
                    {
                        Console.WriteLine($"header key: {headers.Current.Key} header value: {headers.Current.Value.FirstOrDefault()}");
                    }
                    Console.WriteLine($"header key: {headers.Current.Key} header value: {headers.Current.Value.FirstOrDefault()}");
                }
                else
                {
                    Console.Error.WriteLine($"Error code {response.StatusCode} , ErrorMessage: {await response.Content.ReadAsStringAsync()}");
                }
            
                if (useSleepTime)
                {
                    Thread.Sleep(10000);
                }
            }
        }
    }
}
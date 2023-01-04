// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using CustomClient;
using HttpClientTesting;
using Microsoft.Extensions.Configuration;

class Program
{
    public static async Task Main(string[] args)
    {
        // load appsettings
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        using var listener = new HttpEventListener();
        
        // read file
        var filePath = configuration["filePath"];
        var relativeUploadURL = configuration["aws:relativeUploadURL"];
        var useSleepTime = bool.Parse(configuration["useSleepTime"]);
        var usenet6 = bool.Parse(configuration["usenet6"]);
        var clientToUse = configuration["testClient"];
        var baseURL = configuration["aws:baseURL"];
        var sendDummyRequest = bool.Parse(configuration["aws:sendDummyRequest"]);
        var uploadService = new UploadService();

        if (usenet6)
        {
            await UploadFile(configuration, filePath, relativeUploadURL, useSleepTime, clientToUse, baseURL, sendDummyRequest);
        }
        else
        {
            await uploadService.Upload(filePath, relativeUploadURL, useSleepTime, baseURL, sendDummyRequest);
        }
    }

    static async Task UploadFile(IConfiguration configuration,string filePath, string uploadURL, bool useSleepTime, string clientToUse, string baseURL, bool sendDummyRequest)
    {
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        var content = new StreamContent(stream);

        content.Headers.ContentLength = stream.Length;
        content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");


        if (clientToUse.ToLower() == "azure")
        {
            var conString = configuration["azure:connectionString"];
            var containerName = configuration["azure:containerName"];
            var closeConnection = bool.Parse(configuration["azure:closeConnection"]);
            var client = new AzureClient(conString, containerName);
            for (var i = 0; i < 5; i++)
            {
                var stopwatch = Stopwatch.StartNew();
                stream.Position = 0;
                
                if (closeConnection)
                {
                    await client.UploadWithSaSUriAsync(stream);
                }
                else
                {
                    await client.UploadAsync(stream);
                }
                
                stopwatch.Stop();
                Console.WriteLine($"Time taken: {stopwatch.Elapsed.Duration()}");
                if (useSleepTime)
                {
                    Thread.Sleep(10000);
                }
            }   
        }
        else
        {
            var handler = new SocketsHttpHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                PooledConnectionLifetime = TimeSpan.FromMinutes(5),
            };
            var client = new HttpClient(handler);
            
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
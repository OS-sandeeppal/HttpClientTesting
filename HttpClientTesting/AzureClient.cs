using Azure.Storage.Blobs;
using Azure.Storage.Sas;

namespace HttpClientTesting;

public class AzureClient : ICloudClient
{
    private readonly BlobContainerClient _client;
    private readonly Uri _presignedUri;
    
    public AzureClient(string connectionString, string blobContainerName)
    {
        _client = new BlobContainerClient(connectionString, blobContainerName);
        _presignedUri = GetPreSigned(blobContainerName);
    }
    
    public async Task UploadAsync(FileStream stream)
    {
        var blobClient = _client.GetBlobClient(Guid.NewGuid().ToString());
        await blobClient.UploadAsync(stream);
    }
    
    public async Task UploadWithSaSUriAsync(FileStream stream)
    {
        var blobUriBuilder = new BlobUriBuilder(_presignedUri)
        {
            BlobName = Guid.NewGuid().ToString()
        };
        var content = new StreamContent(stream);
        
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.ConnectionClose = true;
        content.Headers.Add("x-ms-blob-type", "BlockBlob");
        
        var response = await httpClient.PutAsync(blobUriBuilder.ToUri(), content);
        
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
    }

    private Uri GetPreSigned(string containerName)
    {
        return _client.GenerateSasUri(new BlobSasBuilder(
            BlobContainerSasPermissions.Add | BlobContainerSasPermissions.Read | BlobContainerSasPermissions.Write,
            DateTimeOffset.UtcNow.Add(TimeSpan.FromMinutes(15)))
            {
                BlobContainerName = containerName
            });
    }
}
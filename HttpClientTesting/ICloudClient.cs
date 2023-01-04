namespace HttpClientTesting;

public interface ICloudClient
{
    Task UploadAsync(FileStream stream);
    Task UploadWithSaSUriAsync(FileStream stream);
}
# HttpClientTesting
Console app to measure the time taken to upload file to S3 and Azure blob storage. Total 5 requests are sent one after the other to the specified endpoint. The utility will log well known http, socket and name resolution events (https://learn.microsoft.com/en-us/dotnet/core/diagnostics/well-known-event-providers) in the console to understand which step is actually taking time.



# Steps to run the console application -

1. Compile the project in release mode. Use the dotnet 6 command line -

   dotnet build HttpClientTesting.sln --configuration release
   
2. Open the appsettings.json file from the .net6 release folder of HttpClientTesting project. Below is the description of each option in appsettings.
   
   I. **filePath** - path of the file that needs to be uploaded.
   
   II. **waitAfterEachRequest** - specify whether the client wait after each upload request. If true, then the next upload request will be sent after waiting for 10 seconds.
   
   III. **usenet6** - specify which version of HttpClient to be used. If true, uses .net 6 HttpClient else use .net standard 2.0 HttpClient.
   
   IV. **testClient** - specify whether to use Azure or AWS to upload file. Default blank is to use AWS and if set to "azure", then Azure blob storage is used.
   
   V. **aws** - has AWS specific configuration.
   
       A. baseURL - is the S3 domain with protocol like "https://test.amazonaws.com/"
      
       B. relativeUploadURL - is the S3 bucket URL
      
       C. sendDummyRequest - specify whether to send a dummy request to the S3 domain before sending the actual file upload request. If true, sends a GET request before the file upload request.
   
   VI. **azure** - has Azure specific configuration.
   
       A. connectionString - Azure blob storage connection string
       
       B. containerName - Azure blob container name in which file will be uploaded.
       
       C. closeConnection - whether to close connection between client and Azure after each request. If true, closes connection else connection remains open till Azure closes the connection.

3. After the appsettings changes are saved, run the below command -

   dotnet HttpClientTesting.dll


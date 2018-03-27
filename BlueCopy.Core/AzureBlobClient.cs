using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace BlueCopy.Core
{
  public class AzureBlobClient : IBlobClient
  {
    public string UrlPrefix { get; }

    public CloudBlobClient Client { get; }

    public AzureBlobClient(IConfiguration conf)
    {
      if(conf == null)
      {
        throw new ArgumentNullException(nameof(conf));
      }

      var urlPrefixKey = "UrlPrefix";
      var prefix = conf[urlPrefixKey] ?? throw new InvalidOperationException($"{urlPrefixKey} is not defined");

      var connectionStringKey = "ConnectionString";
      var connectionString = conf[connectionStringKey] ?? throw new InvalidOperationException($"{connectionStringKey} is not defined");
      var client = CloudStorageAccount.Parse(connectionString).CreateCloudBlobClient();

      this.UrlPrefix = prefix.TrimEnd('/').ToLower();
      this.Client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async Task<string> UploadAsync(string containerName, string path, string content)
    {
      var container = Client.GetContainerReference(containerName);
      try
      {
        await UploadAsync(container, path, content);
      }
      catch
      {
        await container.CreateAsync();
        try
        {
          var permissions = await container.GetPermissionsAsync();
          permissions.PublicAccess = BlobContainerPublicAccessType.Container;
          await container.SetPermissionsAsync(permissions);
        }
        catch
        {
          // if failed to set permissions, delete container
          await container.DeleteIfExistsAsync();

          throw;
        }

        await UploadAsync(container, path, content);
      }

      return $"{UrlPrefix}/{containerName}/{path}";
    }

    private async Task UploadAsync(CloudBlobContainer container, string id2, string content)
    {
      var blob = container.GetBlockBlobReference(id2);
      blob.Properties.ContentType = "text/html";
      await blob.UploadTextAsync(content);
    }
  }
}

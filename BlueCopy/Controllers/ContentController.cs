using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

using BlueCopy.Core;

namespace BlueCopy.Controllers
{
  [Route("api/v1/[controller]")]
  public class ContentController : Controller
  {
    public CloudBlobClient Client { get; }

    public IKeyGenerator KeyGenerator { get; }

    public string UrlPrefix { get; }

    public ContentController(IConfiguration conf, IKeyGenerator keygen)
    {
      var urlPrefixKey = "UrlPrefix";
      var prefix = conf[urlPrefixKey] ?? throw new InvalidOperationException($"{urlPrefixKey} is not defined");

      var connectionStringKey = "ConnectionString";
      var connectionString = conf[connectionStringKey] ?? throw new InvalidOperationException($"{connectionStringKey} is not defined");
      var client = CloudStorageAccount.Parse(connectionString).CreateCloudBlobClient();

      this.UrlPrefix = prefix.TrimEnd('/').ToLower();
      this.Client = client ?? throw new ArgumentNullException(nameof(client));
      this.KeyGenerator = keygen ?? throw new ArgumentNullException(nameof(keygen));
    }

    [HttpGet]
    public IActionResult Get()
    {
      return Content(@"<html>
        <head>
          <title>copy blue</title>
        </head>
        <body>
          <form action='/api/v1/content' method='POST'>
            <textarea name='content' cols='80' rows='7'></textarea>
            <input type='submit'/>
          </form>
        </body></head>", "text/html");
    }

    [HttpPost]
    public async Task<IActionResult> Post(string content)
    {
      var id = KeyGenerator.GenerateNewId();
      var id1 = id[0];
      var id2 = id[1];

      var container = Client.GetContainerReference(id1);
      try
      {
        await UploadAsync(container, id2, content);
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

        await UploadAsync(container, id2, content);
      }

      var url = $"{UrlPrefix}/{id1}/{id2}";
      return Redirect(url);
    }

    private async Task UploadAsync(CloudBlobContainer container, string id2, string content)
    {
      var blob = container.GetBlockBlobReference(id2);
      blob.Properties.ContentType = "text/html";
      await blob.UploadTextAsync(content);
    }
  }
}

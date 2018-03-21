using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace BlueCopy.Controllers
{
  [Route("api/v1/[controller]")]
  public class ContentController : Controller
  {
    public CloudBlobClient Client { get; }

    public ContentController(IConfiguration conf)
    {
      var key = "ConnectionString";
      var connectionString = conf[key] ?? throw new InvalidOperationException($"{key} is not defined");
      var client = CloudStorageAccount.Parse(connectionString).CreateCloudBlobClient();

      this.Client = client;
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
      var id = GenerateNewId();
      var id1 = id.Substring(0, 3);
      var id2 = id.Substring(3);

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

      var url = $"https://copyblue.blob.core.windows.net/{id1}/{id2}";
      return Redirect(url);
    }

    private async Task UploadAsync(CloudBlobContainer container, string id2, string content)
    {
      var blob = container.GetBlockBlobReference(id2);
      blob.Properties.ContentType = "text/html";
      await blob.UploadTextAsync(content);
    }

    private string GenerateNewId()
    {
      var now = DateTime.UtcNow;
      var num = now.Ticks - new DateTime(2018, 3, 21).Ticks;
      var id = ConvertLongToAnyBase(num, 36); // base36

      return id;
    }

    private string ConvertLongToAnyBase(long decimalNumber, ushort radix)
    {
      // http://www.pvladov.com/2012/05/decimal-to-arbitrary-numeral-system.html

      const int BitsInLong = sizeof(long) * 8; // 64
      const string Digits = "0123456789abcdefghijklmnopqrstuvwxyq";
      //const string Digits="0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

      if (radix < 2 || radix > Digits.Length)
      {
        throw new ArgumentException($"The {nameof(radix)} must be >= 2 and <= {Digits.Length}");
      }

      if (decimalNumber == 0)
      {
        return "0";
      }

      var index = BitsInLong - 1;
      var currentNumber = Math.Abs(decimalNumber);
      var charArray = new char[BitsInLong];

      while (currentNumber != 0)
      {
        var remainder = (int)(currentNumber % radix);

        charArray[index--] = Digits[remainder];
        currentNumber = currentNumber / radix;
      }

      var result = new String(charArray, index + 1, BitsInLong - index - 1);
      if (decimalNumber < 0)
      {
        result = "-" + result;
      }

      return result;
    }
  }
}

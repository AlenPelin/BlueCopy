using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using BlueCopy.Core;
using Microsoft.Extensions.Configuration;

namespace BlueCopy.Controllers
{
  [Route("api/v1/[controller]")]
  public class ContentController : Controller
  {
    public string UrlPrefix { get; }

    public IBlobClient BlobClient { get; }

    public IKeyGenerator KeyGenerator { get; }

    public ContentController(IConfiguration conf, IBlobClient blob, IKeyGenerator keygen)
    {
      if(conf == null)
      {
        throw new ArgumentNullException(nameof(conf));
      }

      var urlPrefixKey = "UrlPrefix";
      var prefix = conf[urlPrefixKey] ?? throw new InvalidOperationException($"{urlPrefixKey} is not defined");

      this.UrlPrefix = prefix.TrimEnd('/').ToLower();
      this.BlobClient = blob ?? throw new ArgumentNullException(nameof(blob));
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
    public async Task<IActionResult> Post(string content, bool redirect = true)
    {
      if(string.IsNullOrWhiteSpace(content))
      {
        return BadRequest("Empty content");
      }

      var id = KeyGenerator.GenerateNewId();
      var path = $"{id[0]}/{id[1]}";
      var url = $"{UrlPrefix}/{path}";
      content = content.Replace("%COPYBLUEURL%", url);
      content = content.Replace("%COPYBLUETITLE%", $"Copy #{path}");

      await BlobClient.UploadAsync(id[0], id[1], content);

      if (!redirect)
      {
        return Json(url);
      }

      return Redirect(url);
    }
  }
}

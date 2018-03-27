using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using BlueCopy.Core;

namespace BlueCopy.Controllers
{
  [Route("api/v1/[controller]")]
  public class ContentController : Controller
  {
    public IBlobClient BlobClient { get; }

    public IKeyGenerator KeyGenerator { get; }

    public ContentController(IBlobClient blob, IKeyGenerator keygen)
    {
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
    public async Task<IActionResult> Post(string content)
    {
      var id = KeyGenerator.GenerateNewId();
      var url = await BlobClient.UploadAsync(id[0], id[1], content);

      return Redirect(url);
    }
  }
}

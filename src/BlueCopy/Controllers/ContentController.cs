using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.WindowsAzure.Storage;

namespace BlueCopy.Controllers
{
  [Route("api/v1/[controller]")]
  public class ContentController : Controller
  {
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
      var cstr = "TODO";
      var client = CloudStorageAccount.Parse(cstr).CreateCloudBlobClient();
      var container = client.GetContainerReference("000");
      var now = DateTime.UtcNow;
      var id = now.ToString("yyyyMM/dd/hh/mm/ss/") + now.Ticks + ".html";

      var blob = container.GetBlockBlobReference(id);
      blob.Properties.ContentType = "text/html";
      await blob.UploadTextAsync(content);

      var url = "https://copyblue.blob.core.windows.net/000/" + id;
      return Redirect(url);
    }
  }
}

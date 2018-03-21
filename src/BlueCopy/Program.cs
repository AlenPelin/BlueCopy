using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BlueCopy
{
  public static class Program
  {
    public static void Main(string[] args)
    {
      BuildWebHost(args).Run();
    }

    public static IWebHost BuildWebHost(string[] args) =>
      WebHost.CreateDefaultBuilder(args)
        .BuildConfiguration()
        .UseStartup<Startup>()
        .Build();

    private static IWebHostBuilder BuildConfiguration(this IWebHostBuilder builder)
    {
      return builder.ConfigureAppConfiguration((builderContext, config) =>
      {
          IHostingEnvironment env = builderContext.HostingEnvironment;

          config
            .AddJsonFile($"appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"C:/etc/bluecopy/dev/appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"C:/etc/bluecopy/dev/appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
      });
    }
  }
}

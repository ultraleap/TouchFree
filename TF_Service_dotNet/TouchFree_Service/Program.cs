using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Ultraleap.TouchFree.Library.Configuration;

namespace Ultraleap.TouchFree.Service
{
    public class Program
    {
        static void Main(string[] args)
        {
#if !DEBUG
            TouchFreeLog.SetUpLogging();
#endif

            TouchFreeLog.WriteLine($"TouchFree Version: v{VersionManager.Version}");
            TouchFreeLog.WriteLine();

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls("http://localhost:9739");
                });
    }
}
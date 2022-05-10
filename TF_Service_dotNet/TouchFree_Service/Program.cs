#if !DEBUG
using Ultraleap.TouchFree.Library.Configuration;
#endif
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;

namespace Ultraleap.TouchFree.Service
{
    public class Program
    {
        private static string listeningInterface = "localhost";
        private static string port = "9739";

        static void Main(string[] args)
        {
#if !DEBUG
            TouchFreeLog.SetUpLogging();
#endif
            listeningInterface = Environment.GetEnvironmentVariable("TF_INTERFACE") ?? listeningInterface;
            port = Environment.GetEnvironmentVariable("TF_PORT") ?? port;
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls($"http://{listeningInterface}:{port}");
                });
    }
}
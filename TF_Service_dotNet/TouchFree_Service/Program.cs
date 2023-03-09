using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using Ultraleap.TouchFree.Library.Configuration;

namespace Ultraleap.TouchFree.Service
{
    public class Program
    {
        private static ServiceConfig config = null;

        static void Main(string[] args)
        {
#if !DEBUG
            TouchFreeLog.SetUpLogging();
#endif

            TouchFreeLog.WriteLine($"TouchFree Version: v{VersionManager.Version}");
            TouchFreeLog.WriteLine();

            config = ServiceConfigFile.LoadConfig();

            TouchFreeConfig tf = TouchFreeConfigFile.LoadConfig();
            if (!File.Exists(tf.ctiFilePath))
            {
                tf.DefaultCTI();
                TouchFreeConfigFile.SaveConfig(tf);
            }

            TouchFreeLog.WriteLine($"TouchFree IP: http://{config.ServiceIP}:{config.ServicePort}");
            TouchFreeLog.WriteLine();

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls($"http://{config.ServiceIP}:{config.ServicePort}");
#if !DEBUG
                    webBuilder.ConfigureLogging((loggingBuilder) => loggingBuilder.SetMinimumLevel(LogLevel.Warning));
#endif
                });
    }
}
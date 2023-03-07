using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;
using System;
using Ultraleap.TouchFree.Library.Configuration;
using Newtonsoft.Json.Linq;

namespace Ultraleap.TouchFree.Service
{
    public class Program
    {
        private static string serviceIP = "localhost";
        private static string servicePort = "9739";

        static void Main(string[] args)
        {
#if !DEBUG
            TouchFreeLog.SetUpLogging();
#endif

            TouchFreeLog.WriteLine($"TouchFree Version: v{VersionManager.Version}");
            TouchFreeLog.WriteLine();

            string pathToConfig = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Ultraleap\\TouchFree\\Configuration\\TrackingConfig.json");
            if (File.Exists(pathToConfig))
            {
                JObject obj = JObject.Parse(File.ReadAllText(pathToConfig));
                if (obj.ContainsKey("ServiceIP")) serviceIP = obj["ServiceIP"].ToString();
                if (obj.ContainsKey("ServicePort")) servicePort = obj["ServicePort"].ToString();
            }

            TouchFreeLog.WriteLine("TouchFree IP: http://" + serviceIP + ":" + servicePort);
            TouchFreeLog.WriteLine();

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls("http://" + serviceIP + ":" + servicePort);
#if !DEBUG
                    webBuilder.ConfigureLogging((loggingBuilder) => loggingBuilder.SetMinimumLevel(LogLevel.Warning));
#endif
                });
    }
}
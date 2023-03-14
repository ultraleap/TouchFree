using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using Ultraleap.TouchFree.Library.Configuration;
#if !BRIGHTSIGN
using Ultraleap.TouchFree.Service.Properties;
#endif

namespace Ultraleap.TouchFree.Service
{
    public class Program
    {
        private static string _ctiFolder;

        static void Main(string[] args)
        {
#if !DEBUG
            TouchFreeLog.SetUpLogging();
#endif

            TouchFreeLog.WriteLine($"TouchFree Version: v{VersionManager.Version}");
            TouchFreeLog.WriteLine();

            ServiceConfig serviceConfig = ServiceConfigFile.LoadConfig();
            InteractionConfig interactionConfig = InteractionConfigFile.LoadConfig();

#if !BRIGHTSIGN
            TouchFreeConfig tfConfig = TouchFreeConfigFile.LoadConfig();
            if (!File.Exists(tfConfig.ctiFilePath))
            {
                _ctiFolder = Path.Combine(ConfigFileUtils.ConfigFileDirectory, "CTIs");
                if (!Directory.Exists(_ctiFolder)) Directory.CreateDirectory(_ctiFolder);

                CopyCTI("AirPush_Landscape", Resources.AirPush_Landscape);
                CopyCTI("AirPush_Portrait", Resources.AirPush_Portrait);
                CopyCTI("Hover_Landscape", Resources.Hover_Landscape);
                CopyCTI("Hover_Portrait", Resources.Hover_Portrait);
                CopyCTI("TouchPlane_Landscape", Resources.TouchPlane_Landscape);
                CopyCTI("TouchPlane_Portrait", Resources.TouchPlane_Portrait);

                switch (interactionConfig.InteractionType)
                {
                    case Library.InteractionType.HOVER:
                        tfConfig.ctiFilePath = Path.Combine(_ctiFolder, "Hover_Portrait.mp4");
                        break;
                    case Library.InteractionType.TOUCHPLANE:
                        tfConfig.ctiFilePath = Path.Combine(_ctiFolder, "TouchPlane_Portrait.mp4");
                        break;
                    default:
                        tfConfig.ctiFilePath = Path.Combine(_ctiFolder, "AirPush_Portrait.mp4");
                        break;
                }
                TouchFreeLog.WriteLine($"CTI file did not exist - setting to: '{tfConfig.ctiFilePath}'");
                TouchFreeConfigFile.SaveConfig(tfConfig);
            }
#endif

            TouchFreeLog.WriteLine($"TouchFree IP: http://{serviceConfig.ServiceIP}:{serviceConfig.ServicePort}");
            TouchFreeLog.WriteLine();

            CreateHostBuilder(args, serviceConfig.ServiceIP, serviceConfig.ServicePort).Build().Run();
        }

        private static void CopyCTI(string name, byte[] content)
        {
            File.WriteAllBytes(Path.Combine(_ctiFolder, name + ".mp4"), content);
        }

        public static IHostBuilder CreateHostBuilder(string[] args, string ip, string port) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls($"http://{ip}:{port}");
#if !DEBUG
                    webBuilder.ConfigureLogging((loggingBuilder) => loggingBuilder.SetMinimumLevel(LogLevel.Warning));
#endif
                });
    }
}
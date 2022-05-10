using Ultraleap.TouchFree.Library.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace Ultraleap.TouchFree.Service
{
    public class Program
    {
        static void Main(string[] args)
        {
#if !DEBUG
            TouchFreeLog.SetUpLogging();
#endif
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webBuilder =>
                {
                    string listeningInterface = "localhost";
                    string port = "9739";

                    string filePath = ConfigFileUtils.ConfigFileDirectory + "ServiceConfig.json";
                    if (System.IO.File.Exists(filePath))
                    {
                        string json = System.IO.File.ReadAllText(filePath);
                        ServiceConfig srvConfig = JsonConvert.DeserializeObject<ServiceConfig>(json);
                        listeningInterface = srvConfig?.Interface ?? "localhost";
                        port = srvConfig?.Port ?? "9739";
                    }

                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls($"http://{listeningInterface}:{port}");
                });
    }
}
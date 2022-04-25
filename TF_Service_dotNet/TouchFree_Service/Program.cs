#if !DEBUG
using System;
using System.IO;
using System.Runtime.InteropServices;
#endif
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
            var loggingFileDirectory = Path.Combine(ConfigFileUtils.ConfigFileDirectory, "..\\Logs\\");

            if (loggingFileDirectory != "") {
                Directory.CreateDirectory(loggingFileDirectory);
            }

            FileStream filestream = new FileStream(loggingFileDirectory + "log.txt", FileMode.Create);
            StreamWriter streamwriter = new StreamWriter(filestream)
            {
                AutoFlush = true
            };
            Console.SetOut(streamwriter);
#endif
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
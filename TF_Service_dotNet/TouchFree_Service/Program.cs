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

            var filename = loggingFileDirectory + "log.txt";

            if (File.Exists(filename))
            {
                var fileInfo = new FileInfo(filename);
                var fileSize = fileInfo.Length;
                if (fileSize > 100000)
                {
                    File.Move(filename, filename.Replace("log.txt", "log_old.txt"), true);
                }
            }

            FileStream filestream = new FileStream(filename, FileMode.Append);
            StreamWriter streamwriter = new StreamWriter(filestream)
            {
                AutoFlush = true
            };

            Console.SetOut(streamwriter);

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine($"{DateTime.Now} - Starting Service");
            Console.WriteLine();
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
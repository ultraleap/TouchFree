#if !DEBUG
using System;
using System.IO;
#endif
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Ultraleap.TouchFree.Service
{
    public class Program
    {
        static void Main(string[] args)
        {
#if !DEBUG
            FileStream filestream = new FileStream("log.txt", FileMode.Create);
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
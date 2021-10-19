using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

using Ultraleap.TouchFree.Library.Configuration;

namespace Ultraleap.TouchFree.Service
{
    public class Program
    {
        static void Main(string[] args)
        {
            UpdateBehaviour updateLoop = new();

            ConfigFileWatcher configFileWatcher = new ConfigFileWatcher();
            updateLoop.OnUpdate += configFileWatcher.Update;

            CreateHostBuilder(args).Build().Run();

            Console.WriteLine("TouchFree physical config screen height is: " + ConfigManager.PhysicalConfig.ScreenHeightM);

            while (true)
            {

            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
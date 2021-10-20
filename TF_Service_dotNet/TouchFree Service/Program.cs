using System;
using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Library;

namespace Ultraleap.TouchFree.Service
{
    class Program
    {

        static void Main(string[] args)
        {
            UpdateBehaviour updateLoop = new();
            updateLoop.OnUpdate += TickTock;

            TrackingConnectionManager trackingConnectionManager = new TrackingConnectionManager();

            ConfigFileWatcher configFileWatcher = new ConfigFileWatcher();
            updateLoop.OnUpdate += configFileWatcher.Update;

            Console.WriteLine("TouchFree physical config screen height is: " + ConfigManager.PhysicalConfig.ScreenHeightM);
            while(true)
            {

            }
        }

        private static void TickTock()
        {
            //Console.WriteLine("TickTock");
        }
    }
}
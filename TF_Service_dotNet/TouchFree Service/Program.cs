using System;
using System.Timers;

namespace Ultraleap.TouchFree.Service
{
    class Program
    {

        static void Main(string[] args)
        {
            UpdateBehaviour updateLoop = new();
            updateLoop.OnUpdate += TickTock;

            Console.WriteLine("TouchFree physical config screen height is: " + Configuration.ConfigManager.PhysicalConfig.ScreenHeightM);
            while(true)
            {

            }
        }

        private static void TickTock()
        {
            Console.WriteLine("TickTock");
        }
    }
}

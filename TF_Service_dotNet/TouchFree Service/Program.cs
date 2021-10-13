using System;

namespace Ultraleap.TouchFree.Service
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("TouchFree physical config screen height is: " + Configuration.ConfigManager.PhysicalConfig.ScreenHeightM);
        }
    }
}

using System;

namespace Ultraleap.TouchFree.Service
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(Configuration.ConfigManager.PhysicalConfig.ScreenHeightM);
        }
    }
}

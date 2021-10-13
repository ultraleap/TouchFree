using System;
using Leap;

namespace Ultraleap.TouchFree.Service
{
    class Program
    {
        private Controller controller = new Controller();

        static void Main(string[] args)
        {
            new Program().Run();
        }

        void Run()
        {
            Console.WriteLine("TouchFree physical config screen height is: " + Configuration.ConfigManager.PhysicalConfig.ScreenHeightM);

            controller.FrameReady += OnFrameReady;

            while (Console.ReadKey().Key != ConsoleKey.Enter) { }
        }

        void OnFrameReady(object sender, FrameEventArgs eventArgs)
        {
            Frame frame = eventArgs.frame;

            if (frame.Hands.Count > 0)
            {
                Console.WriteLine("Palm Position = " + frame.Hands[0].PalmPosition);
            }
        }
    }
}

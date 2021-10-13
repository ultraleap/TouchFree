using System;
using System.Timers;

namespace Ultraleap.TouchFree.Service
{
    class Program
    {
        private Timer mainTimer;

        static void Main(string[] args)
        {
            var program = new Program();

            while(true) { }
        }

        Program()
        {
            Console.WriteLine(Configuration.ConfigManager.PhysicalConfig.ScreenHeightM);

            mainTimer = new Timer(1000f / 60f);

            var trackingMgr = new TrackingConnectionManager();
            //var interactionMgr = new InteractionManager();
            var clientMgr = new ClientConnectionManager(mainTimer);
            //var configWatcher = ConfigFileWatcher();



            mainTimer.Start();
        }
    }
}

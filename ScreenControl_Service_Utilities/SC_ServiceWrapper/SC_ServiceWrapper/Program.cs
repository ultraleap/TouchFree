using System.Diagnostics;
using System.Timers;
using Topshelf;

namespace SC_Service
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(serviceConfig =>
            {
                serviceConfig.Service<ServiceCore>(srv =>
                {
                    srv.ConstructUsing(core => new ServiceCore());
                    srv.WhenStarted(core => core.Start());
                    srv.WhenStopped(core => core.Stop());
                    srv.WhenShutdown(core => core.Stop());
                });
                serviceConfig.RunAsLocalSystem();
                serviceConfig.StartAutomatically();
                serviceConfig.SetServiceName("ScreenControl Service");
                serviceConfig.SetDisplayName("ScreenControl Service");
                serviceConfig.SetDescription("A Windows Service to handle ScreenControl");
            });
        }
    }

    public class ServiceCore
    {
        private Timer _timer = new Timer();
        private Process _process;

        public void Start()
        {
            _process = new Process();
            _process.StartInfo.FileName = @"../ScreenControlService/ScreenControlService.exe";
            _process.StartInfo.Arguments = "-batchmode -silent-crashes -nographics";
            _process.StartInfo.UseShellExecute = false;
            _process.StartInfo.CreateNoWindow = true;
            _process.StartInfo.RedirectStandardError = true;
            _process.StartInfo.RedirectStandardOutput = true;
            _process.StartInfo.RedirectStandardInput = true;
            _process.Start();

            _timer.Interval = 1000;
            _timer.Elapsed += TimerElapsed;
            _timer.Start();
        }
        public void Stop()
        {
            _timer.Elapsed -= TimerElapsed;
            _timer.Stop();

            if (_process != null)
            {
                _process.Kill();
            }
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (_process.HasExited) //If Unity crashes restart the application
            {
                _process.Start();
            }
        }
    }
}

﻿using System.Diagnostics;
using System.Timers;
using Topshelf;

namespace TouchFreeService
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
                serviceConfig.SetServiceName("TouchFree Service");
                serviceConfig.SetDisplayName("TouchFree Service");
                serviceConfig.SetDescription("TouchFree Service converts Ultraleap tracking" +
                    " data into a data structure suitable for input systems for touchscreen user" +
                    " interfaces via a tooling package.");
            });
        }
    }

    public class ServiceCore
    {
        private Timer _timer = new Timer();
        //private Process _process;

        public void Start()
        {
            //var userName = System.Security.Principal.WindowsIdentity.GetCurrent().User. .Token;// .Name;

            //_process = new Process();
            //_process.StartInfo.FileName = @"../Service/TouchFree_Service.exe";
            ////_process.StartInfo.Arguments = "-batchmode -silent-crashes -nographics -logFile C:\\ProgramData\\Ultraleap\\TouchFree\\Service.log";
            //_process.StartInfo.UseShellExecute = false;
            ////_process.StartInfo.CreateNoWindow = true;
            //_process.StartInfo.RedirectStandardError = true;
            //_process.StartInfo.RedirectStandardOutput = true;
            //_process.StartInfo.RedirectStandardInput = true;
            ////_process.StartInfo.UserName = "MattGray";
            //_process.Start();

            ServiceWrapper.ProcessExtensions.StartProcessAsCurrentUser(@"../Service/TouchFree_Service.exe");

            _timer.Interval = 10000;
            _timer.Elapsed += TimerElapsed;
            _timer.Start();
        }
        public void Stop()
        {
            _timer.Elapsed -= TimerElapsed;
            _timer.Stop();

            //if (_process != null)
            //{
            //    _process.Kill();
            //}

            Process[] processes = Process.GetProcessesByName("TouchFree_Service");

            foreach (var p in processes)
            {
                p.Kill();
            }
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            //if (_process.HasExited) //If Unity crashes restart the application
            //{
            //    _process.Start();
            //}
        }
    }
}

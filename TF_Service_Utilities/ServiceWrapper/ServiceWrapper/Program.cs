using System.Diagnostics;
using System.Timers;
using Topshelf;

namespace TouchFreeService;

internal static class Program
{
    private static void Main()
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
    private readonly Timer _timer = new Timer();
    private Process? _process;

    public void Start()
    {
        _process = new Process();
        _process.StartInfo.FileName = @"../Service/TouchFree_Service.exe";
        _process.StartInfo.UseShellExecute = false;
        _process.Start();

        _timer.Interval = 10000;
        _timer.Elapsed += TimerElapsed;
        _timer.Start();
    }
    public void Stop()
    {
        _timer.Elapsed -= TimerElapsed;
        _timer.Stop();
        
        _process?.Kill();
    }

    private void TimerElapsed(object? sender, ElapsedEventArgs e)
    {
        if (_process is { HasExited: true }) // If process crashed restart it
        {
            _process.Start();
        }
    }
}
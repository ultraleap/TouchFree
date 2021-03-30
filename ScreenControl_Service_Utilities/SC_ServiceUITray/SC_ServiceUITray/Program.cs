using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using System.ServiceProcess;
using Timer = System.Timers.Timer;
using System.Timers;

namespace SC_ServiceUITray
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SC_ServiceUITray());
        }
    }

    public class SC_ServiceUITray : ApplicationContext
    {
        private NotifyIcon trayIcon;
        Process startedProcess;
        ServiceController screenControlService = null;
        
        private Timer statusCheckTimer = new Timer();

        public SC_ServiceUITray()
        {
            trayIcon = new NotifyIcon()
            {
                Icon = Properties.Resources.IconActive,
                ContextMenu = new ContextMenu(new MenuItem[] {
                    new MenuItem("Settings", Settings),
                    new MenuItem("-"),
                    new MenuItem("Exit", Exit),
                }),
                Visible = true
            };

            CheckForServiceActivity(null, null);

            statusCheckTimer.Interval = 5000;
            statusCheckTimer.Elapsed += CheckForServiceActivity;
            statusCheckTimer.Start();
        }

        private void Settings(object sender, EventArgs e)
        {
            if (startedProcess != null && !startedProcess.HasExited)
            {
                // Trying to launch the Unity application will force the exsisting one to focus as we use 'Force Single Instance'
                Process.Start(System.IO.Path.GetFullPath("../Service/ScreenControlService.exe"));
            }
            else
            {
                startedProcess = Process.Start(System.IO.Path.GetFullPath("../Service/ScreenControlService.exe"));
            }
        }

        private void Exit(object sender, EventArgs e)
        {
            if(startedProcess != null && !startedProcess.HasExited)
            {
                startedProcess.Kill();
            }

            statusCheckTimer.Elapsed -= CheckForServiceActivity;
            statusCheckTimer.Stop();

            // Hide tray icon, otherwise it will remain shown until user mouses over it
            trayIcon.Visible = false;
            Application.Exit(); 
            Environment.Exit(0);
        }

        private void CheckForServiceActivity(object sender, ElapsedEventArgs e)
        {
            screenControlService = null;

            if (ServiceExists("ScreenControl Service"))
            {
                screenControlService = new ServiceController("ScreenControl Service");
            }

            if (screenControlService == null || (screenControlService != null && screenControlService.Status != ServiceControllerStatus.Running))
            {
                trayIcon.Icon = Properties.Resources.IconInactive;
                trayIcon.Text = "ScreenControl Service is not running";
            }
            else
            {
                trayIcon.Icon = Properties.Resources.IconActive;
                trayIcon.Text = "ScreenControl Service is running";
            }
        }

        private bool ServiceExists(string serviceName)
        {
            return ServiceController.GetServices().Any(serviceController => serviceController.ServiceName.Equals(serviceName));
        }
    }
}
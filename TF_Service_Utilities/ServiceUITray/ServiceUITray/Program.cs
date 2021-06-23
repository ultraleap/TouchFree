using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using System.ServiceProcess;
using Timer = System.Timers.Timer;
using System.Timers;
using System.Threading;

namespace ServiceUITray
{
    static class Program
    {
        private static Mutex mutex = null;

        [STAThread]
        static void Main()
        {
            const string appName = "TouchFree Service Tray";
            bool createdNew;

            mutex = new Mutex(true, appName, out createdNew);

            if(!createdNew)
            {
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ServiceUITray());
        }
    }

    public class ServiceUITray : ApplicationContext
    {
        private NotifyIcon trayIcon;
        Process startedProcess;
        ServiceController touchFreeService = null;

        private Timer statusCheckTimer = new Timer();

        public ServiceUITray()
        {
            trayIcon = new NotifyIcon()
            {
                Icon = Properties.Resources.IconActive,
                ContextMenu = new ContextMenu(new MenuItem[] {
                    new MenuItem("Settings", Settings),
                    new MenuItem("Settings (Admin)", SettingsAdmin),
                    new MenuItem("-"),
                    new MenuItem("Exit", Exit),
                }),
                Visible = true
            };

            trayIcon.DoubleClick += new EventHandler(Settings);

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
                ExecuteAsAdmin(System.IO.Path.GetFullPath("../ServiceUI/TouchFreeServiceUI.exe"));
            }
            else
            {
                startedProcess = ExecuteAsAdmin(System.IO.Path.GetFullPath("../ServiceUI/TouchFreeServiceUI.exe"));
            }
        }

        private void SettingsAdmin(object sender, EventArgs e)
        {
            if (startedProcess != null && !startedProcess.HasExited)
            {
                // Trying to launch the Unity application will force the exsisting one to focus as we use 'Force Single Instance'
                ExecuteAsAdmin(System.IO.Path.GetFullPath("../ServiceUI/TouchFreeServiceUI.exe"));
            }
            else
            {
                startedProcess = ExecuteAsAdmin(System.IO.Path.GetFullPath("../ServiceUI/TouchFreeServiceUI.exe"));
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
            touchFreeService = null;

            if (ServiceExists("TouchFree Service"))
            {
                touchFreeService = new ServiceController("TouchFree Service");
            }

            if (touchFreeService == null || (touchFreeService != null && touchFreeService.Status != ServiceControllerStatus.Running))
            {
                trayIcon.Icon = Properties.Resources.IconInactive;
                trayIcon.Text = "TouchFree Service is not running";
            }
            else
            {
                trayIcon.Icon = Properties.Resources.IconActive;
                trayIcon.Text = "TouchFree Service is running";
            }
        }

        private bool ServiceExists(string serviceName)
        {
            return ServiceController.GetServices().Any(serviceController => serviceController.ServiceName.Equals(serviceName));
        }

        public Process ExecuteAsAdmin(string fileName)
        {
            Process proc = new Process();
            proc.StartInfo.FileName = fileName;
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.Verb = "runas";
            proc.Start();

            return proc;
        }
    }
}
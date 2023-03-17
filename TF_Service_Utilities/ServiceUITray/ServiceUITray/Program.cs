using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using System.ServiceProcess;
using Timer = System.Timers.Timer;
using System.Timers;
using System.Threading;
using System.IO;

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

            if (!createdNew)
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
        const string SERVICE_SETTINGS_PATH = "../SettingsUI/TouchFreeSettingsUI.exe";
        const string APPLICATION_PATH = "../TouchFree/TouchFree.exe";

        private NotifyIcon trayIcon;
        private MenuItem touchFreeMenuItem;

        Process startedSettingsProcess;
        Process startedAppProcess;
        ServiceController touchFreeService = null;

        private Timer statusCheckTimer = new Timer();

        public ServiceUITray()
        {
            touchFreeMenuItem = new MenuItem("Start TouchFree", LaunchApp);

            trayIcon = new NotifyIcon()
            {
                Icon = Properties.Resources.IconActive,
                ContextMenu = new ContextMenu(new MenuItem[] {
                    touchFreeMenuItem,
                    new MenuItem("-"),
                    new MenuItem("Start Service", StartService),
                    new MenuItem("Stop Service", StopService),
                    new MenuItem("-"),
                    new MenuItem("Settings", Settings),
                }),
                Visible = true
            };

            trayIcon.DoubleClick += new EventHandler(Settings);

            CheckForServiceActivity(null, null);

            statusCheckTimer.Interval = 5000;
            statusCheckTimer.Elapsed += CheckForTouchFree;
            statusCheckTimer.Elapsed += CheckForServiceActivity;
            statusCheckTimer.Start();
        }

        private void StartService(object sender, EventArgs e)
        {
            if (touchFreeService == null) return;
            touchFreeService.Start();
        }

        private void StopService(object sender, EventArgs e)
        {
            if (touchFreeService == null) return;
            touchFreeService.Stop();
        }

        private void LaunchApp(object sender, EventArgs e)
        {
            if (startedAppProcess != null && !startedAppProcess.HasExited)
            {
                // Trying to launch the Unity application will force the exsisting one to focus as we use 'Force Single Instance'
                // LaunchApplication(Path.GetFullPath(APPLICATION_PATH));

                startedAppProcess.Kill();
            }
            else
            {
                startedAppProcess = LaunchApplication(Path.GetFullPath(APPLICATION_PATH));
            }

            CheckForTouchFree(null, null);
        }

        private void Settings(object sender, EventArgs e)
        {
            if (startedSettingsProcess != null && !startedSettingsProcess.HasExited)
            {
                // Trying to launch the Unity application will force the exsisting one to focus as we use 'Force Single Instance'
                LaunchApplication(Path.GetFullPath(SERVICE_SETTINGS_PATH));
            }
            else
            {
                startedSettingsProcess = LaunchApplication(Path.GetFullPath(SERVICE_SETTINGS_PATH));
            }
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

        private void CheckForTouchFree(object sender, ElapsedEventArgs e)
        {
            Process[] processes = Process.GetProcessesByName("TouchFree");

            if (processes != null && processes.Length > 0)
            {
                startedAppProcess = processes[0];
                touchFreeMenuItem.Text = "Stop TouchFree";
            }
            else
            {
                startedAppProcess = null;
                touchFreeMenuItem.Text = "Start TouchFree";
            }
        }

        private bool ServiceExists(string serviceName)
        {
            return ServiceController.GetServices().Any(serviceController => serviceController.ServiceName.Equals(serviceName));
        }

        public Process LaunchApplication(string fileName)
        {
            Process proc = new Process();
            proc.StartInfo.FileName = fileName;
            proc.Start();

            return proc;
        }
    }
}
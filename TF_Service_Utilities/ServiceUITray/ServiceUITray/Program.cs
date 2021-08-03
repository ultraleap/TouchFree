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
        const string SERVICE_SETTINGS_PATH = "../ServiceUI/TouchFreeServiceUI.exe";
        const string APPLICATION_PATH = "../TouchFree/TouchFree_Application.exe";

        private NotifyIcon trayIcon;
        Process startedSettingsProcess;
        Process startedAppProcess;
        ServiceController touchFreeService = null;

        private Timer statusCheckTimer = new Timer();

        public ServiceUITray()
        {
            if (File.Exists(APPLICATION_PATH))
            {
                InitializeTray(true);
            }
            else
            {
                InitializeTray(false);
            }

            trayIcon.DoubleClick += new EventHandler(Settings);

            CheckForServiceActivity(null, null);

            statusCheckTimer.Interval = 5000;
            statusCheckTimer.Elapsed += CheckForServiceActivity;
            statusCheckTimer.Start();
        }

        void InitializeTray(bool _withApplication)
        {
            if (_withApplication)
            {
                trayIcon = new NotifyIcon()
                {
                    Icon = Properties.Resources.IconActive,
                    ContextMenu = new ContextMenu(new MenuItem[] {
                    new MenuItem("Open TouchFree", LaunchApp),
                    new MenuItem("Open TouchFree (Admin)", LaunchAppAdmin),
                    new MenuItem("-"),
                    new MenuItem("Open Settings", Settings),
                    new MenuItem("Open Settings (Admin)", SettingsAdmin),
                    new MenuItem("-"),
                    new MenuItem("Exit", Exit),
                }),
                    Visible = true
                };
            }
            else
            {
                trayIcon = new NotifyIcon()
                {
                    Icon = Properties.Resources.IconActive,
                    ContextMenu = new ContextMenu(new MenuItem[] {
                    new MenuItem("Open Settings", Settings),
                    new MenuItem("Open Settings (Admin)", SettingsAdmin),
                    new MenuItem("-"),
                    new MenuItem("Exit", Exit),
                }),
                    Visible = true
                };
            }
        }

        private void LaunchApp(object sender, EventArgs e)
        {
            if (startedAppProcess != null && !startedAppProcess.HasExited)
            {
                // Trying to launch the Unity application will force the exsisting one to focus as we use 'Force Single Instance'
                ExecuteWithoutAdmin(Path.GetFullPath(APPLICATION_PATH));
            }
            else
            {
                startedAppProcess = ExecuteWithoutAdmin(Path.GetFullPath(APPLICATION_PATH));
            }
        }

        private void LaunchAppAdmin(object sender, EventArgs e)
        {
            if (startedAppProcess != null && !startedAppProcess.HasExited)
            {
                // Trying to launch the Unity application will force the exsisting one to focus as we use 'Force Single Instance'
                ExecuteAsAdmin(Path.GetFullPath(APPLICATION_PATH));
            }
            else
            {
                startedAppProcess = ExecuteAsAdmin(Path.GetFullPath(APPLICATION_PATH));
            }
        }

        private void Settings(object sender, EventArgs e)
        {
            if (startedSettingsProcess != null && !startedSettingsProcess.HasExited)
            {
                // Trying to launch the Unity application will force the exsisting one to focus as we use 'Force Single Instance'
                ExecuteWithoutAdmin(Path.GetFullPath(SERVICE_SETTINGS_PATH));
            }
            else
            {
                startedSettingsProcess = ExecuteWithoutAdmin(Path.GetFullPath(SERVICE_SETTINGS_PATH));
            }
        }

        private void SettingsAdmin(object sender, EventArgs e)
        {
            if (startedSettingsProcess != null && !startedSettingsProcess.HasExited)
            {
                // Trying to launch the Unity application will force the exsisting one to focus as we use 'Force Single Instance'
                ExecuteAsAdmin(Path.GetFullPath(SERVICE_SETTINGS_PATH));
            }
            else
            {
                startedSettingsProcess = ExecuteAsAdmin(Path.GetFullPath(SERVICE_SETTINGS_PATH));
            }
        }

        private void Exit(object sender, EventArgs e)
        {
            if(startedSettingsProcess != null && !startedSettingsProcess.HasExited)
            {
                startedSettingsProcess.Kill();
            }

            if (startedAppProcess != null && !startedAppProcess.HasExited)
            {
                startedAppProcess.Kill();
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

        public Process ExecuteWithoutAdmin(string fileName)
        {
            Process proc = new Process();
            proc.StartInfo.FileName = fileName;
            proc.Start();

            return proc;
        }
    }
}
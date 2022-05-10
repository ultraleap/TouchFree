using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using Timer = System.Timers.Timer;

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
        private ToolStripItem touchFreeMenuItem;

        Process startedSettingsProcess;
        Process startedAppProcess;
        ServiceController touchFreeService = null;

        private Timer statusCheckTimer = new Timer();

        public ServiceUITray()
        {
            var menuStrip = new ContextMenuStrip();
            touchFreeMenuItem = menuStrip.Items.Add("Start TouchFree", null, LaunchApp);
            menuStrip.Items.Add("-");
            menuStrip.Items.Add("Settings", null, Settings);

            trayIcon = new NotifyIcon()
            {
                Icon = Properties.Resources.IconActive,
                ContextMenuStrip = menuStrip,
                Visible = true
            };

            trayIcon.DoubleClick += new EventHandler(Settings);

            CheckForServiceActivity(null, null);

            statusCheckTimer.Interval = 5000;
            statusCheckTimer.Elapsed += CheckForTouchFree;
            statusCheckTimer.Elapsed += CheckForServiceActivity;
            statusCheckTimer.Start();
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
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace Ultraleap.TouchFree
{
    public class HomeScreen : MonoBehaviour
    {
        public GameObject userInterfaceSettingsScreen;

        public Text versionText;
        string versionPath;
        Process startedProcess;

        private string configFileDirectory = null;
        private readonly string DefaultConfigDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Ultraleap\\ScreenControl\\Configuration\\");

        private void Awake()
        {
            versionPath = Path.Combine(Application.dataPath, "../Version.txt");
            PopulateVersion();
        }

        void PopulateVersion()
        {
            string version = "N/A";

            if (File.Exists(versionPath))
            {
                var fileLines = File.ReadAllLines(versionPath);
                foreach (var line in fileLines)
                {
                    if (line.Contains("TouchFree Version"))
                    {
                        version = line.Replace("TouchFree Version: ", "");
                        break;
                    }
                }
            }

            versionText.text = "Version " + version;
        }

        public void OpenServiceUI()
        {
            if (startedProcess != null && !startedProcess.HasExited)
            {
                // Trying to launch the Unity application will force the exsisting one to focus as we use 'Force Single Instance'
                ExecuteAsAdmin(System.IO.Path.GetFullPath("../ServiceUI/ScreenControlServiceUI.exe"));
            }
            else
            {
                startedProcess = ExecuteAsAdmin(System.IO.Path.GetFullPath("../ServiceUI/ScreenControlServiceUI.exe"));
            }
        }

        private Process ExecuteAsAdmin(string fileName)
        {
            Process proc = new Process();
            proc.StartInfo.FileName = fileName;
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.Verb = "runas";
            proc.Start();

            return proc;
        }

        void GetConfigFileDirectory()
        {
            // Computer\HKEY_LOCAL_MACHINE\SOFTWARE\Ultraleap\ScreenControl\Service\Settings
            // Check registry for override to default directory
            RegistryKey regKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Ultraleap\ScreenControl\Service\Settings");

            if (regKey != null)
            {
                var pathObj = regKey.GetValue("ConfigFileDirectory");

                if (pathObj != null)
                {
                    string path = pathObj.ToString();

                    if (Directory.Exists(path))
                    {
                        regKey.Close();
                        configFileDirectory = path;
                        return;
                    }
                }

                regKey.Close();
            }

            // else
            configFileDirectory = DefaultConfigDirectory;
        }

        public void ChangeToUserInterfaceSettings()
        {
            ScreenManager.Instance.ChangeScreen(userInterfaceSettingsScreen);
        }
    }
}
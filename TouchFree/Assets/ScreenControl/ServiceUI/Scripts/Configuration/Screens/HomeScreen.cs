using System;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Microsoft.Win32;

namespace Ultraleap.ScreenControl.Core
{
    public class HomeScreen : MonoBehaviour
    {
        [Space]
        public GameObject userInterfaceSettingsScreen;

        public Text versionText;
        string versionPath;

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
                    if (line.Contains("ScreenControl Service Version"))
                    {
                        version = line.Replace("ScreenControl Service Version: ", "");
                        break;
                    }
                }
            }
            versionText.text = "Version " + version;
        }

        void GetConfigFileDirectory()
        {
            // Computer\HKEY_LOCAL_MACHINE\SOFTWARE\Ultraleap\ScreenControl\Service\Settings
            // Check registry for override to default directory
            RegistryKey regKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Ultraleap\ScreenControl\Service\Settings");

            if(regKey != null)
            {
                var pathObj = regKey.GetValue("ConfigFileDirectory");

                if(pathObj != null)
                {
                    string path = pathObj.ToString();

                    if(Directory.Exists(path))
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


        public void ChangeToSetupCamera()
        {
            Debug.Log("Need to launch Service UI here");
        }

        public void ChangeToUserInterfaceSettings()
        {
            ScreenManager.Instance.ChangeScreen(userInterfaceSettingsScreen);
        }
    }
}
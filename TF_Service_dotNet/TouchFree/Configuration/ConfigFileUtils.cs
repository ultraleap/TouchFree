using System.IO;
using System;
using Microsoft.Win32;

namespace Ultraleap.TouchFree.Service.Configuration
{
    public static class ConfigFileUtils
    {
        static string configFileDirectory = null;
        public static string ConfigFileDirectory
        {
            get
            {
                if (configFileDirectory == null)
                {
                    GetConfigFileDirectory();
                }

                return configFileDirectory;
            }
            set
            {
                configFileDirectory = value;
            }
        }

        static readonly string DefaultConfigDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Ultraleap\\TouchFree\\Configuration\\");

        public static void CheckForConfigDirectoryChange()
        {
            GetConfigFileDirectory();
        }

        static void GetConfigFileDirectory()
        {
            // Computer\HKEY_LOCAL_MACHINE\SOFTWARE\Ultraleap\TouchFree\Service\Settings
            // Check registry for override to default directory
            RegistryKey regKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Ultraleap\TouchFree\Service\Settings");

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
    }
}
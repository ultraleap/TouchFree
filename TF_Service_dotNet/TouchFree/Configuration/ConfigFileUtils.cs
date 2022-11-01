using Microsoft.Win32;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Ultraleap.TouchFree.Library.Configuration
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

        static readonly string DefaultConfigDirectory = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ?
            Path.GetFullPath("/storage/sd/ultraleap/touchfree/configuration/") :
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Ultraleap\\TouchFree\\Configuration\\");

        public static void CheckForConfigDirectoryChange()
        {
            GetConfigFileDirectory();
        }

        static void GetConfigFileDirectory()
        {
            // Computer\HKEY_LOCAL_MACHINE\SOFTWARE\Ultraleap\TouchFree\Service\Settings
            // Check registry for override to default directory
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
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
            }

            // else
            configFileDirectory = DefaultConfigDirectory;
        }
    }
}
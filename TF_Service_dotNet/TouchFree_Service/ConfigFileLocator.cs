using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Ultraleap.TouchFree.Library.Configuration;

namespace Ultraleap.TouchFree.Service
{
    public class ConfigFileLocator : IConfigFileLocator
    {
        string configFileDirectory = null;
        public string ConfigFileDirectory
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

        public void ReloadConfigFileDirectoryFromRegistry()
        {
            GetConfigFileDirectory();
        }

        void GetConfigFileDirectory()
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
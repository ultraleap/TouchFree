using UnityEngine;
using System.IO;
using System;
using Microsoft.Win32;

namespace Ultraleap.ScreenControl.Core
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

        static readonly string DefaultConfigDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Ultraleap/ScreenControl/Configuration/");

        public static void CheckForConfigDirectoryChange()
        {
            GetConfigFileDirectory();
        }

        static void GetConfigFileDirectory()
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
                        configFileDirectory = path;
                        return;
                    }
                }
            }

            // else
            configFileDirectory = DefaultConfigDirectory;
        }

        public static bool ChangeConfigFileDirectory(string _newPath, bool _makeNew = false)
        {
            if(Directory.Exists(_newPath))
            {
                MoveConfigDirectory(_newPath, configFileDirectory);
                return true;
            }
            else
            {
                if (_makeNew)
                {
                    Directory.CreateDirectory(_newPath);
                    MoveConfigDirectory(_newPath, configFileDirectory);
                    return true;
                }
                else
                {
                    Debug.LogError($"The target path {_newPath} does not exist. The configuration path was not changed.");
                    return false;
                }
            }
        }

        static void MoveConfigDirectory(string _newPath, string _oldPath)
        {
            // set the new directory path in the registry
            RegistryKey regKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Ultraleap\ScreenControl\Service\Settings");

            if (regKey == null)
            {
                // make a new one
                regKey = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Ultraleap\ScreenControl\Service\Settings");
            }

            regKey.SetValue("ConfigFileDirectory", _newPath);

            // copy the files from the current location to the new one
            Directory.Move(_oldPath, _newPath);
        }
    }
}
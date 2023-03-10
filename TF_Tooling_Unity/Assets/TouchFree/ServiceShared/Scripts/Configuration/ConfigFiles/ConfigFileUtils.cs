using UnityEngine;
using System.IO;
using System;
using Microsoft.Win32;

namespace Ultraleap.TouchFree.ServiceShared
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
            RegistryKey regKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Ultraleap\TouchFree\Service\Settings", true);

            if (regKey == null)
            {
                // make a new one
                regKey = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Ultraleap\TouchFree\Service\Settings", true);
            }

            regKey.SetValue("ConfigFileDirectory", _newPath);
            regKey.Close();

            //// copy the files from the current location to the new one
            DirectoryInfo dir = new DirectoryInfo(_oldPath);
            FileInfo[] fileInfos = dir.GetFiles();

            foreach (FileInfo file in fileInfos)
            {
                string path = Path.Combine(_newPath, file.Name);

                if (!File.Exists(path))
                {
                    file.CopyTo(path);
                }
            }

            ConfigManager.SaveAllConfigs();
            GetConfigFileDirectory();
        }
    }
}
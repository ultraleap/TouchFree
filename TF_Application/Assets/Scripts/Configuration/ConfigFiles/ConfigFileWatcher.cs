using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace Ultraleap.TouchFree
{
    public class ConfigFileWatcher : MonoBehaviour
    {
        private FileSystemWatcher serviceWatcher;
        private FileSystemWatcher TFConfigWatcher;

        bool fileChanged = false;
        bool configUpdated = false;

        private void Start()
        {
            // We watch for changes in the Service's physical config, in case the files
            // are relocated via the Service
            serviceWatcher = new FileSystemWatcher();
            serviceWatcher.Path = ConfigFileUtils.ConfigFileDirectory;
            serviceWatcher.NotifyFilter = NotifyFilters.LastWrite;
            serviceWatcher.NotifyFilter = NotifyFilters.LastAccess;
            serviceWatcher.Filter = "PhysicalConfig.json";
            serviceWatcher.Changed += new FileSystemEventHandler(FileUpdated);
            serviceWatcher.IncludeSubdirectories = true;
            serviceWatcher.EnableRaisingEvents = true;

            TFConfigWatcher = new FileSystemWatcher();
            TFConfigWatcher.Path = ConfigFileUtils.ConfigFileDirectory;
            TFConfigWatcher.NotifyFilter = NotifyFilters.LastWrite;
            TFConfigWatcher.NotifyFilter = NotifyFilters.LastAccess;
            TFConfigWatcher.Filter = "TouchFreeConfig.json";
            TFConfigWatcher.Changed += new FileSystemEventHandler(TFConfigUpdated);
            TFConfigWatcher.IncludeSubdirectories = true;
            TFConfigWatcher.EnableRaisingEvents = true;
        }

        private void Update()
        {
            if (fileChanged)
            {
                var previousPath = ConfigFileUtils.ConfigFileDirectory;
                ConfigFileUtils.CheckForConfigDirectoryChange();
                fileChanged = false;

                if (previousPath != ConfigFileUtils.ConfigFileDirectory)
                {
                    serviceWatcher.Path = ConfigFileUtils.ConfigFileDirectory;
                    TFConfigWatcher.Path = ConfigFileUtils.ConfigFileDirectory;
                    ConfigManager.Config = TouchFreeConfigFile.LoadConfig();
                }
            }

            if(configUpdated)
            {
                configUpdated = false;
                ConfigManager.Config = TouchFreeConfigFile.LoadConfig();
                ConfigManager.ConfigWasUpdated();
            }
        }

        private void FileUpdated(object source, FileSystemEventArgs e)
        {
            // save that it changed, this is on a thread so needs the reaction to be thread safe
            fileChanged = true;
        }

        private void TFConfigUpdated(object source, FileSystemEventArgs e)
        {
            // save that it changed, this is on a thread so needs the reaction to be thread safe
            configUpdated = true;
        }
    }
}
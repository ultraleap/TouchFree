using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace Ultraleap.TouchFree
{
    public class ConfigFileWatcher : MonoBehaviour
    {
        private FileSystemWatcher touchfreeWatcher;
        private FileSystemWatcher serviceWatcher;

        bool fileChanged = false;

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
                    ConfigManager.Config = TouchFreeConfigFile.LoadConfig();
                }
            }
        }

        private void FileUpdated(object source, FileSystemEventArgs e)
        {
            Debug.Log("observed a file change in: " + e.Name);
            // save that it changed, this is on a thread so needs the reaction to be thread safe
            fileChanged = true;
        }
    }
}
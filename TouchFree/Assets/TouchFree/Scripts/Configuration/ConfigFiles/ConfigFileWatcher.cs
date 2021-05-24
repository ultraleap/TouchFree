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
            touchfreeWatcher = new FileSystemWatcher();
            touchfreeWatcher.Path = ConfigFileUtils.ConfigFileDirectory;
            touchfreeWatcher.NotifyFilter = NotifyFilters.LastWrite;
            touchfreeWatcher.NotifyFilter = NotifyFilters.LastAccess;
            touchfreeWatcher.Filter = TouchFreeConfigFile.ConfigFileName;
            touchfreeWatcher.Changed += new FileSystemEventHandler(FileUpdated);
            touchfreeWatcher.IncludeSubdirectories = true;
            touchfreeWatcher.EnableRaisingEvents = true;

            // We watch for changes in the Service's physical config too, in case the files
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
                ConfigFileUtils.CheckForConfigDirectoryChange();
                touchfreeWatcher.Path = ConfigFileUtils.ConfigFileDirectory;
                serviceWatcher.Path = ConfigFileUtils.ConfigFileDirectory;
                fileChanged = false;

                ConfigManager.Config = TouchFreeConfigFile.LoadConfig();
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
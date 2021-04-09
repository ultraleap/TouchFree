using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace Ultraleap.ScreenControl.Core
{
    public class ConfigFileWatcher : MonoBehaviour
    {
        private FileSystemWatcher interactionWatcher;
        private FileSystemWatcher physicalWatcher;

        bool fileChanged = false;

        private void Start()
        {
            interactionWatcher = new FileSystemWatcher();
            interactionWatcher.Path = InteractionConfigFile.ConfigFileDirectory;
            interactionWatcher.NotifyFilter = NotifyFilters.LastWrite;
            interactionWatcher.Filter = InteractionConfigFile.ConfigFileNameS;
            interactionWatcher.Changed += new FileSystemEventHandler(FileUpdated);

            interactionWatcher.IncludeSubdirectories = true;
            interactionWatcher.EnableRaisingEvents = true;


            physicalWatcher = new FileSystemWatcher();
            physicalWatcher.Path = PhysicalConfigFile.ConfigFileDirectory;
            physicalWatcher.NotifyFilter = NotifyFilters.LastWrite;
            physicalWatcher.Filter = PhysicalConfigFile.ConfigFileNameS;
            physicalWatcher.Changed += new FileSystemEventHandler(FileUpdated);

            physicalWatcher.IncludeSubdirectories = true;
            physicalWatcher.EnableRaisingEvents = true;
        }

        private void Update()
        {
            if(fileChanged)
            {
                fileChanged = false;
                ConfigManager.InitialiseConfigs();
                ConfigManager.InteractionConfig.ConfigWasUpdated();
                ConfigManager.PhysicalConfig.ConfigWasUpdated();
                ConfigManager.GlobalSettings.CreateVirtualScreen();
            }
        }

        private void FileUpdated(object source, FileSystemEventArgs e)
        {
            // save that it changed, this is on a thread so needs the reaction to be thread safe
            fileChanged = true;
        }
    }
}
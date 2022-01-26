using System;
using System.Timers;
using System.IO;

namespace Ultraleap.TouchFree.Library.Configuration
{
    public class ConfigFileWatcher
    {
        private readonly IConfigManager configManager;
        private FileSystemWatcher interactionWatcher;
        private FileSystemWatcher physicalWatcher;

        private bool configFileChanged = false;

        public ConfigFileWatcher(IConfigManager _configManager)
        {
            configManager = _configManager;

            interactionWatcher = new FileSystemWatcher();
            interactionWatcher.Path = ConfigFileUtils.ConfigFileDirectory;
            interactionWatcher.NotifyFilter = NotifyFilters.LastWrite;
            interactionWatcher.NotifyFilter = NotifyFilters.LastAccess;
            interactionWatcher.Filter = InteractionConfigFile.ConfigFileName;
            interactionWatcher.Changed += new FileSystemEventHandler(FileUpdated);
            interactionWatcher.IncludeSubdirectories = true;
            interactionWatcher.EnableRaisingEvents = true;

            physicalWatcher = new FileSystemWatcher();
            physicalWatcher.Path = ConfigFileUtils.ConfigFileDirectory;
            physicalWatcher.NotifyFilter = NotifyFilters.LastWrite;
            physicalWatcher.NotifyFilter = NotifyFilters.LastAccess;
            physicalWatcher.Filter = PhysicalConfigFile.ConfigFileName;
            physicalWatcher.Changed += new FileSystemEventHandler(FileUpdated);
            physicalWatcher.IncludeSubdirectories = true;
            physicalWatcher.EnableRaisingEvents = true;
        }

        public void Update()
        {
            if(configFileChanged)
            {
                try
                {
                    ConfigFileUtils.CheckForConfigDirectoryChange();
                    interactionWatcher.Path = ConfigFileUtils.ConfigFileDirectory;
                    physicalWatcher.Path = ConfigFileUtils.ConfigFileDirectory;
                    configManager.LoadConfigsFromFiles();
                    Console.WriteLine("A config file was changed. Re-loading configs from files.");
                    configFileChanged = false;
                }
                catch
                {
                }
            }
        }

        private void FileUpdated(object source, FileSystemEventArgs e)
        {
            configFileChanged = true;
        }
    }
}
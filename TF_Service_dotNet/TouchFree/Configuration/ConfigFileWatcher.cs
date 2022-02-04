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
            // We ask the config manager for references for these as this will cause the
            // files to be created if they don't already exist, and FileSystemWatchers will
            // error if the file they need to watch does not exist.
            InteractionConfig InteractionCfg = _configManager.InteractionConfig;
            PhysicalConfig PhysicalCfg = _configManager.PhysicalConfig;

            configManager = _configManager;

            interactionWatcher = new FileSystemWatcher();
            interactionWatcher.Path = ConfigFileUtils.ConfigFileDirectory;
            interactionWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.LastAccess;
            interactionWatcher.Filter = InteractionConfigFile.ConfigFileName;
            interactionWatcher.Changed += new FileSystemEventHandler(FileUpdated);
            interactionWatcher.IncludeSubdirectories = true;
            interactionWatcher.EnableRaisingEvents = true;

            physicalWatcher = new FileSystemWatcher();
            physicalWatcher.Path = ConfigFileUtils.ConfigFileDirectory;
            physicalWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.LastAccess;
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
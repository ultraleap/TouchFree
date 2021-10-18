using System;
using System.IO;

namespace Ultraleap.TouchFree.Library.Configuration
{
    public class ConfigFileWatcher
    {
        private FileSystemWatcher interactionWatcher;
        private FileSystemWatcher physicalWatcher;

        public ConfigFileWatcher()
        {
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

        private void UpdateConfigs()
        {
            ConfigFileUtils.CheckForConfigDirectoryChange();
            interactionWatcher.Path = ConfigFileUtils.ConfigFileDirectory;
            physicalWatcher.Path = ConfigFileUtils.ConfigFileDirectory;
            ConfigManager.LoadConfigsFromFiles();
            ConfigManager.InteractionConfig.ConfigWasUpdated();
            ConfigManager.PhysicalConfig.ConfigWasUpdated();

            Console.WriteLine("A config file was changed. Re-loading configs from files.");
        }

        private void FileUpdated(object source, FileSystemEventArgs e)
        {
            var configChangedDelegate = new Action(delegate()
            {
                UpdateConfigs();
            });
            configChangedDelegate.Invoke();
        }
    }
}
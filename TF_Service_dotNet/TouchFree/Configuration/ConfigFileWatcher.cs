using System;
using System.IO;

namespace Ultraleap.TouchFree.Library.Configuration
{
    public class ConfigFileWatcher
    {
        private readonly IConfigManager configManager;
        private readonly FileSystemWatcher interactionWatcher;
        private readonly FileSystemWatcher physicalWatcher;
        private readonly FileSystemWatcher trackingWatcher;
        private readonly FileSystemWatcher trackingLoggingWatcher;
        private int trackingWatcherIgnoreEventCount;

        private bool configFileChanged = false;
        private readonly object loadSyncRoot = new();

        public ConfigFileWatcher(IUpdateBehaviour updateBehaviour, IConfigManager _configManager)
        {
            // We ask the config manager for references for these as this will cause the
            // files to be created if they don't already exist, and FileSystemWatchers will
            // error if the file they need to watch does not exist.
            InteractionConfigInternal InteractionCfg = _configManager.InteractionConfig;
            PhysicalConfigInternal PhysicalCfg = _configManager.PhysicalConfig;
            TrackingConfig TrackingCfg = _configManager.TrackingConfig;
            TrackingLoggingConfig TrackingLoggingCfg = _configManager.TrackingLoggingConfig;

            configManager = _configManager;
            configManager.OnTrackingConfigSaved += _ => trackingWatcherIgnoreEventCount++;

            interactionWatcher = InteractionConfigFile.CreateWatcher(FileUpdated);
            physicalWatcher = PhysicalConfigFile.CreateWatcher(FileUpdated);
            trackingWatcher = TrackingConfigFile.CreateWatcher(FileUpdated);
            trackingLoggingWatcher = TrackingLoggingConfigFile.CreateWatcher(FileUpdated);

            updateBehaviour.OnUpdate += Update;
        }

        private void Update()
        {
            lock (loadSyncRoot)
            {
                if (configFileChanged)
                {
                    ConfigFileUtils.CheckForConfigDirectoryChange();
                    // Refresh watcher paths
                    interactionWatcher.Path = InteractionConfigFile.ConfigFileDirectory;
                    physicalWatcher.Path = PhysicalConfigFile.ConfigFileDirectory;
                    trackingWatcher.Path = TrackingConfigFile.ConfigFileDirectory;
                    trackingLoggingWatcher.Path = TrackingLoggingConfigFile.ConfigFileDirectory;
                    configManager.LoadConfigsFromFiles();
                    TouchFreeLog.WriteLine("A config file was changed. Re-loading configs from files.");
                    configFileChanged = false;
                }
            }
        }

        private void FileUpdated(object source, FileSystemEventArgs e)
        {
            lock (TrackingConfigFile.SaveConfigSyncRoot)
            {
                if (e.Name == TrackingConfigFile.ConfigFileName && trackingWatcherIgnoreEventCount > 0)
                {
                    trackingWatcherIgnoreEventCount--;
                    return;
                }

                lock (loadSyncRoot)
                {
                    configFileChanged = true;
                }
            }
        }
    }
}
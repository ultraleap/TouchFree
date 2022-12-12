using System.IO;

namespace Ultraleap.TouchFree.Library.Configuration
{
    public class ConfigFileWatcher
    {
        private readonly IConfigManager configManager;
        private readonly FileSystemWatcher interactionWatcher;
        private readonly FileSystemWatcher physicalWatcher;
        private readonly FileSystemWatcher trackingWatcher;
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

            configManager = _configManager;
            configManager.OnTrackingConfigSaved += _ => trackingWatcherIgnoreEventCount++;

            interactionWatcher = CreateWatcherForFile(InteractionConfigFile.ConfigFileName);
            physicalWatcher = CreateWatcherForFile(PhysicalConfigFile.ConfigFileName);
            trackingWatcher = CreateWatcherForFile(TrackingConfigFile.ConfigFileName);

            updateBehaviour.OnUpdate += Update;
        }

        public FileSystemWatcher CreateWatcherForFile(string fileName)
        {
            var fileWatcher = new FileSystemWatcher();
            fileWatcher.Path = ConfigFileUtils.ConfigFileDirectory;
            fileWatcher.NotifyFilter = NotifyFilters.LastWrite;
            fileWatcher.Filter = fileName;
            fileWatcher.Changed += FileUpdated;
            fileWatcher.IncludeSubdirectories = true;
            fileWatcher.EnableRaisingEvents = true;

            return fileWatcher;
        }

        private void Update()
        {
            lock (loadSyncRoot)
            {
                if (configFileChanged)
                {
                    ConfigFileUtils.CheckForConfigDirectoryChange();
                    interactionWatcher.Path = ConfigFileUtils.ConfigFileDirectory;
                    physicalWatcher.Path = ConfigFileUtils.ConfigFileDirectory;
                    trackingWatcher.Path = ConfigFileUtils.ConfigFileDirectory;
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
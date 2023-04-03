using System.IO;

namespace Ultraleap.TouchFree.Library.Configuration;

public class ConfigFileWatcher
{
    private readonly IConfigManager _configManager;
    private readonly FileSystemWatcher _interactionWatcher;
    private readonly FileSystemWatcher _physicalWatcher;
    private readonly FileSystemWatcher _trackingWatcher;
    private readonly FileSystemWatcher _serviceWatcher;
    private readonly FileSystemWatcher _tfWatcher;
    private int _trackingWatcherIgnoreEventCount;

    private bool _configFileChanged = false;
    private readonly object _loadSyncRoot = new();

    public ConfigFileWatcher(IUpdateBehaviour updateBehaviour, IConfigManager configManager)
    {
        // We ask the config manager for references for these as this will cause the
        // files to be created if they don't already exist, and FileSystemWatchers will
        // error if the file they need to watch does not exist.
        InteractionConfigInternal interactionCfg = configManager.InteractionConfig;
        PhysicalConfigInternal physicalCfg = configManager.PhysicalConfig;
        TrackingConfig trackingCfg = configManager.TrackingConfig;
        ServiceConfig serviceCfg = configManager.ServiceConfig;
        TouchFreeConfig tfConfig = configManager.TouchFreeConfig;

        _configManager = configManager;
        _configManager.OnTrackingConfigSaved += _ => _trackingWatcherIgnoreEventCount++;

        _interactionWatcher = CreateWatcherForFile(InteractionConfigFile.ConfigFileName);
        _physicalWatcher = CreateWatcherForFile(PhysicalConfigFile.ConfigFileName);
        _trackingWatcher = CreateWatcherForFile(TrackingConfigFile.ConfigFileName);
        _serviceWatcher = CreateWatcherForFile(ServiceConfigFile.ConfigFileName);
        _tfWatcher = CreateWatcherForFile(TouchFreeConfigFile.ConfigFileName);

        updateBehaviour.OnUpdate += Update;
    }

    private FileSystemWatcher CreateWatcherForFile(string fileName)
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
        lock (_loadSyncRoot)
        {
            if (_configFileChanged)
            {
                ConfigFileUtils.CheckForConfigDirectoryChange();
                _interactionWatcher.Path = ConfigFileUtils.ConfigFileDirectory;
                _physicalWatcher.Path = ConfigFileUtils.ConfigFileDirectory;
                _trackingWatcher.Path = ConfigFileUtils.ConfigFileDirectory;
                _serviceWatcher.Path = ConfigFileUtils.ConfigFileDirectory;
                _tfWatcher.Path = ConfigFileUtils.ConfigFileDirectory;
                _configManager.LoadConfigsFromFiles();
                TouchFreeLog.WriteLine("A config file was changed. Re-loading configs from files.");
                _configFileChanged = false;
            }
        }
    }

    private void FileUpdated(object source, FileSystemEventArgs e)
    {
        lock (TrackingConfigFile.SaveConfigSyncRoot)
        {
            if (e.Name == TrackingConfigFile.ConfigFileName && _trackingWatcherIgnoreEventCount > 0)
            {
                _trackingWatcherIgnoreEventCount--;
                return;
            }

            lock (_loadSyncRoot)
            {
                _configFileChanged = true;
            }
        }
    }
}
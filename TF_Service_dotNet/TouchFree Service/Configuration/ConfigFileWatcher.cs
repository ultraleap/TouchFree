using System.IO;
using System.Timers;

namespace Ultraleap.TouchFree.Service.Configuration
{
    public class ConfigFileWatcher
    {
        private FileSystemWatcher interactionWatcher;
        private FileSystemWatcher physicalWatcher;

        bool fileChanged = false;

        private Timer mainTimer;

        public ConfigFileWatcher(Timer _timer)
        {
            mainTimer = _timer;

            mainTimer.Elapsed += UpdateTimer_Elapsed;

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

        void UpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (fileChanged)
            {
                ConfigFileUtils.CheckForConfigDirectoryChange();
                interactionWatcher.Path = ConfigFileUtils.ConfigFileDirectory;
                physicalWatcher.Path = ConfigFileUtils.ConfigFileDirectory;
                fileChanged = false;
                ConfigManager.LoadConfigsFromFiles();
                ConfigManager.InteractionConfig.ConfigWasUpdated();
                ConfigManager.PhysicalConfig.ConfigWasUpdated();
            }
        }

        private void FileUpdated(object source, FileSystemEventArgs e)
        {
            // save that it changed, this is on a thread so needs the reaction to be thread safe
            fileChanged = true;
        }
    }
}
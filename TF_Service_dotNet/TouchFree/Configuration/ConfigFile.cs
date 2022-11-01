using Newtonsoft.Json;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Ultraleap.TouchFree.Library.Configuration
{
    public abstract class ConfigFile<TData, TThisClass>
        where TData : class, new()
        where TThisClass : ConfigFile<TData, TThisClass>, new()
    {
        #region Singleton

        private static TThisClass _instance;
        protected static TThisClass Instance => _instance ??= new TThisClass();

        #endregion

        #region Public

        // ReSharper disable once StaticMemberInGenericType - by design, each derived type will have its own sync root
        // NOTE: If multiple derived classes use the same types they will share a sync root!
        public static readonly object SaveConfigSyncRoot = new();

        public static event Action OnConfigFileSaved
        {
            add => Instance._OnConfigFileSaved += value;
            remove => Instance._OnConfigFileSaved -= value;
        }

        public static event Action OnConfigFileUpdated
        {
            add => Instance._OnConfigFileUpdated += value;
            remove => Instance._OnConfigFileUpdated -= value;
        }

        protected abstract string _ConfigFileName { get; }

        public static string ConfigFilePath => Instance._ConfigFilePath;
        public static string ConfigFileName => Instance._ConfigFileName;

        public static TData LoadConfig() => Instance.LoadConfig_Internal();

        public static void SaveConfig(TData newConfig)
        {
            lock (SaveConfigSyncRoot)
            {
                Instance.WriteConfigToFile(newConfig);
                Instance._OnConfigFileSaved?.Invoke();
            }
        }

        public static bool ErrorLoadingConfiguration() => Instance.ErrorLoadingConfig;

        #endregion

        #region Internal

        private event Action _OnConfigFileSaved;
        private event Action _OnConfigFileUpdated;
        protected virtual string _ConfigFilePath => Path.Combine(ConfigFileUtils.ConfigFileDirectory, ConfigFileName);

        public bool ErrorLoadingConfig { get; private set; } = false;

        private TData LoadConfig_Internal()
        {
            if (!DoesConfigFileExist())
            {
                CreateDefaultConfigFile();
            }

            string data = File.ReadAllText(_ConfigFilePath);
            TData config = DeserialiseRawText(data);

            if (ErrorLoadingConfig)
            {
                // If we have errored then use a default config but don't overwrite the file
                config = new TData();
            }
            else if (config == null)
            {
                // If the config is null after deserialisation then create a default config
                CreateDefaultConfigFile();

                data = File.ReadAllText(_ConfigFilePath);
                config = DeserialiseRawText(data);
            }

            _OnConfigFileUpdated?.Invoke();

            return config;
        }

        protected TData DeserialiseRawText(string rawText)
        {
            ErrorLoadingConfig = false;
            TData config = JsonConvert.DeserializeObject<TData>(rawText, new JsonSerializerSettings()
            {
                Error = HandleDeserialisationError
            });
            return config;
        }

        private void HandleDeserialisationError(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs errorArgs)
        {
            ErrorLoadingConfig = true;
            errorArgs.ErrorContext.Handled = true;
            TouchFreeLog.WriteLine($"Unable to load settings from config {typeof(TData)}");
        }

        public static bool DoesConfigFileExist()
        {
            if (!Directory.Exists(ConfigFileUtils.ConfigFileDirectory))
            {
                return false;
            }

            if (!File.Exists(Instance._ConfigFilePath))
            {
                return false;
            }

            return true;
        }

        private void CreateDefaultConfigFile()
        {
            Directory.CreateDirectory(ConfigFileUtils.ConfigFileDirectory);
            RequestConfigFilePermissions();
            WriteConfigToFile(new TData());
            TouchFreeLog.WriteLine($"No {ConfigFileName} file found in {ConfigFileUtils.ConfigFileDirectory}. One has been generated for you with default values.");
        }

        private void WriteConfigToFile(TData data)
        {
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            // Replace the coordinate bits of json with lowercase versions for compatibility with the Unity Settings UI
            json = json.Replace("\"X\"", "\"x\"").Replace("\"Y\"", "\"y\"").Replace("\"Z\"", "\"z\"");
            File.WriteAllText(_ConfigFilePath, json);
        }

        void RequestConfigFilePermissions()
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    SecurityIdentifier securityIdentifier = new SecurityIdentifier
                        (WellKnownSidType.AuthenticatedUserSid, null);

                    AccessRule rule = new FileSystemAccessRule(
                        securityIdentifier,
                        FileSystemRights.Write |
                        FileSystemRights.ReadAndExecute |
                        FileSystemRights.Modify,
                        InheritanceFlags.ContainerInherit |
                        InheritanceFlags.ObjectInherit,
                        PropagationFlags.InheritOnly,
                        AccessControlType.Allow);

                    DirectoryInfo dirInfo = new DirectoryInfo(ConfigFileUtils.ConfigFileDirectory);

                    // Create the directory and request permissions to it for all users
                    DirectorySecurity directorySecurity = dirInfo.GetAccessControl();
                    directorySecurity.ModifyAccessRule(AccessControlModification.Add, rule, out _);
                    dirInfo.SetAccessControl(directorySecurity);
                }
            }
            catch
            {
                TouchFreeLog.WriteLine("Did not have permissions to set file access rules");
            }
        }

        #endregion
    }
}
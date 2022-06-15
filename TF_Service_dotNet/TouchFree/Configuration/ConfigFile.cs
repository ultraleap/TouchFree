using Newtonsoft.Json;
using System;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Runtime.InteropServices;

namespace Ultraleap.TouchFree.Library.Configuration
{
    public abstract class ConfigFile<TData, UThisClass>
    where TData : class, new()
    where UThisClass : ConfigFile<TData, UThisClass>, new()
    {
        #region Singleton

        protected static UThisClass _instance;
        protected static UThisClass Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new UThisClass();
                }
                return _instance;
            }
        }

        #endregion

        #region Public

        public static event Action OnConfigFileUpdated
        {
            add { Instance._OnConfigFileUpdated += value; }
            remove { Instance._OnConfigFileUpdated -= value; }
        }

        protected abstract string _ConfigFileName { get; }

        public static string ConfigFilePath => Instance._ConfigFilePath;
        public static string ConfigFileName => Instance._ConfigFileName;

        public static TData LoadConfig()
        {
            return Instance.LoadConfig_Internal();
        }

        public static void SaveConfig(TData newConfig)
        {
            Instance.SaveConfig_Internal(newConfig);
        }

        public static TData GetDefaultValues()
        {
            return new TData();
        }

        public static bool ErrorLoadingConfiguration()
        {
            return Instance.ErrorLoadingConfig;
        }

        #endregion

        #region Internal

        private static IConfigFileLocator configFileLocator;
        public static IConfigFileLocator ConfigFileLocator
        {
            get => configFileLocator;
            set
            {
                configFileLocator = value ?? throw new ArgumentNullException("ConfigFileLocator");
            }
        }
        private static ITouchFreeLogger logger;
        public static ITouchFreeLogger Logger
        {
            get => logger;
            set {
                logger = value ?? throw new ArgumentNullException("Logger");
            }
        }

        private event Action _OnConfigFileUpdated;
        protected virtual string _ConfigFilePath => Path.Combine(ConfigFileLocator.ConfigFileDirectory, ConfigFileName);

        public bool ErrorLoadingConfig { get; private set; } = false;

        protected TData LoadConfig_Internal()
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

        protected void SaveConfig_Internal(TData config)
        {
            File.WriteAllText(_ConfigFilePath, JsonConvert.SerializeObject(config, Formatting.Indented));
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
            Logger.WriteLine($"Unable to load settings from config {typeof(TData)}");
        }

        private bool DoesConfigFileExist()
        {
            if (!Directory.Exists(ConfigFileLocator.ConfigFileDirectory))
            {
                return false;
            }

            if (!File.Exists(_ConfigFilePath))
            {
                return false;
            }

            return true;
        }

        private void CreateDefaultConfigFile()
        {
            Directory.CreateDirectory(ConfigFileLocator.ConfigFileDirectory);
            RequestConfigFilePermissions();
            File.WriteAllText(_ConfigFilePath, JsonConvert.SerializeObject(new TData()));
            Logger.WriteLine($"No {ConfigFileName} file found in {ConfigFileLocator.ConfigFileDirectory}. One has been generated for you with default values.");
        }

        void RequestConfigFilePermissions()
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    SecurityIdentifier securityIdentifier = new SecurityIdentifier
                        (WellKnownSidType.BuiltinUsersSid, null);

                    AccessRule rule = new FileSystemAccessRule(
                        securityIdentifier,
                        FileSystemRights.Write |
                        FileSystemRights.ReadAndExecute |
                        FileSystemRights.Modify,
                        InheritanceFlags.ContainerInherit |
                        InheritanceFlags.ObjectInherit,
                        PropagationFlags.InheritOnly,
                        AccessControlType.Allow);

                    DirectoryInfo dirInfo = new DirectoryInfo(ConfigFileLocator.ConfigFileDirectory);

                    // Create the directory and request permissions to it for all users
                    DirectorySecurity directorySecurity = FileSystemAclExtensions.GetAccessControl(dirInfo);
                    directorySecurity.ModifyAccessRule(AccessControlModification.Add, rule, out _);
                    FileSystemAclExtensions.SetAccessControl(dirInfo, directorySecurity);
                }
            }
            catch
            {
                Logger.WriteLine("Did not have permissios to set file access rules");
            }
        }

        #endregion
    }
}
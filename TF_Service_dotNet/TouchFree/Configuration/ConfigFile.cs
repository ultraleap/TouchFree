using Newtonsoft.Json;
using System;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

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

        public static TData GetDefaultValues()
        {
            return new TData();
        }

        #endregion

        #region Internal

        private event Action _OnConfigFileUpdated;
        protected virtual string _ConfigFilePath => Path.Combine(ConfigFileUtils.ConfigFileDirectory, ConfigFileName);

        protected TData LoadConfig_Internal()
        {
            if (!DoesConfigFileExist())
            {
                CreateDefaultConfigFile();
            }

            string data = File.ReadAllText(_ConfigFilePath);
            TData config = JsonConvert.DeserializeObject<TData>(data);
            _OnConfigFileUpdated?.Invoke();

            return config;
        }

        private bool DoesConfigFileExist()
        {
            if (!Directory.Exists(ConfigFileUtils.ConfigFileDirectory))
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
            Directory.CreateDirectory(ConfigFileUtils.ConfigFileDirectory);
            RequestConfigFilePermissions();
            File.WriteAllText(_ConfigFilePath, JsonConvert.SerializeObject(new TData()));
            Console.WriteLine($"No {ConfigFileName} file found in {ConfigFileUtils.ConfigFileDirectory}. One has been generated for you with default values.");
        }

        void RequestConfigFilePermissions()
        {
            try
            {
                AccessRule rule;
                SecurityIdentifier securityIdentifier = new SecurityIdentifier
                    (WellKnownSidType.BuiltinUsersSid, null);

                rule = new FileSystemAccessRule(
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
                DirectorySecurity directorySecurity = FileSystemAclExtensions.GetAccessControl(dirInfo);
                directorySecurity.ModifyAccessRule(AccessControlModification.Add, rule, out _);
                FileSystemAclExtensions.SetAccessControl(dirInfo, directorySecurity);
            }
            catch
            {
                Console.WriteLine("Did not have permissios to set file access rules");
            }
        }

        #endregion
    }
}
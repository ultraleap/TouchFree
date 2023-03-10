using System;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using UnityEngine;

namespace Ultraleap.TouchFree
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

        /// <summary>
        /// Write the current values stored in Config to a file. File path will be ConfigFilePath which is a combination of the ConfigFileName and ConfigFileDirectory.
        /// </summary>
        public static void SaveConfig(TData _config)
        {
            Instance.SaveConfig_Internal(_config);
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
            TData config = JsonUtility.FromJson<TData>(data);
            _OnConfigFileUpdated?.Invoke();

            return config;
        }

        protected void SaveConfig_Internal(TData config)
        {
            File.WriteAllText(_ConfigFilePath, JsonUtility.ToJson(config, true));
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
            File.WriteAllText(_ConfigFilePath, JsonUtility.ToJson(new TData(), true));
            Debug.LogWarning($"No {ConfigFileName} file found in {ConfigFileUtils.ConfigFileDirectory}. One has been generated for you with default values.");
        }

        void RequestConfigFilePermissions()
        {
            // Create the directory and request permissions to it for all users
            DirectorySecurity directorySecurity;
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

            directorySecurity = Directory.GetAccessControl(ConfigFileUtils.ConfigFileDirectory);
            directorySecurity.ModifyAccessRule(AccessControlModification.Add, rule, out _);
            Directory.SetAccessControl(ConfigFileUtils.ConfigFileDirectory, directorySecurity);
        }

        #endregion
    }
}
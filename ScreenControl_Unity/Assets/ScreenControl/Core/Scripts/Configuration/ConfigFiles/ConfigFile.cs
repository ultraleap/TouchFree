using System;
using System.IO;
using UnityEngine;

namespace Ultraleap.ScreenControl.Core
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

        public static readonly string ConfigFileDirectory = Application.persistentDataPath;
        public static readonly string CustomDefaultConfigFileDirectory = Path.Combine(Application.streamingAssetsPath + "/SavedSetups/");

        public abstract string ConfigFileName { get; }

        public static string ConfigFilePath => Instance._ConfigFilePath;
        public static string ConfigFileNameS => Instance.ConfigFileName;

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

        // Called via reflection in ConfigFileUtils.
        public static void LoadCustomDefaults(string _customFolderName)
        {
            Instance.LoadConfigFromCustomDefault(_customFolderName);
        }

        #endregion

        #region Internal

        private event Action _OnConfigFileUpdated;
        protected virtual string _ConfigFilePath => Path.Combine(ConfigFileDirectory, ConfigFileName);

        protected TData LoadConfig_Internal()
        {
            if (!DoesConfigFileExist())
            {
                CreateDefaultConfigFile();

                if (DoesCustomDefaultConfigFileExist()) // check if there are ANY custom default files. This will speed up rollout to many machines
                {
                    //find the first one and load it
                    LoadConfigFromCustomDefault(GetFirstCustomDefault());
                }
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
            if (!Directory.Exists(ConfigFileDirectory))
            {
                return false;
            }

            if (!File.Exists(_ConfigFilePath))
            {
                return false;
            }

            return true;
        }

        private bool DoesCustomDefaultConfigFileExist(string _customDefaultName = "")
        {
            if (!Directory.Exists(CustomDefaultConfigFileDirectory))
            {
                // there is no root
                return false;
            }
            else if (_customDefaultName == "" && Directory.GetDirectories(CustomDefaultConfigFileDirectory).Length == 0)
            {
                // there are no custom folders available
                return false;
            }
            else if (_customDefaultName != "" && !Directory.Exists(Path.Combine(CustomDefaultConfigFileDirectory, _customDefaultName)))
            {
                // the specified folder does not exist
                return false;
            }

            if (_customDefaultName != "" && !File.Exists(Path.Combine(CustomDefaultConfigFileDirectory, _customDefaultName, ConfigFileName)))
            {
                return false;
            }

            return true;
        }

        private void LoadConfigFromCustomDefault(string _customDefaultName)
        {
            if (DoesCustomDefaultConfigFileExist(_customDefaultName) && DoesConfigFileExist())
            {
                File.WriteAllText(_ConfigFilePath, File.ReadAllText(Path.Combine(CustomDefaultConfigFileDirectory, _customDefaultName, ConfigFileName)));
                LoadConfig();
            }
        }

        /// <summary>
        /// Returns the first custom default directory found
        /// </summary>
        /// <returns></returns>
        private string GetFirstCustomDefault()
        {
            if (DoesCustomDefaultConfigFileExist())
            {
                return Directory.GetDirectories(CustomDefaultConfigFileDirectory)[0].Replace(CustomDefaultConfigFileDirectory, "");
            }

            return null;
        }

        private void CreateDefaultConfigFile()
        {
            Directory.CreateDirectory(ConfigFileDirectory);
            File.WriteAllText(_ConfigFilePath, JsonUtility.ToJson(new TData(), true));
            Debug.LogWarning($"No {ConfigFileName} file found in {ConfigFileDirectory}. One has been generated for you with default values.");
        }

        #endregion
    }
}
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

        public static TData Config
        {
            get
            {
                if (Instance._config == null)
                {
                    Instance.LoadConfig_Internal();
                }
                return Instance._config;
            }
        }

        public static event Action OnConfigUpdated
        {
            add { Instance._OnConfigUpdated += value; }
            remove { Instance._OnConfigUpdated -= value; }
        }

        public static readonly string ConfigFileDirectory = Application.persistentDataPath;
        public static readonly string CustomDefaultConfigFileDirectory = Path.Combine(Application.streamingAssetsPath + "/SavedSetups/");

        public abstract string ConfigFileName { get; }

        public static string ConfigFilePath => Instance._ConfigFilePath;
        public static string ConfigFileNameS => Instance.ConfigFileName;

        public static void LoadConfig()
        {
            Instance.LoadConfig_Internal();
        }

        /// <summary>
        /// Write the current values stored in Config to a file. File path will be ConfigFilePath which is a combination of the ConfigFileName and ConfigFileDirectory.
        /// </summary>
        public static void SaveConfig()
        {
            Instance.SaveConfig_Internal(Config);
        }

        /// <summary>
        /// Call after changing the current Config (pass in null) or pass in a new config object to replace the current config.
        /// Applies parameter limits and calls OnConfigUpdated event.
        /// </summary>
        /// <param name="config">The config object to update over the top of the old one. Passing null is like passing the current config in ConfigFile.Config to this method.</param>
        public static void UpdateConfig(TData config = null)
        {
            Instance.UpdateConfig_Internal(config);
        }

        public static void SetAllValuesToDefault()
        {
            Instance.UpdateConfig_Internal(GetDefaultValues());
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

        private event Action _OnConfigUpdated;
        protected virtual string _ConfigFilePath => Path.Combine(ConfigFileDirectory, ConfigFileName);
        protected TData _config;

        protected void LoadConfig_Internal()
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
            ApplyParameterLimits(ref config);
            _config = config;

            _OnConfigUpdated?.Invoke();
        }

        protected void SaveConfig_Internal(TData config)
        {
            ApplyParameterLimits(ref config);
            File.WriteAllText(_ConfigFilePath, JsonUtility.ToJson(config, true));
        }

        protected virtual void UpdateConfig_Internal(TData config)
        {
            if (config == null)
            {
                config = Config;
            }
            ApplyParameterLimits(ref config);
            _config = config;
            _OnConfigUpdated?.Invoke();
        }

        /// <summary>
        /// Limits parameters to particular hard-coded ranges.
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        protected virtual void ApplyParameterLimits(ref TData config)
        {

        }

        protected void ClampValue(ref float value, float min, float max) => value = Mathf.Clamp(value, min, max);
        protected void ClampValue(ref int value, int min, int max) => value = Mathf.Clamp(value, min, max);
        protected void ClampValue(ref Vector3 value, Vector3 min, Vector3 max)
        {
            ClampValue(ref value.x, min.x, max.x);
            ClampValue(ref value.y, min.y, max.y);
            ClampValue(ref value.z, min.z, max.z);
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
using UnityEngine;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Ultraleap.ScreenControl.Core
{
    public static class ConfigFileUtils
    {
        public static void SetCustomDefaultsOnAllConfigFiles()
        {
            InvokeStaticMethodOnBaseTypeOfAllConfigFileImplementors("SetCustomDefaults");
        }

        public static void CheckForInvalidCustomDefault(string _customName, out bool _corrupt, out bool _missing)
        {
            _corrupt = false;
            _missing = false;

            bool foundMissing = false;

            // We have to be specific as to which config files we read AND know the contents of them, so we must explicitly check them.
            string path = Path.Combine(InteractionConfigFile.CustomDefaultConfigFileDirectory, _customName, InteractionConfigFile.ConfigFileNameS);

            if (CheckCustomDefaultTypeInvalid<InteractionConfig>(path, out _corrupt, out _missing))
            {
                if (_corrupt)   // there was a corrupt file, ignore the rest.
                {
                    return;
                }

                if (_missing)
                {
                    foundMissing = true;
                }
            }

            path = Path.Combine(PhysicalConfigFile.CustomDefaultConfigFileDirectory, _customName, PhysicalConfigFile.ConfigFileNameS);

            if (CheckCustomDefaultTypeInvalid<PhysicalConfig>(path, out _corrupt, out _missing))
            {
                if (_corrupt)   // there was a corrupt file, ignore the rest.
                {
                    return;
                }

                if (_missing)
                {
                    foundMissing = true;
                }
            }

            if (foundMissing)
            {
                _missing = true;
            }
        }

        static bool CheckCustomDefaultTypeInvalid<T>(string _path, out bool _corrupt, out bool _missing)
        {
            _corrupt = false;
            _missing = false;

            try
            {
                var fromJson = JsonUtility.FromJson<T>(File.ReadAllText(_path));
            }
            catch
            {
                _corrupt = true;
                return true;
            }

            // no error so continue
            var fields = typeof(T).GetFields(); // get all the fields of the target config file
            string[] customPresetText = File.ReadAllLines(_path); // find all the lines of the saved file

            for (int lineIndex = 0; lineIndex < customPresetText.Length; lineIndex++)
            {
                string[] split = customPresetText[lineIndex].Split('"'); // look for the keys of the json value

                if (split.Length > 2)
                {
                    customPresetText[lineIndex] = split[1]; // this should be a key
                }
            }

            foreach (var field in fields)
            {
                if (!customPresetText.Contains(field.Name))
                {
                    // a field is missing, it will be populated for the user, but will be the default value. Warn them.
                    _missing = true;
                    return true;
                }
            }

            return false;
        }

        public static void LoadCustomDefaultsOnAllConfigFiles(string _customName)
        {
            InvokeStaticMethodOnBaseTypeOfAllConfigFileImplementors("LoadCustomDefaults", new object[] { _customName });
        }

        public static void SaveCustomDefaults(string _customName)
        {
            DirectoryCopy(Application.persistentDataPath, Path.Combine(PhysicalConfigFile.CustomDefaultConfigFileDirectory, _customName));
        }

        public static void SaveAllConfigFiles()
        {
            InvokeStaticMethodOnBaseTypeOfAllConfigFileImplementors("SaveConfig");
        }

        private static void InvokeStaticMethodOnBaseTypeOfAllConfigFileImplementors(string methodName, object[] _params = default)
        {
            // Use reflection to get all implementors of ConfigFile<,> and call their LoadCustomDefaults static method.
            var configFileTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => !t.IsAbstract
                    && !t.IsInterface
                    && t.BaseType != null
                    && t.BaseType.IsGenericType
                    && t.BaseType.GetGenericTypeDefinition() == typeof(ConfigFile<,>))
                .ToList();
            foreach (var type in configFileTypes)
            {
                var method = type.BaseType.GetMethod(methodName); // Use the BaseType ConfigFile<,> as this is where the method is implemented.
                method.Invoke(null, _params);
            }
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs = true, bool deleteExisting = true)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);

                if (File.Exists(temppath))
                {
                    File.Delete(temppath);
                }

                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);

                    if (Directory.Exists(temppath))
                    {
                        Directory.Delete(temppath);
                    }

                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
    }
}
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
    }
}
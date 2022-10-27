using System;
using System.Reflection;

namespace Ultraleap.TouchFree.Library.Configuration
{
    public static class VersionManager
    {
        public static string Version
        {
            get
            {
                if (string.IsNullOrEmpty(_version))
                {
                    _version = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? string.Empty;
                }

                return _version;
            }
        }

        private static string _version = string.Empty;
        public static readonly Version ApiVersion = new Version("1.3.0");
        public const string API_HEADER_NAME = "TfApiVersion";
    }
}

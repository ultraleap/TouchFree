using System;
using System.Diagnostics;
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
                    Assembly assembly = Assembly.GetEntryAssembly();
                    FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
                    _version = fileVersionInfo.ProductVersion;
                }

                return _version;
            }
        }

        private static string _version = string.Empty;
        public static readonly Version ApiVersion = new Version("1.3.0");
        public const string API_HEADER_NAME = "TfApiVersion";
    }
}

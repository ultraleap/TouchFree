using System;
using System.Reflection;

namespace Ultraleap.TouchFree.Library.Configuration
{
    public static class VersionManager
    {
        public static readonly Version Version = Assembly.GetExecutingAssembly().GetName().Version;
        public static readonly Version ApiVersion = new Version("1.3.0");
        public const string API_HEADER_NAME = "TfApiVersion";
    }
}

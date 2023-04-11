using System;
using System.Reflection;

namespace Ultraleap.TouchFree.Library.Configuration;

public static class VersionManager
{
    public static string Version =>
        string.IsNullOrEmpty(_version)
            ? _version = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? string.Empty
            : _version;

    private static string _version = string.Empty;
    public static readonly Version ApiVersion = new("1.4.0");
    public const string API_HEADER_NAME = "TfApiVersion";
}
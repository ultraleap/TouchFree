using Microsoft.Win32;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Ultraleap.TouchFree.Library.Configuration;

public static class ConfigFileUtils
{
    public static string ConfigFileDirectory => _configFileDirectory ??= GetConfigFileDirectory();

    private static string _configFileDirectory = null;
    private static readonly string _defaultConfigDirectory = RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
        ? Path.GetFullPath("/storage/sd/ultraleap/touchfree/configuration/")
        : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Ultraleap\\TouchFree\\Configuration\\");

    public static void CheckForConfigDirectoryChange() => _configFileDirectory = GetConfigFileDirectory();

    private static string GetConfigFileDirectory()
    {
        // Computer\HKEY_LOCAL_MACHINE\SOFTWARE\Ultraleap\TouchFree\Service\Settings
        // Check registry for override to default directory
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            RegistryKey regKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Ultraleap\TouchFree\Service\Settings");

            if (regKey != null)
            {
                var pathObj = regKey.GetValue("ConfigFileDirectory");

                if (pathObj != null)
                {
                    string path = pathObj.ToString();

                    if (Directory.Exists(path))
                    {
                        regKey.Close();
                        return path;
                    }
                }

                regKey.Close();
            }
        }

        // else
        return _defaultConfigDirectory;
    }
}
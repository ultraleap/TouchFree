using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Ultraleap.TouchFree.Library.Configuration;

namespace Ultraleap.TouchFree.Service_Android
{
    public class ConfigFileLocator : IConfigFileLocator
    {
        public ConfigFileLocator(string fileLocation)
        {
            ConfigFileDirectory = fileLocation;
        }

        public string ConfigFileDirectory { get; private set; }

        public void ReloadConfigFileDirectoryFromRegistry()
        {
        }
    }
}
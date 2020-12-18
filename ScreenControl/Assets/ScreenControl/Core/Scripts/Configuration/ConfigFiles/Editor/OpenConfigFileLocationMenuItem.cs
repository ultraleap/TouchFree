using System.Diagnostics;
using UnityEditor;

namespace Ultraleap.ScreenControl.Core
{
    public static class OpenConfigFileLocationMenuItem
    {
        [MenuItem("Ultraleap/Reach/Open Config File Location")]
        static void _OpenConfigFileLocation()
        {
            Process.Start(PhysicalConfigFile.ConfigFileDirectory);
        }
    }
}
using UnityEngine;

namespace Ultraleap.ScreenControl.Core
{
    public class ScreenControlMain
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void ScreenControl_Main()
        {
            ConfigManager.GlobalSettings.ScreenWidth = Display.main.systemWidth;
            ConfigManager.GlobalSettings.ScreenHeight = Display.main.systemHeight;
            Screen.SetResolution(ConfigManager.GlobalSettings.ScreenWidth, ConfigManager.GlobalSettings.ScreenHeight, true);
        }
    }
}

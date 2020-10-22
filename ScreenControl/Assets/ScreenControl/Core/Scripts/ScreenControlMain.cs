using UnityEngine;

namespace Ultraleap.ScreenControl.Core
{
    public class ScreenControlMain
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void ScreenControl_Main()
        {
            GlobalSettings.ScreenWidth = Display.main.systemWidth;
            GlobalSettings.ScreenHeight = Display.main.systemHeight;
            Screen.SetResolution(GlobalSettings.ScreenWidth, GlobalSettings.ScreenHeight, true);
        }
    }
}

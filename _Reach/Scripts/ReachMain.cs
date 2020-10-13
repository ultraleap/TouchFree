using UnityEngine;

public class ReachMain
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Reach_Main()
    {
        GlobalSettings.ScreenWidth = Display.main.systemWidth;
        GlobalSettings.ScreenHeight = Display.main.systemHeight;
        Screen.SetResolution(GlobalSettings.ScreenWidth, GlobalSettings.ScreenHeight, true);
    }
}

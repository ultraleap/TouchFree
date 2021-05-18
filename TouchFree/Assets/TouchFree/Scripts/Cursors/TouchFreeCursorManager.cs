using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ultraleap.TouchFree;

public class TouchFreeCursorManager : CursorManager
{
    private void OnEnable()
    {
        ConfigManager.Config.OnConfigUpdated += ConfigUpdated;
    }

    private void OnDisable()
    {
        ConfigManager.Config.OnConfigUpdated -= ConfigUpdated;
    }

    void ConfigUpdated()
    {
        SetCursorVisibility(ConfigManager.Config.cursorEnabled);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ultraleap.TouchFree;

public class TouchFreeCursorManager : CursorManager
{
    protected override void OnEnable()
    {
        ConfigManager.Config.OnConfigUpdated += ConfigUpdated;
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        ConfigManager.Config.OnConfigUpdated -= ConfigUpdated;
        base.OnDisable();
    }

    void ConfigUpdated()
    {
        SetCursorVisibility(ConfigManager.Config.cursorEnabled);
    }
}
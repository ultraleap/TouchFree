﻿using System;

namespace Ultraleap.TouchFree.Library.Configuration;

public class TouchFreeConfigFile : ConfigFile<TouchFreeConfig, TouchFreeConfigFile>
{
    protected override string _ConfigFileName => "TouchFreeConfig.json";
}

public enum CtiHideTrigger
{
    PRESENCE,
    INTERACTION
}

public enum CursorColorPreset
{
    LIGHT,
    DARK,
    CUSTOM,
    LIGHT_OUTLINE,
    DARK_OUTLINE,
}

[Serializable]
public record TouchFreeConfig
{
    // Cursor Settings
    public bool cursorEnabled = true;
    public float cursorSizeCm = 0.25f;
    public float cursorRingThickness = 0.15f;
    public CursorColorPreset activeCursorPreset = CursorColorPreset.LIGHT;
    public TFColour primaryCustomColor = new() { r = 1.0, g = 1.0, b = 1.0, a = 1.0 };
    public TFColour secondaryCustomColor = new() { r = 0.0, g = 0.0, b = 0.0, a = 1.0 };
    public TFColour tertiaryCustomColor = new() { r = 0.0, g = 0.0, b = 0.0, a = 1.0 };

    // CTI Settings
    public bool ctiEnabled = false;
    public string ctiFilePath = "";
    public CtiHideTrigger ctiHideTrigger = CtiHideTrigger.INTERACTION;
    public float ctiShowAfterTimer = 10.0f;

    // Misc Settings
    public bool StartupUIShown = false;
}

[Serializable]
public readonly record struct TFColour(double r, double g, double b, double a);
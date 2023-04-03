using System;
using System.Numerics;

namespace Ultraleap.TouchFree.Library.Configuration;

public class PhysicalConfigFile : ConfigFile<PhysicalConfig, PhysicalConfigFile>
{
    protected override string _ConfigFileName => "PhysicalConfig.json";
}

[Serializable]
public record PhysicalConfig
{
    public float ScreenHeightM = 0.33f;
    public Vector3 LeapPositionRelativeToScreenBottomM = new(0f, -0.12f, -0.25f);
    public Vector3 LeapRotationD = Vector3.Zero;
    public float ScreenRotationD = 0f;

    public int ScreenWidthPX = 0;
    public int ScreenHeightPX = 0;

    public PhysicalConfig() { /* Defaults set in field initializers */ }

    public PhysicalConfig(PhysicalConfigInternal cfg)
    {
        ScreenHeightM = cfg.ScreenHeightMm / 1000f;
        LeapPositionRelativeToScreenBottomM = cfg.LeapPositionRelativeToScreenBottomMm / 1000f;

        LeapRotationD = cfg.LeapRotationD;
        ScreenRotationD = cfg.ScreenRotationD;

        ScreenWidthPX = cfg.ScreenWidthPX;
        ScreenHeightPX = cfg.ScreenHeightPX;
    }
}
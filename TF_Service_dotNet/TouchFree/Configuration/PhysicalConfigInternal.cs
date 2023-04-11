using System;
using System.Numerics;

namespace Ultraleap.TouchFree.Library.Configuration;

[Serializable]
public record PhysicalConfigInternal
{
    public float ScreenHeightMm = 330f;
    public Vector3 LeapPositionRelativeToScreenBottomMm = new Vector3(0f, -120f, -250f);
    public Vector3 LeapRotationD = Vector3.Zero;
    public float ScreenRotationD = 0f;

    public int ScreenWidthPX = 0;
    public int ScreenHeightPX = 0;

    public PhysicalConfig ForApi() =>
        new()
        {
            LeapPositionRelativeToScreenBottomM = LeapPositionRelativeToScreenBottomMm / 1000f,
            LeapRotationD = LeapRotationD,
            ScreenHeightM = ScreenHeightMm / 1000f,
            ScreenHeightPX = ScreenHeightPX,
            ScreenRotationD = ScreenRotationD,
            ScreenWidthPX = ScreenWidthPX
        };

    public PhysicalConfigInternal() { /* Defaults set in field initializers */ }

    public PhysicalConfigInternal(PhysicalConfig cfg)
    {
        ScreenHeightMm = cfg.ScreenHeightM * 1000f;
        LeapPositionRelativeToScreenBottomMm = cfg.LeapPositionRelativeToScreenBottomM * 1000f;

        LeapRotationD = cfg.LeapRotationD;
        ScreenRotationD = cfg.ScreenRotationD;

        ScreenWidthPX = cfg.ScreenWidthPX;
        ScreenHeightPX = cfg.ScreenHeightPX;
    }
}
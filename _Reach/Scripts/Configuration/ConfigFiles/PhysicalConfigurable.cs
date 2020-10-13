using UnityEngine;

public class PhysicalSetup
{
    public float ScreenHeightM = 0.33f;
    public Vector3 LeapPositionRelativeToScreenBottomM = new Vector3(0f, -0.12f, -0.25f);
    public Vector3 LeapRotationD = Vector3.zero;
    public float ScreenRotationD = 0f;
}

public class PhysicalConfigurable : ConfigFile<PhysicalSetup, PhysicalConfigurable>
{
    public override string ConfigFileName => "TouchlessConfig.json";

    public const float ScreenHeight_Min = 0.05f;
    public const float ScreenHeight_Max = 1f;

    public const float TrackingOriginX_Min = -0.25f;
    public const float TrackingOriginX_Max = 0.25f;

    public const float TrackingOriginY_Min = -1f;
    public const float TrackingOriginY_Max = 1f;

    public const float TrackingOriginZ_Min = -0.5f;
    public const float TrackingOriginZ_Max = 0.5f;

    public const float ScreenTilt_Min = -90f;
    public const float ScreenTilt_Max = 90f;

    public const float VirtualScreenDist_Min = 0.01f;
    public const float VirtualScreenDist_Max = 0.5f;

    public const float TrackingRoation_Min = -90f;
    public const float TrackingRoation_Max = 90f;

    /// <summary>
    /// Limits parameters to particular hard-coded ranges.
    /// </summary>
    /// <param name="setup"></param>
    /// <returns></returns>
    protected override void ApplyParameterLimits(ref PhysicalSetup config)
    {

    }

    /// <summary>
    /// Override the base so we can use the config upon updating
    /// </summary>
    /// <param name="config"></param>
    protected override void UpdateConfig_Internal(PhysicalSetup config)
    {
        base.UpdateConfig_Internal(config);
        CreateVirtualScreen(config);
    }

    public static void CreateVirtualScreen(PhysicalSetup _config)
    {
        GlobalSettings.virtualScreen = new VirtualScreen(
            GlobalSettings.ScreenWidth,
            GlobalSettings.ScreenHeight,
            _config.ScreenHeightM,
            _config.ScreenRotationD);
    }
}
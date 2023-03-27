using System;

namespace Ultraleap.TouchFree.Library.Configuration;

public class TrackingConfigFile : ConfigFile<TrackingConfig, TrackingConfigFile>
{
    protected override string _ConfigFileName => "TrackingConfig.json";
}

[Serializable]
public record TrackingConfig(MaskingData Mask,
    bool AllowImages,
    bool CameraReversed,
    bool AnalyticsEnabled)
{
    public TrackingConfig()
        : this(new MaskingData(), true, false, true)
    { }
}

[Serializable]
public readonly record struct MaskingData(double Lower, double Upper, double Right, double Left);
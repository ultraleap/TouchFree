using System;
using System.Numerics;

namespace Ultraleap.TouchFree.Library.Configuration
{
    public class TrackingConfigFile : ConfigFile<TrackingConfig, TrackingConfigFile>
    {
        protected override string _ConfigFileName => "TrackingConfig.json";
    }

    [Serializable]
    public class TrackingConfig
    {
        public MaskingData Mask = new MaskingData();
        public bool AllowImages = true;
        public bool CameraReversed = false;
        public bool AnalyticsEnabled = true;

        public TrackingConfig()
        {
            Mask = new MaskingData();
            AllowImages = true;
            CameraReversed = false;
            AnalyticsEnabled = true;
        }

        public TrackingConfig(TrackingConfigInternal _internal)
        {
            Mask = _internal.Mask;
            AllowImages = _internal.AllowImages;
            CameraReversed = _internal.CameraReversed;
            AnalyticsEnabled = _internal.AnalyticsEnabled;
        }
    }

    [Serializable]
    public class MaskingData
    {
        public float Lower = 0;
        public float Upper = 0;
        public float Right = 0;
        public float Left = 0;
    }
}
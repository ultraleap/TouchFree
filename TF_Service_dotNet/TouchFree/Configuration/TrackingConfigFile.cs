using System;
using Ultraleap.TouchFree.Library.Connections;

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
    }

    [Serializable]
    public class MaskingData
    {
        public double Lower = 0;
        public double Upper = 0;
        public double Right = 0;
        public double Left = 0;

        public static explicit operator MaskingData(ImageMaskData other) => new()
        {
            Left = other.left,
            Lower = other.lower,
            Right = other.right,
            Upper = other.upper
        };
    }
}
using System;

namespace Ultraleap.TouchFree.Library.Configuration
{
    [Serializable]
    public class TrackingConfigInternal
    {
        public MaskingData Mask = new MaskingData();
        public bool AllowImages = true;
        public bool CameraReversed = false;
        public bool AnalyticsEnabled = true;

        public TrackingConfig ForApi()
        {
            return new TrackingConfig()
            {
                AllowImages = AllowImages,
                CameraReversed = CameraReversed,
                AnalyticsEnabled = AnalyticsEnabled,
                Mask = Mask
            };
        }

        public TrackingConfigInternal()
        {
            Mask = new MaskingData();
            AllowImages = true;
            CameraReversed = false;
            AnalyticsEnabled = true;
        }

        public TrackingConfigInternal(TrackingConfig fromFile)
        {
            Mask = fromFile.Mask;
            AllowImages = fromFile.AllowImages;
            CameraReversed = fromFile.CameraReversed;
            AnalyticsEnabled = fromFile.AnalyticsEnabled;
        }
    }
}
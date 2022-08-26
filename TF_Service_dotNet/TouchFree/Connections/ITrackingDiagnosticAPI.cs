using System;
using Ultraleap.TouchFree.Library.Configuration;

namespace Ultraleap.TouchFree.Library.Connections
{
    public interface ITrackingDiagnosticApi
    {
        public event Action<ImageMaskData?, string> OnMaskingResponse;
        public event Action<bool?, string> OnAnalyticsResponse;
        public event Action<bool?, string> OnAllowImagesResponse;
        public event Action<bool?, string> OnCameraOrientationResponse;

        public event Action OnTrackingApiVersionResponse;
        public event Action OnTrackingServerInfoResponse;
        public event Action OnTrackingDeviceInfoResponse;

        void GetAnalyticsMode();
        void SetAnalyticsMode(bool enabled);

        void GetAllowImages();
        void SetAllowImages(bool enabled);

        void GetImageMask();
        void SetMasking(float _left, float _right, float _top, float _bottom);

        void GetCameraOrientation();
        void SetCameraOrientation(bool reverseOrientation);

        void GetDeviceInfo();
        void GetDevices();
        void GetServerInfo();
        void GetVersion();

        void HandleDiagnosticAPIVersion(string _version);
        void Request(object payload);

        void TriggerUpdatingTrackingConfiguration();
    }
}
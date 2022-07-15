using System;

namespace Ultraleap.TouchFree.Library.Connections
{
    public interface ITrackingDiagnosticApi
    {
        public event Action<ImageMaskData> OnMaskingResponse;
        public event Action<bool> OnAnalyticsResponse;
        public event Action<bool> OnAllowImagesResponse;
        public event Action<bool> OnCameraOrientationResponse;

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
    }
}
using System;

namespace Ultraleap.TouchFree.Library.Connections
{
    public interface ITrackingDiagnosticApi
    {
        public event Action<Result<ImageMaskData>> OnMaskingResponse;
        public event Action<Result<bool>> OnAnalyticsResponse;
        public event Action<Result<bool>> OnAllowImagesResponse;
        public event Action<Result<bool>> OnCameraOrientationResponse;

        public event Action OnTrackingApiVersionResponse;
        public event Action OnTrackingServerInfoResponse;
        public event Action OnTrackingDeviceInfoResponse;

        public string trackingServiceVersion { get; }

        public uint? connectedDeviceID { get; }
        public string connectedDeviceFirmware { get; }
        public string connectedDeviceSerial { get; }

        void RequestGetAnalyticsMode();
        void RequestSetAnalyticsMode(bool enabled);

        void RequestGetAllowImages();
        void RequestSetAllowImages(bool enabled);

        void RequestGetImageMask();
        void RequestSetImageMask(double _left, double _right, double _top, double _bottom);

        void RequestGetCameraOrientation();
        void RequestSetCameraOrientation(bool reverseOrientation);

        void RequestGetDeviceInfo();
        void RequestGetDevices();
        void RequestGetServerInfo();
        void RequestGetVersion();
    }
}
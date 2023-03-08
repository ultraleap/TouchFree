using System;
using System.Threading.Tasks;

namespace Ultraleap.TouchFree.Library.Connections.DiagnosticApi;

public readonly record struct GetAllInfo(bool GetMasking, bool GetAllowImage, bool GetOrientation, bool GetAnalytics);
public readonly record struct DiagnosticData(MaskingData? Masking, bool? AllowImages, bool? CameraOrientation, bool? Analytics);

public interface ITrackingDiagnosticApi
{
    public event Action OnTrackingApiVersionResponse;
    public event Action OnTrackingServerInfoResponse;
    public event Action OnTrackingDeviceInfoResponse;

    public string trackingServiceVersion { get; }

    public string connectedDeviceFirmware { get; }
    public string connectedDeviceSerial { get; }

    Task<DiagnosticData> RequestGetAll(GetAllInfo request);
    Task RequestSetAll(DiagnosticData data);

    void RequestGetAnalyticsMode();
    void RequestSetAnalyticsMode(bool enabled);
    public event Action<Result<bool>> OnAnalyticsResponse;

    void RequestGetAllowImages();
    void RequestSetAllowImages(bool enabled);
    public event Action<Result<bool>> OnAllowImagesResponse;

    void RequestGetImageMask();
    void RequestSetImageMask(MaskingData maskingData);
    public event Action<Result<MaskingData>> OnMaskingResponse;

    void RequestGetCameraOrientation();
    void RequestSetCameraOrientation(bool reverseOrientation);
    public event Action<Result<bool>> OnCameraOrientationResponse;

    bool RequestGetDeviceInfo();
    void RequestGetDevices();
    void RequestGetServerInfo();
    void RequestGetVersion();
}

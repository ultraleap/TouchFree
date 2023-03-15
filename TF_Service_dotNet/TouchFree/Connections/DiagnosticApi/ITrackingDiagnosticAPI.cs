using System;
using System.Threading.Tasks;
using Ultraleap.TouchFree.Library.Configuration;

namespace Ultraleap.TouchFree.Library.Connections.DiagnosticApi;

public readonly record struct DiagnosticData(MaskingData? Masking, bool? AllowImages, bool? CameraOrientation, bool? Analytics)
{
    public static explicit operator DiagnosticData(TrackingConfig config) => new(
        (MaskingData)config.Mask,
        config.AllowImages,
        config.CameraReversed,
        config.AnalyticsEnabled);
}

public readonly record struct DeviceInfo(uint DeviceId, string Firmware, string Serial, string Type);
public readonly record struct ApiInfo(string ServiceVersion, string ProtocolVersion);

public interface ITrackingDiagnosticApi
{
    public event Action<DeviceInfo> DeviceConnected;
    public event Action<DeviceInfo> DeviceDisconnected;

    public ApiInfo? ApiInfo { get; }
    public DeviceInfo? ConnectedDevice { get; }

    Task<DeviceInfo?> UpdateDeviceStatus();
    Task<DiagnosticData> RequestGet();
    Task RequestSet(DiagnosticData data);
}

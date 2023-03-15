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

public readonly record struct DeviceInfo(uint DeviceId, string Firmware, string Serial, string Type)
{
    internal DeviceInfo(DiagnosticDevice device)
        : this(device.device_id, device.device_firmware, device.serial_number, device.type)
    { }
}
public readonly record struct ApiInfo(string ServiceVersion, string ProtocolVersion);


public interface ITrackingDiagnosticApi
{
    public ApiInfo? ApiInfo { get; }
    Task<DeviceInfo?> RequestDeviceInfo();
    Task<DiagnosticData> RequestGet();
    Task RequestSet(DiagnosticData data);
}

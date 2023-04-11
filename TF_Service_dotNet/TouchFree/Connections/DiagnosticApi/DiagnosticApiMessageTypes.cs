namespace Ultraleap.TouchFree.Library.Connections.DiagnosticApi;

enum DApiMsgTypes
{
    GetDevices,
    GetServerInfo,
    GetVersion,

    GetAllowImages,
    SetAllowImages,

    GetAnalyticsEnabled,
    SetAnalyticsEnabled,

    GetCameraOrientation,
    SetCameraOrientation,

    GetImageMask,
    SetImageMask,
}

readonly record struct DApiMessage(string type)
{
    public DApiMessage(DApiMsgTypes type) :this(type.ToString()) {}
}

readonly record struct DApiPayloadMessage<T>(string type, T payload)
{
    public DApiPayloadMessage(DApiMsgTypes type, T payload) : this(type.ToString(), payload) {}
}

readonly record struct DeviceIdPayload(uint device_id);

readonly record struct ImageMaskDataPayload(uint device_id, double lower, double upper, double left, double right)
{
    public static explicit operator ImageMaskDataPayload(in MaskingData other) => new()
    {
        left = other.left,
        right = other.right,
        upper = other.upper,
        lower = other.lower
    };
        
    public static explicit operator MaskingData(in ImageMaskDataPayload other) => new()
    {
        left = other.left,
        lower = other.lower,
        right = other.right,
        upper = other.upper
    };
}

readonly record struct DiagnosticDevicePayload(uint device_id,
    string type,
    uint clients,
    bool streaming,
    string serial_number,
    string device_firmware);

readonly record struct ServiceInfoPayload(string server_version);

readonly record struct CameraOrientationPayload(uint device_id, string camera_orientation);
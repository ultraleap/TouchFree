namespace Ultraleap.TouchFree.Library.Connections.DiagnosticApi;

public enum DApiMsgTypes
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

class DApiMessage
{
    public DApiMessage() { }
    public DApiMessage(DApiMsgTypes _type)
    {
        type = _type.ToString();
    }
    public string type;
}

class DApiPayloadMessage<T> : DApiMessage
{
    public DApiPayloadMessage() { }
    public DApiPayloadMessage(DApiMsgTypes _type, T _payload) : base(_type)
    {
        payload = _payload;
    }
    public T payload;
}

struct DeviceIdPayload
{
    public uint device_id;
}

struct ImageMaskData
{
    public double lower;
    public double upper;
    public double right;
    public double left;
    public uint device_id;

    public static explicit operator ImageMaskData(MaskingData other) => new()
    {
        left = other.left,
        right = other.right,
        upper = other.upper,
        lower = other.lower
    };
        
    public static explicit operator MaskingData(ImageMaskData other) => new()
    {
        left = other.left,
        lower = other.lower,
        right = other.right,
        upper = other.upper
    };
}

struct DiagnosticDevice
{
    public uint device_id;
    public string type;
    public uint clients;
    public bool streaming;
    public string serial_number;
    public string device_firmware;
}

struct ServiceInfoPayload
{
    public string server_version;
}

struct CameraOrientationPayload
{
    public uint device_id;
    public string camera_orientation;
}
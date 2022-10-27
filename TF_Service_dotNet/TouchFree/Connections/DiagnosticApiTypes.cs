namespace Ultraleap.TouchFree.Library.Connections
{
    public enum DApiMsgTypes
    {
        GetDeviceInfo,
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

    struct DApiResponseCache {
        DApiMsgTypes type;
        string requestID;
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

    public struct ImageMaskData
    {
        public double lower;
        public double upper;
        public double right;
        public double left;
        public uint device_id;

        public static explicit operator ImageMaskData(Configuration.MaskingData other) => new()
        {
            left = other.Left,
            right = other.Right,
            upper = other.Upper,
            lower = other.Lower
        };
    }

    struct DiagnosticDevice
    {
        public uint device_id;
        public string type;
        public uint clients;
        public bool streaming;
    }

    struct DiagnosticDeviceInformation
    {
        public string device_hardware;
        public string device_serial;
        public string device_firmware;
        public uint device_id;
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
}
using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;

public class DiagnosticAPI : IDisposable
{
    private static string uri = "ws://127.0.0.1:1024/";

    public enum Status { Closed, Connecting, Connected, Expired }
    private Status status = Status.Expired;
    private WebSocket webSocket = null;

    public delegate void MaskingDataDelegate(float _left, float _right, float _top, float _bottom);

    public static event MaskingDataDelegate OnGetMaskingResponse;
    public static event Action OnTrackingApiVersionResponse;
    public static event Action OnTrackingServerInfoResponse;
    public static event Action OnTrackingDeviceInfoResponse;
    public static event Action OnAllowImagesResponse;
    public static event Action OnCameraOrientationResponse;
    public static event Action<bool> OnGetAnalyticsEnabledResponse;

    public uint connectedDeviceID;
    public string connectedDeviceFirmware;
    public string connectedDeviceSerial;
    public bool maskingAllowed = false;
    public bool cameraReversed = false;
    public bool? allowImages;
    public Version version { get; private set; }
    public string trackingServiceVersion { get; private set; }

    const string minimumMaskingAPIVerison = "2.1.0";

    ConcurrentQueue<string> newMessages = new ConcurrentQueue<string>();

    public DiagnosticAPI(MonoBehaviour _creatorMonobehaviour)
    {
        Connect();
        MessageQueueReader();
    }

    async Task MessageQueueReader()
    {
        while (true)
        {
            await Task.Delay(10);
            if (newMessages.TryDequeue(out var message))
            {
                HandleMessage(message);
            }
        }
    }

    private void Connect()
    {
        if (status == Status.Connecting || status == Status.Connected)
        {
            return;
        }

        bool requireSetup = status == Status.Expired;
        status = Status.Connecting;

        if (requireSetup)
        {
            if (webSocket != null)
            {
                webSocket.Close();
            }

            webSocket = new WebSocket(uri);
            webSocket.OnMessage += onMessage;
            webSocket.OnOpen += (sender, e) =>
            {
                Debug.Log("DiagnosticAPI open... ");
                status = Status.Connected;
                GetServerInfo();
                GetDevices();
                GetVersion();
            };
            webSocket.OnError += (sender, e) =>
            {
                Debug.Log("DiagnosticAPI error! " + e.Message + "\n" + e.Exception.ToString());
                status = Status.Expired;
            };
            webSocket.OnClose += (sender, e) =>
            {
                Debug.Log("DiagnosticAPI closed. " + e.Reason);
                status = Status.Closed;
            };
        }

        try
        {
            webSocket.Connect();
        }
        catch (Exception ex)
        {
            Debug.Log("DiagnosticAPI connection exception... " + "\n" + ex.ToString());
            status = Status.Expired;
        }
    }

    private void onMessage(object sender, MessageEventArgs e)
    {
        if (e.IsText)
        {
            newMessages.Enqueue(e.Data);
        }
    }

    void HandleMessage(string _message)
    {
        var response = JsonUtility.FromJson<DiagnosticApiResponse>(_message);

        switch (response.type)
        {
            case "GetImageMask":
                try
                {
                    var maskingResponse = JsonUtility.FromJson<GetImageMaskResponse>(_message);
                    OnGetMaskingResponse?.Invoke(
                        (float)maskingResponse.payload.left,
                        (float)maskingResponse.payload.right,
                        (float)maskingResponse.payload.upper,
                        (float)maskingResponse.payload.lower);
                }
                catch
                {
                    Debug.Log("DiagnosticAPI - Could not parse GetImageMask data: " + _message);
                }
                break;
            case "GetDevices":
                try
                {
                    GetDevicesResponse devicesResponse = JsonUtility.FromJson<GetDevicesResponse>(_message);
                    if (devicesResponse.payload.Length > 0)
                    {
                        connectedDeviceID = devicesResponse.payload[0].device_id;
                    }
                }
                catch
                {
                    Debug.Log("DiagnosticAPI - Could not parse GetDevices data: " + _message);
                }
                break;
            case "GetVersion":
                try
                {
                    GetVersionResponse versionResponse = JsonUtility.FromJson<GetVersionResponse>(_message);
                    HandleDiagnosticAPIVersion(versionResponse.payload);
                }
                catch
                {
                    Debug.Log("DiagnosticAPI - Could not parse Version data: " + _message);
                }
                break;
            case "GetAnalyticsEnabled":
                try
                {
                    var data = JsonUtility.FromJson<GetAnalyticsEnabledResponse>(_message);
                    OnGetAnalyticsEnabledResponse?.Invoke(data.payload);
                }
                catch
                {
                    Debug.Log("DiagnosticAPI - Could not parse analytics response: " + _message);
                }
                break;
            case "GetServerInfo":
                try
                {
                    var data = JsonUtility.FromJson<GetServerInfoResponse>(_message);
                    trackingServiceVersion = data.payload.server_version ?? trackingServiceVersion;
                    OnTrackingServerInfoResponse?.Invoke();
                }
                catch (Exception ex)
                {
                    Debug.Log("DiagnosticAPI - Could not parse server info response: " + _message);
                }
                break;
            case "GetDeviceInfo":
                try
                {
                    var data = JsonUtility.FromJson<GetDeviceInfoResponse>(_message);
                    connectedDeviceFirmware = data?.payload.device_firmware ?? connectedDeviceFirmware;
                    connectedDeviceSerial = data?.payload.device_serial ?? connectedDeviceSerial;
                    OnTrackingDeviceInfoResponse?.Invoke();
                }
                catch (Exception ex)
                {
                    Debug.Log("DiagnosticAPI - Could not parse device info response: " + _message);
                }
                break;
            case "GetAllowImages":
                try
                {
                    GetAllowImagesResponse data = JsonUtility.FromJson<GetAllowImagesResponse>(_message);
                    allowImages = data?.payload ?? false;
                    OnAllowImagesResponse?.Invoke();
                }
                catch (Exception ex)
                {
                    Debug.Log("DiagnosticAPI - Could not parse allow images response: " + _message);
                }
                break;
            case "GetCameraOrientation":
                try
                {
                    GetCameraOrientationResponse data = JsonUtility.FromJson<GetCameraOrientationResponse>(_message);
                    cameraReversed = data?.payload.camera_orientation == "fixed-inverted";
                    OnCameraOrientationResponse?.Invoke();
                }
                catch (Exception ex)
                {
                    Debug.Log("DiagnosticAPI - Could not parse camera orientation response: " + _message);
                }
                break;
            case "SetAnalyticsEnabled":
            case "SetCameraOrientation":
            case "SetAllowImages":
                // We are expecting responses from these requests but do not need to consume them
                break;
            default:
                Debug.Log("DiagnosticAPI - Could not parse response of type: " + response.type + " with message: " + _message);
                break;
        }
    }

    public void HandleDiagnosticAPIVersion(string _version)
    {
        Version curVersion = new Version(_version);
        Version minVersion = new Version(minimumMaskingAPIVerison);
        version = curVersion;

        if (curVersion.CompareTo(minVersion) >= 0)
        {
            // Version allows masking
            maskingAllowed = true;
        }
        else
        {
            // Version does not allow masking
            maskingAllowed = false;
        }
        OnTrackingApiVersionResponse?.Invoke();
    }

    public void Request(object payload)
    {
        if (status == Status.Connected)
        {
            var requestMessage = JsonUtility.ToJson(payload, true);
            webSocket.Send(requestMessage);
        }
        else
        {
            Connect();
        }
    }

    public void SetMasking(float _left, float _right, float _top, float _bottom)
    {
        Request(new SetImageMaskRequest()
        {
            payload = new ImageMaskData()
            {
                device_id = connectedDeviceID,
                left = _left,
                right = _right,
                upper = _top,
                lower = _bottom
            }
        });
    }

    public void GetAnalyticsMode()
    {
        Request(new GetAnalyticsEnabledRequest());
    }

    public void GetImageMask()
    {
        Request(new GetImageMaskRequest() { payload = new DeviceIdPayload { device_id = connectedDeviceID } });
    }

    public void SetAnalyticsMode(bool enabled)
    {
        Request(new SetAnalyticsEnabledRequest() { payload = enabled });
    }

    public void GetDevices()
    {
        Request(new GetDevicesRequest());
    }

    public void GetVersion()
    {
        Request(new GetVersionRequest());
    }

    public void GetServerInfo()
    {
        Request(new GetServerInfoRequest());
    }

    public void GetDeviceInfo()
    {
        Request(new GetDeviceInfoRequest()
        {
            payload = new DeviceIdPayload() { device_id = connectedDeviceID }
        });
    }

    public void GetAllowImages()
    {
        Request(new GetAllowImagesRequest());
    }

    public void SetAllowImages(bool enabled)
    {
        Request(new SetAllowImagesRequest() { payload = enabled });
    }

    public void GetCameraOrientation()
    {
        Request(new GetCameraOrientationRequest()
        {
            payload = new DeviceIdPayload() { device_id = connectedDeviceID }
        });
    }

    public void SetCameraOrientation(bool reverseOrientation)
    {
        Request(new SetCameraOrientationRequest()
        {
            payload = new CameraOrientationPayload() { device_id = connectedDeviceID, camera_orientation = reverseOrientation ? "fixed-inverted" : "fixed-normal" }
        });
    }

    void IDisposable.Dispose()
    {
        status = Status.Expired;
        webSocket.Close();
    }

    #region "Diagnostic API Messages"

    [Serializable]
    class DiagnosticApiRequest
    {
        public DiagnosticApiRequest() { }
        protected DiagnosticApiRequest(string _type)
        {
            type = _type;
        }
        public string type;
    }

    [Serializable]
    class DiagnosticApiResponse
    {
        public DiagnosticApiResponse() { }
        protected DiagnosticApiResponse(string _type)
        {
            type = _type;
        }
        public string type;
        public int? status;
    }

    [Serializable]
    class SetImageMaskRequest : DiagnosticApiRequest
    {
        public SetImageMaskRequest() : base("SetImageMask") { }
        public ImageMaskData payload;
    }

    [Serializable]
    class SetImageMaskResponse : DiagnosticApiResponse
    {
        public SetImageMaskResponse() : base("SetImageMask") { }
        public ImageMaskData payload;
    }

    [Serializable]
    class GetImageMaskRequest : DiagnosticApiRequest
    {
        public GetImageMaskRequest() : base("GetImageMask") { }
        public DeviceIdPayload payload;
    }

    [Serializable]
    class GetImageMaskResponse : DiagnosticApiResponse
    {
        public GetImageMaskResponse() : base("GetImageMask") { }
        public ImageMaskData payload;
    }

    [Serializable]
    class GetVersionRequest : DiagnosticApiRequest
    {
        public GetVersionRequest() : base("GetVersion") { }
    }

    [Serializable]
    class GetVersionResponse : DiagnosticApiResponse
    {
        public GetVersionResponse() : base("GetVersion") { }
        public string payload;
    }

    [Serializable]
    class GetAnalyticsEnabledRequest : DiagnosticApiRequest
    {
        public GetAnalyticsEnabledRequest() : base("GetAnalyticsEnabled") { }
    }

    [Serializable]
    class GetAnalyticsEnabledResponse : DiagnosticApiResponse
    {
        public GetAnalyticsEnabledResponse() : base("GetAnalyticsEnabled") { }
        public bool payload;
    }

    [Serializable]
    class SetAnalyticsEnabledRequest : DiagnosticApiRequest
    {
        public SetAnalyticsEnabledRequest() : base("SetAnalyticsEnabled") { }
        public bool payload;
    }

    [Serializable]
    class GetDevicesRequest : DiagnosticApiRequest
    {
        public GetDevicesRequest() : base("GetDevices") { }
    }

    [Serializable]
    class GetDevicesResponse : DiagnosticApiResponse
    {
        public GetDevicesResponse() : base("GetDevices") { }
        public DiagnosticDevice[] payload;
    }

    [Serializable]
    class GetDeviceInfoRequest : DiagnosticApiRequest
    {
        public GetDeviceInfoRequest() : base("GetDeviceInfo") { }
        public DeviceIdPayload payload;
    }

    [Serializable]
    class GetDeviceInfoResponse : DiagnosticApiResponse
    {
        public GetDeviceInfoResponse() : base("GetDeviceInfo") { }
        public DiagnosticDeviceInformation payload;
    }

    [Serializable]
    class GetServerInfoRequest : DiagnosticApiRequest
    {
        public GetServerInfoRequest() : base("GetServerInfo") { }
    }

    [Serializable]
    class GetServerInfoResponse : DiagnosticApiResponse
    {
        public GetServerInfoResponse() : base("GetServerInfo") { }
        public ServiceInfoPayload payload;
    }

    [Serializable]
    class GetAllowImagesRequest : DiagnosticApiRequest
    {
        public GetAllowImagesRequest() : base("GetAllowImages") { }
    }

    [Serializable]
    class GetAllowImagesResponse : DiagnosticApiResponse
    {
        public GetAllowImagesResponse() : base("GetAllowImages") { }
        public bool payload;
    }

    [Serializable]
    class SetAllowImagesRequest : DiagnosticApiRequest
    {
        public SetAllowImagesRequest() : base("SetAllowImages") { }
        public bool payload;
    }

    [Serializable]
    class GetCameraOrientationRequest : DiagnosticApiRequest
    {
        public GetCameraOrientationRequest() : base("GetCameraOrientation") { }
        public DeviceIdPayload payload;
    }

    [Serializable]
    class GetCameraOrientationResponse : DiagnosticApiResponse
    {
        public GetCameraOrientationResponse() : base("GetCameraOrientation") { }
        public CameraOrientationPayload payload;
    }

    [Serializable]
    class SetCameraOrientationRequest : DiagnosticApiRequest
    {
        public SetCameraOrientationRequest() : base("SetCameraOrientation") { }
        public CameraOrientationPayload payload;
    }

    [Serializable]
    struct DeviceIdPayload
    {
        public uint device_id;
    }

    [Serializable]
    struct ImageMaskData
    {
        public double lower;
        public double upper;
        public double right;
        public double left;
        public uint device_id;
    }

    [Serializable]
    struct DiagnosticDevice
    {
        public uint device_id;
        public string type;
        public uint clients;
        public bool streaming;
    }

    [Serializable]
    struct DiagnosticDeviceInformation
    {
        public string device_hardware;
        public string device_serial;
        public string device_firmware;
        public uint device_id;
    }

    [Serializable]
    struct ServiceInfoPayload
    {
        public string server_version;
    }

    [Serializable]
    struct CameraOrientationPayload
    {
        public uint device_id;
        public string camera_orientation;
    }

    #endregion
}
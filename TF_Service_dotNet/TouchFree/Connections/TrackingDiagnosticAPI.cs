using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Ultraleap.TouchFree.Library.Configuration;
using WebSocketSharp;

namespace Ultraleap.TouchFree.Library.Connections
{
    public class TrackingDiagnosticApi : ITrackingDiagnosticApi, IDisposable
    {
        private static string uri = "ws://127.0.0.1:1024/";
        private const string minimumMaskingAPIVerison = "2.1.0";
        private readonly IConfigManager configManager;
        public string trackingServiceVersion;
        public Version version;

        public uint? connectedDeviceID;
        public string connectedDeviceFirmware;
        public string connectedDeviceSerial;
        public bool maskingAllowed = false;
        public bool cameraReversed = false;
        public bool? allowImages;

        public event Action<ImageMaskData?, String> OnMaskingResponse;
        public event Action<bool?, string> OnAnalyticsResponse;
        public event Action<bool?, string> OnAllowImagesResponse;
        public event Action<bool?, string> OnCameraOrientationResponse;

        public event Action OnTrackingApiVersionResponse;
        public event Action OnTrackingServerInfoResponse;
        public event Action OnTrackingDeviceInfoResponse;

        public enum Status { Closed, Connecting, Connected, Expired }
        private Status status = Status.Expired;
        private WebSocket webSocket = null;

        ConcurrentQueue<string> newMessages = new ConcurrentQueue<string>();
        private bool loadingAllowImagesConfiguration;
        private bool loadingAnalyticsModeConfiguration;
        private bool loadingCameraOrientationConfiguration;
        private bool loadingImageMaskConfiguration;

        private TrackingConfig trackingConfig = new TrackingConfig();

        public TrackingDiagnosticApi(IConfigManager _configManager)
        {
            configManager = _configManager;
            Connect();
            MessageQueueReader();

            configManager.OnTrackingConfigUpdated += EnforceTrackingConfiguration;

            OnMaskingResponse += TrackingDiagnosticApi_OnMaskingResponse;
            OnAnalyticsResponse += TrackingDiagnosticApi_OnAnalyticsResponse;
            OnAllowImagesResponse += TrackingDiagnosticApi_OnAllowImagesResponse;
            OnCameraOrientationResponse += TrackingDiagnosticApi_OnCameraOrientationResponse;
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

        public void TriggerUpdatingTrackingConfiguration()
        {
            GetDevices();
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
                    Console.WriteLine("DiagnosticAPI open... ");
                    status = Status.Connected;
                    GetServerInfo();
                    GetDevices();
                    GetVersion();
                };
                webSocket.OnError += (sender, e) =>
                {
                    Console.WriteLine("DiagnosticAPI error! " + e.Message + "\n");
                    status = Status.Expired;
                };
                webSocket.OnClose += (sender, e) =>
                {
                    Console.WriteLine("DiagnosticAPI closed. " + e.Reason);
                    status = Status.Closed;
                };
            }

            try
            {
                webSocket.Connect();
            }
            catch (Exception ex)
            {
                Console.WriteLine("DiagnosticAPI connection exception... " + "\n" + ex.ToString());
                status = Status.Expired;
            }
        }

        private void onMessage(object sender, MessageEventArgs e)
        {
            if (e.IsText)
            {
                newMessages.Enqueue(e.Data + " ");
            }
        }

        void HandleMessage(string _message)
        {
            var response = JsonConvert.DeserializeObject<DApiMessage>(_message);

            var parsed = Enum.TryParse(response.type, out DApiMsgTypes status);

            if (!parsed)
            {
                Console.WriteLine("DiagnosticAPI - Could not parse response of type: " + response.type + " with message: " + _message);
            }
            else
            {
                switch (status)
                {
                    case DApiMsgTypes.GetImageMask:
                    case DApiMsgTypes.SetImageMask:
                        try
                        {
                            var maskingResponse = JsonConvert.DeserializeObject<DApiPayloadMessage<ImageMaskData>>(_message);

                            this.OnMaskingResponse?.Invoke(maskingResponse.payload, "Image Mask State");
                        }
                        catch
                        {
                            Console.WriteLine("DiagnosticAPI - Could not parse GetImageMask data: " + _message);

                            this.OnMaskingResponse?.Invoke(null, $"Could not access Masking data. Tracking Response: \"{_message}\"");
                        }
                        break;

                    case DApiMsgTypes.GetDevices:
                        try
                        {
                            var devicesResponse = JsonConvert.DeserializeObject<DApiPayloadMessage<DiagnosticDevice[]>>(_message);
                            if (devicesResponse.payload.Length > 0)
                            {
                                connectedDeviceID = devicesResponse.payload[0].device_id;
                            }

                            HandleDeviceConnectedRequests();
                        }
                        catch
                        {
                            Console.WriteLine("DiagnosticAPI - Could not parse GetDevices data: " + _message);
                        }
                        break;

                    case DApiMsgTypes.GetVersion:
                        try
                        {
                            var getVersionResponse = JsonConvert.DeserializeObject<DApiPayloadMessage<string>>(_message);
                            HandleDiagnosticAPIVersion(getVersionResponse.payload);
                        }
                        catch
                        {
                            Console.WriteLine("DiagnosticAPI - Could not parse Version data: " + _message);

                        }
                        break;

                    case DApiMsgTypes.GetAnalyticsEnabled:
                    case DApiMsgTypes.SetAnalyticsEnabled:
                        try
                        {
                            var data = JsonConvert.DeserializeObject<DApiPayloadMessage<bool>>(_message);

                            this.OnAnalyticsResponse?.Invoke(data.payload, "Analytics State");
                        }
                        catch
                        {
                            Console.WriteLine("DiagnosticAPI - Could not parse analytics response: " + _message);

                            this.OnAnalyticsResponse?.Invoke(null, $"Could not access Analytics state. Tracking Response: \"{_message}\"");
                        }
                        break;

                    case DApiMsgTypes.GetServerInfo:
                        try
                        {
                            var data = JsonConvert.DeserializeObject<DApiPayloadMessage<ServiceInfoPayload>>(_message);
                            trackingServiceVersion = data.payload.server_version ?? trackingServiceVersion;
                            OnTrackingServerInfoResponse?.Invoke();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("DiagnosticAPI - Could not parse server info response: " + _message);
                        }
                        break;

                    case DApiMsgTypes.GetDeviceInfo:
                        try
                        {
                            var data = JsonConvert.DeserializeObject<DApiPayloadMessage<DiagnosticDeviceInformation>>(_message);
                            connectedDeviceFirmware = data?.payload.device_firmware ?? connectedDeviceFirmware;
                            connectedDeviceSerial = data?.payload.device_serial ?? connectedDeviceSerial;
                            OnTrackingDeviceInfoResponse?.Invoke();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("DiagnosticAPI - Could not parse device info response: " + _message);
                        }
                        break;

                    case DApiMsgTypes.SetAllowImages:
                    case DApiMsgTypes.GetAllowImages:
                        try
                        {

                            var data = JsonConvert.DeserializeObject<DApiPayloadMessage<bool>>(_message);
                            allowImages = data?.payload ?? false;

                            this.OnAllowImagesResponse?.Invoke(allowImages, "AllowImages state");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("DiagnosticAPI - Could not parse allow images response: " + _message);

                            this.OnAllowImagesResponse?.Invoke(null, $"Could not access AllowImages state. Tracking Response: \"{_message}\"");
                        }
                        break;

                    case DApiMsgTypes.GetCameraOrientation:
                    case DApiMsgTypes.SetCameraOrientation:
                        try
                        {
                            var data = JsonConvert.DeserializeObject<DApiPayloadMessage<CameraOrientationPayload>>(_message);
                            cameraReversed = data?.payload.camera_orientation == "fixed-inverted";

                            this.OnCameraOrientationResponse?.Invoke(cameraReversed, "CameraOrientation state");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("DiagnosticAPI - Could not parse camera orientation response: " + _message);

                            this.OnCameraOrientationResponse?.Invoke(null, $"Could not access CameraOrientation state. Tracking Response: \"{_message}\"");
                        }
                        break;

                    default:
                        Console.WriteLine("DiagnosticAPI - Could not parse response of type: " + response.type + " with message: " + _message);
                        break;
                }
            }
        }

        private void EnforceTrackingConfiguration(TrackingConfigInternal trackingConfig)
        {
            SetAllowImages(trackingConfig.AllowImages);
            SetAnalyticsMode(trackingConfig.AnalyticsEnabled);
            SetCameraOrientation(trackingConfig.CameraReversed);
            SetMasking(trackingConfig.Mask.Left, trackingConfig.Mask.Right, trackingConfig.Mask.Upper, trackingConfig.Mask.Lower);
        }

        private void LoadTrackingConfigurationWithDelay()
        {
            if (connectedDeviceID.HasValue)
            {
                loadingAllowImagesConfiguration = true;
                loadingAnalyticsModeConfiguration = true;
                loadingCameraOrientationConfiguration = true;
                loadingImageMaskConfiguration = true;

                GetAllowImages();
                GetAnalyticsMode();
                GetCameraOrientation();
                GetImageMask();
            }
            // TODO: handle no device cases
        }

        public void HandleDeviceConnectedRequests()
        {
            // Delay enforcing tracking configuration until we have a device connected
            if (configManager.TrackingConfig != null)
            {
                EnforceTrackingConfiguration(configManager.TrackingConfig);
            }
            else
            {
                LoadTrackingConfigurationWithDelay();
            }
        }

        private void TrackingDiagnosticApi_OnCameraOrientationResponse(bool? arg1, string arg2)
        {
            if (loadingCameraOrientationConfiguration && arg1.HasValue)
            {
                loadingCameraOrientationConfiguration = false;
                trackingConfig.CameraReversed = arg1.Value;

                SaveTrackingConfiguration();
            }
        }

        private void TrackingDiagnosticApi_OnAllowImagesResponse(bool? arg1, string arg2)
        {
            if (loadingAllowImagesConfiguration && arg1.HasValue)
            {
                loadingAllowImagesConfiguration = false;
                trackingConfig.AllowImages = arg1.Value;

                SaveTrackingConfiguration();
            }
        }

        private void TrackingDiagnosticApi_OnAnalyticsResponse(bool? arg1, string arg2)
        {
            if (loadingAnalyticsModeConfiguration && arg1.HasValue)
            {
                loadingAnalyticsModeConfiguration = false;
                trackingConfig.AnalyticsEnabled = arg1.Value;

                SaveTrackingConfiguration();
            }
        }

        private void TrackingDiagnosticApi_OnMaskingResponse(ImageMaskData? arg1, string arg2)
        {
            if (loadingImageMaskConfiguration && arg1.HasValue)
            {
                loadingImageMaskConfiguration = false;
                trackingConfig.Mask.Upper = (float)arg1.Value.upper;
                trackingConfig.Mask.Right = (float)arg1.Value.right;
                trackingConfig.Mask.Lower = (float)arg1.Value.lower;
                trackingConfig.Mask.Left = (float)arg1.Value.left;

                SaveTrackingConfiguration();
            }
        }

        private void SaveTrackingConfiguration()
        {
            if (!loadingAllowImagesConfiguration &&
                !loadingAnalyticsModeConfiguration &&
                !loadingCameraOrientationConfiguration &&
                !loadingImageMaskConfiguration)
            {
                TrackingConfigFile.SaveConfig(trackingConfig);
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

        public async void Request(object payload)
        {
            if (!TrySendRequest(payload))
            {
                Connect();
                for (var attempt = 0; attempt < 10; attempt++)
                {
                    await Task.Delay(1000);
                    if (TrySendRequest(payload))
                    {
                        break;
                    }
                }
            }
        }

        public bool TrySendRequest(object payload)
        {
            var canSendRequest = status == Status.Connected;
            if (canSendRequest)
            {
                var requestMessage = JsonConvert.SerializeObject(payload);
                webSocket.Send(requestMessage);
            }

            return canSendRequest;
        }

        public void GetAnalyticsMode()
        {
            Request(new DApiMessage(DApiMsgTypes.GetAnalyticsEnabled));
        }

        public void SetAnalyticsMode(bool enabled)
        {
            Request(new DApiPayloadMessage<bool>(DApiMsgTypes.SetAnalyticsEnabled, enabled));
        }

        public void GetImageMask()
        {
            if (connectedDeviceID.HasValue)
            {
                var payload = new DeviceIdPayload { device_id = connectedDeviceID.Value };
                Request(new DApiPayloadMessage<DeviceIdPayload>(DApiMsgTypes.GetImageMask, payload));
            }
            // TODO: handle not having a connected device
        }

        public void SetMasking(float _left, float _right, float _top, float _bottom)
        {
            if (connectedDeviceID.HasValue)
            {
                var payload = new ImageMaskData()
                {
                    device_id = connectedDeviceID.Value,
                    left = _left,
                    right = _right,
                    upper = _top,
                    lower = _bottom
                };
                Request(new DApiPayloadMessage<ImageMaskData>(DApiMsgTypes.SetImageMask, payload));
            }
            // TODO: handle not having a connected device
        }


        public void GetAllowImages()
        {
            Request(new DApiMessage(DApiMsgTypes.GetAllowImages));
        }

        public void SetAllowImages(bool enabled)
        {
            Request(new DApiPayloadMessage<bool>(DApiMsgTypes.SetAllowImages, enabled));
        }


        public void GetCameraOrientation()
        {
            if (connectedDeviceID.HasValue)
            {
                var payload = new DeviceIdPayload() { device_id = connectedDeviceID.Value };
                Request(new DApiPayloadMessage<DeviceIdPayload>(DApiMsgTypes.GetCameraOrientation, payload));
            }
            // TODO: handle not having a connected device
        }

        public void SetCameraOrientation(bool reverseOrientation)
        {
            if (connectedDeviceID.HasValue)
            {
                var payload = new CameraOrientationPayload() { device_id = connectedDeviceID.Value, camera_orientation = reverseOrientation ? "fixed-inverted" : "fixed-normal" };
                Request(new DApiPayloadMessage<CameraOrientationPayload>(DApiMsgTypes.SetCameraOrientation, payload));
            }
            // TODO: handle not having a connected device
        }

        public void GetDeviceInfo()
        {
            if (connectedDeviceID.HasValue)
            {
                var payload = new DeviceIdPayload() { device_id = connectedDeviceID.Value };
                Request(new DApiPayloadMessage<DeviceIdPayload>(DApiMsgTypes.GetDeviceInfo, payload));
            }
            // TODO: handle not having a connected device
        }

        public void GetDevices()
        {
            Request(new DApiMessage(DApiMsgTypes.GetDevices));
        }

        public void GetVersion()
        {
            Request(new DApiMessage(DApiMsgTypes.GetVersion));
        }

        public void GetServerInfo()
        {
            Request(new DApiMessage(DApiMsgTypes.GetServerInfo));
        }

        void IDisposable.Dispose()
        {
            status = Status.Expired;
            webSocket.Close();
        }
    }
}
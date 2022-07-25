using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;

using Newtonsoft.Json;
using WebSocketSharp;

namespace Ultraleap.TouchFree.Library.Connections
{
    public class TrackingDiagnosticApi : ITrackingDiagnosticApi, IDisposable
    {
        private static string uri = "ws://127.0.0.1:1024/";
        private const string minimumMaskingAPIVerison = "2.1.0";

        public string trackingServiceVersion;
        public Version version;

        public uint connectedDeviceID;
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

        public TrackingDiagnosticApi()
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
                    Console.WriteLine("DiagnosticAPI open... ");
                    status = Status.Connected;
                    GetServerInfo();
                    GetDevices();
                    GetVersion();
                };
                webSocket.OnError += (sender, e) =>
                {
                    Console.WriteLine("DiagnosticAPI error! " + e.Message + "\n" + e.Exception.ToString());
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
                newMessages.Enqueue(e.Data);
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
                var requestMessage = JsonConvert.SerializeObject(payload);
                webSocket.Send(requestMessage);
            }
            else
            {
                Connect();
            }
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
            var payload = new DeviceIdPayload { device_id = connectedDeviceID };
            Request(new DApiPayloadMessage<DeviceIdPayload>(DApiMsgTypes.GetImageMask, payload));
        }

        public void SetMasking(float _left, float _right, float _top, float _bottom)
        {
            var payload = new ImageMaskData()
            {
                device_id = connectedDeviceID,
                left = _left,
                right = _right,
                upper = _top,
                lower = _bottom
            };
            Request(new DApiPayloadMessage<ImageMaskData>(DApiMsgTypes.SetImageMask, payload));
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
            var payload = new DeviceIdPayload() { device_id = connectedDeviceID };
            Request(new DApiPayloadMessage<DeviceIdPayload>(DApiMsgTypes.GetCameraOrientation, payload));
        }

        public void SetCameraOrientation(bool reverseOrientation)
        {
            var payload = new CameraOrientationPayload() { device_id = connectedDeviceID, camera_orientation = reverseOrientation ? "fixed-inverted" : "fixed-normal" };
            Request(new DApiPayloadMessage<CameraOrientationPayload>(DApiMsgTypes.SetCameraOrientation, payload));
        }


        public void GetDeviceInfo()
        {
            var payload = new DeviceIdPayload() { device_id = connectedDeviceID };
            Request(new DApiPayloadMessage<DeviceIdPayload>(DApiMsgTypes.GetDeviceInfo, payload));
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
using Leap;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ultraleap.TouchFree.Library.Configuration;
using WebSocketSharp;

namespace Ultraleap.TouchFree.Library.Connections
{
    public class TrackingDiagnosticApi : ITrackingDiagnosticApi, IDisposable
    {
        private static string uri = "ws://127.0.0.1:1024/";
        private readonly IConfigManager configManager;
        private Version version;

        public string trackingServiceVersion { get; private set; }

        public uint? connectedDeviceID { get; private set; }
        public string connectedDeviceFirmware { get; private set; }
        public string connectedDeviceSerial { get; private set; }

        public event Action<Result<ImageMaskData>> OnMaskingResponse;
        public event Action<Result<bool>> OnAnalyticsResponse;
        public event Action<Result<bool>> OnAllowImagesResponse;
        public event Action<Result<bool>> OnCameraOrientationResponse;

        public event Action OnTrackingApiVersionResponse;
        public event Action OnTrackingServerInfoResponse;
        public event Action OnTrackingDeviceInfoResponse;

        // Represents a variable that will be "initialized" once set at least once after construction
        private class ConfigurationVariable<TData>
        {
            private readonly TrackingDiagnosticApi _diagnosticApi;
            private readonly bool _connectedDeviceRequired;
            private readonly Func<uint?, object> _getPayloadFunc;
            private readonly Func<TData, uint?, object> _setPayloadFunc;
            private readonly List<TData> _responsesToIgnore; // Not a queue as responses could come back out of order
            private TData _value;

            public ConfigurationVariable(
                TrackingDiagnosticApi diagnosticApi,
                bool connectedDeviceRequired,
                Func<uint?, object> getPayloadFunc,
                Func<TData, uint?, object> setPayloadFunc,
                TData initial = default)
            {
                _diagnosticApi = diagnosticApi;
                _connectedDeviceRequired = connectedDeviceRequired;
                _getPayloadFunc = getPayloadFunc;
                _setPayloadFunc = setPayloadFunc;
                _responsesToIgnore = new List<TData>();
                _value = initial;
            }

            public TData Value
            {
                get => _value;
                private set
                {
                    Initialized = true;
                    if (Equals(value, _value)) return;
                    _value = value;
                }
            }

            public void RequestGet() => _diagnosticApi.Request(_getPayloadFunc(_diagnosticApi.connectedDeviceID));
            public void RequestSet(TData value)
            {
                Value = value;
                if (_connectedDeviceRequired && !_diagnosticApi.connectedDeviceID.HasValue) return; // Only send the request if we have a device, when one is required

                lock (_responsesToIgnore)
                {
                    _responsesToIgnore.Add(value);
                    _diagnosticApi.Request(_setPayloadFunc(_value, _diagnosticApi.connectedDeviceID));
                }
            }

            public void HandleResponse(Result<TData> result)
            {
                // TODO: Do something with any errors?
                if (!result.TryGetValue(out var val)) return;
                lock (_responsesToIgnore)
                {
                    var found = _responsesToIgnore.Remove(val);
                    if (found) return;
                    Value = val;
                }
            }

            // Variable is not considered initialized until it has been set externally from the constructor.
            // Scenarios this is expected to happen:
            // a. Set by a request
            // b. Loaded from an existing config
            // c. Loaded from connected device if not initialized before connecting
            public bool Initialized { get; private set; }

            public void Match(Action<TData> initialized, Action uninitialized)
            {
                if (Initialized) initialized?.Invoke(Value);
                else uninitialized?.Invoke();
            }
        }

        private readonly ConfigurationVariable<ImageMaskData> maskingData;
        private readonly ConfigurationVariable<bool> allowImages;
        private readonly ConfigurationVariable<bool> cameraReversed;
        private readonly ConfigurationVariable<bool> analyticsEnabled;

        private WebSocket webSocket = null;
        private readonly ConcurrentQueue<string> newMessages = new();

        public TrackingDiagnosticApi(IConfigManager _configManager, ITrackingConnectionManager _trackingConnectionManager)
        {
            configManager = _configManager;

            // Payload functions commonly used between multiple configuration variables
            Func<uint?, object> DefaultGetPayloadFunc(DApiMsgTypes requestType) =>
                _ => new DApiMessage(requestType);
            Func<uint?, object> DeviceIdPayloadFunc(DApiMsgTypes requestType) =>
                deviceId => new DApiPayloadMessage<DeviceIdPayload>(requestType, new DeviceIdPayload { device_id = deviceId.GetValueOrDefault() });
            Func<T, uint?, object> DefaultSetPayloadFunc<T>(DApiMsgTypes requestType) =>
                (val, _) => new DApiPayloadMessage<T>(requestType, val);

            // Configuration variable setup
            maskingData = new ConfigurationVariable<ImageMaskData>(this, true,
                DeviceIdPayloadFunc(DApiMsgTypes.GetImageMask),
                (maskData, deviceId) =>
                {
                    var payload = maskData with { device_id = deviceId.GetValueOrDefault() };
                    return new DApiPayloadMessage<ImageMaskData>(DApiMsgTypes.SetImageMask, payload);
                });

            allowImages = new ConfigurationVariable<bool>(this, false,
                DefaultGetPayloadFunc(DApiMsgTypes.GetAllowImages),
                DefaultSetPayloadFunc<bool>(DApiMsgTypes.SetAllowImages));

            cameraReversed = new ConfigurationVariable<bool>(this, true,
                DeviceIdPayloadFunc(DApiMsgTypes.GetCameraOrientation),
                (reversed, deviceId) =>
                {
                    var payload = new CameraOrientationPayload
                    {
                        device_id = deviceId.GetValueOrDefault(),
                        camera_orientation = reversed ? "fixed-inverted" : "fixed-normal"
                    };
                    return new DApiPayloadMessage<CameraOrientationPayload>(DApiMsgTypes.SetCameraOrientation, payload);
                });

            analyticsEnabled = new ConfigurationVariable<bool>(this, false,
                DefaultGetPayloadFunc(DApiMsgTypes.GetAnalyticsEnabled),
                DefaultSetPayloadFunc<bool>(DApiMsgTypes.SetAnalyticsEnabled));

            Connect();
#pragma warning disable CS4014
            MessageQueueReader();
#pragma warning restore CS4014

            void SetTrackingConfigurationOnDevice(TrackingConfig config)
            {
                allowImages.RequestSet(config.AllowImages);
                analyticsEnabled.RequestSet(config.AnalyticsEnabled);
                cameraReversed.RequestSet(config.CameraReversed);
                maskingData.RequestSet((ImageMaskData)config.Mask);
            }
            // Get devices response will update the connected device and refresh tracking config
            void ControllerOnDevice(object sender, DeviceEventArgs e) => RequestGetDevices();

            // Works even when no device is connected
            void ControllerOnDeviceLost(object sender, DeviceEventArgs e) => RequestGetDevices();

            _configManager.OnTrackingConfigUpdated += SetTrackingConfigurationOnDevice;
            _trackingConnectionManager.Controller.Device += ControllerOnDevice;
            _trackingConnectionManager.Controller.DeviceLost += ControllerOnDeviceLost;

            OnMaskingResponse += maskingData.HandleResponse;
            OnAnalyticsResponse += analyticsEnabled.HandleResponse;
            OnAllowImagesResponse += allowImages.HandleResponse;
            OnCameraOrientationResponse += cameraReversed.HandleResponse;

            OnMaskingResponse += SetTrackingConfigIfUnset;
            OnAnalyticsResponse += SetTrackingConfigIfUnset;
            OnAllowImagesResponse += SetTrackingConfigIfUnset;
            OnCameraOrientationResponse += SetTrackingConfigIfUnset;
        }

        private void SetTrackingConfigIfUnset<T>(Result<T> _)
        {
            if (configManager.TrackingConfig == null &&
                maskingData.Initialized &&
                analyticsEnabled.Initialized &&
                allowImages.Initialized &&
                cameraReversed.Initialized &&
                !TrackingConfigFile.DoesConfigFileExist())
            {
                var trackingConfigurationToStore = new TrackingConfig()
                {
                    AllowImages = allowImages.Value,
                    AnalyticsEnabled = analyticsEnabled.Value,
                    CameraReversed = cameraReversed.Value,
                    Mask = new Configuration.MaskingData()
                    {
                        Left = maskingData.Value.left,
                        Right = maskingData.Value.right,
                        Lower = maskingData.Value.lower,
                        Upper = maskingData.Value.upper
                    }
                };

                configManager.TrackingConfig = trackingConfigurationToStore;
                TrackingConfigFile.SaveConfig(trackingConfigurationToStore);
            }
        }

        private async Task MessageQueueReader()
        {
            while (true)
            {
                await Task.Delay(10);
                while (newMessages.TryDequeue(out var message))
                {
                    HandleMessage(message);
                }

                // Only happens after dispose clears the web socket back to null
                if (webSocket == null)
                {
                    break;
                }
            }
        }

        private void Connect()
        {
            if (webSocket?.ReadyState is WebSocketState.Connecting or WebSocketState.Open)
            {
                return;
            }

            if (webSocket == null)
            {
                webSocket = new WebSocket(uri);
                webSocket.OnMessage += OnMessage;
                webSocket.OnOpen += (sender, e) =>
                {
                    TouchFreeLog.WriteLine("DiagnosticAPI open... ");
                    RequestGetServerInfo();
                    RequestGetDevices();
                    RequestGetVersion();
                };
                webSocket.OnError += (sender, e) =>
                {
                    TouchFreeLog.ErrorWriteLine($"DiagnosticAPI error! {e.Message}\n");
                };
                webSocket.OnClose += (sender, e) =>
                {
                    TouchFreeLog.WriteLine($"DiagnosticAPI closed. {e.Reason}");
                };
            }

            try
            {
                webSocket.Connect();
            }
            catch (Exception ex)
            {
                TouchFreeLog.ErrorWriteLine($"DiagnosticAPI connection exception... \n{ex}");
            }
        }

        private void OnMessage(object sender, MessageEventArgs e)
        {
            if (e.IsText)
            {
                // + " " is used to avoid stack overflowing by creating a new string to
                // avoiding keeping around the event args object
                // TODO: Different method of avoiding stack overflow exceptions?
                // NOTE: Suspected that overflows are related to this issue https://github.com/sta/websocket-sharp/issues/702
                newMessages.Enqueue(e.Data + " ");
            }
        }

        private void HandleMessage(string _message)
        {
            var response = JsonConvert.DeserializeObject<DApiMessage>(_message);

            var parsed = Enum.TryParse(response.type, out DApiMsgTypes status);

            void Handle<TPayload>(Action<TPayload> onSuccess, Action? onFailure = null)
            {
                try
                {
                    var payload = JsonConvert.DeserializeObject<DApiPayloadMessage<TPayload>>(_message);
                    if (payload == null)
                    {
                        TouchFreeLog.WriteLine($"DiagnosticAPI - Payload for {status.ToString()} failed to deserialize: {_message}");
                    }
                    else
                    {
                        onSuccess(payload.payload);
                    }
                }
                catch
                {
                    TouchFreeLog.WriteLine($"DiagnosticAPI - Could not parse {status.ToString()} data: {_message}");
                    onFailure?.Invoke();
                }
            }

            if (!parsed)
            {
                TouchFreeLog.WriteLine(
                    $"DiagnosticAPI - Could not parse response of type: {response.type} with message: {_message}");
            }
            else
            {
                switch (status)
                {
                    case DApiMsgTypes.GetImageMask:
                    case DApiMsgTypes.SetImageMask:
                        Handle<ImageMaskData>(payload => OnMaskingResponse?.Invoke(payload),
                            () => OnMaskingResponse?.Invoke($"Could not access Masking data. Tracking Response: \"{_message}\""));
                        break;

                    case DApiMsgTypes.GetAnalyticsEnabled:
                    case DApiMsgTypes.SetAnalyticsEnabled:
                        Handle<bool>(payload => OnAnalyticsResponse?.Invoke(payload),
                            () => OnAnalyticsResponse?.Invoke($"Could not access Analytics state. Tracking Response: \"{_message}\""));
                        break;

                    case DApiMsgTypes.SetAllowImages:
                    case DApiMsgTypes.GetAllowImages:
                        Handle<bool>(payload => OnAllowImagesResponse?.Invoke(payload),
                            () => OnAllowImagesResponse?.Invoke($"Could not access AllowImages state. Tracking Response: \"{_message}\""));
                        break;

                    case DApiMsgTypes.GetCameraOrientation:
                    case DApiMsgTypes.SetCameraOrientation:
                        Handle<CameraOrientationPayload>(payload => OnCameraOrientationResponse?.Invoke(payload.camera_orientation == "fixed-inverted"),
                            () => OnCameraOrientationResponse?.Invoke($"Could not access CameraOrientation state. Tracking Response: \"{_message}\""));
                        break;

                    case DApiMsgTypes.GetDevices:
                        Handle<DiagnosticDevice[]>(
                            devices =>
                            {
                                uint? newConnectedDeviceId = null;
                                if (devices.Length > 0)
                                {
                                    newConnectedDeviceId = devices[0].device_id;
                                }

                                if (connectedDeviceID != newConnectedDeviceId)
                                {
                                    connectedDeviceID = newConnectedDeviceId;

                                    if (connectedDeviceID != null)
                                    {
                                        // Set initialized variables to the device, get uninitialized variables
                                        allowImages.Match(RequestSetAllowImages, RequestGetAllowImages);
                                        analyticsEnabled.Match(RequestSetAnalyticsMode, RequestGetAnalyticsMode);
                                        cameraReversed.Match(RequestSetCameraOrientation, RequestGetCameraOrientation);
                                        maskingData.Match(data => RequestSetImageMask(data.left, data.right, data.upper, data.lower), RequestGetImageMask);
                                        RequestGetDeviceInfo();
                                    }
                                }
                            }); // TODO: Handle failure?
                        break;

                    case DApiMsgTypes.GetVersion:
                        Handle<string>(s =>
                        {
                            version = new Version(s);
                            OnTrackingApiVersionResponse?.Invoke();
                        }); // TODO: Handle failure?
                        break;

                    case DApiMsgTypes.GetServerInfo:
                        Handle<ServiceInfoPayload>(payload =>
                        {
                            trackingServiceVersion = payload.server_version;
                            OnTrackingServerInfoResponse?.Invoke();
                        }); // TODO: Handle failure?
                        break;

                    case DApiMsgTypes.GetDeviceInfo:
                        Handle<DiagnosticDeviceInformation>(information =>
                        {
                            connectedDeviceFirmware = information.device_firmware;
                            connectedDeviceSerial = information.device_serial;
                            OnTrackingDeviceInfoResponse?.Invoke();
                        }); // TODO: Handle failure?
                        break;

                    default:
                        TouchFreeLog.WriteLine(
                            $"DiagnosticAPI - Could not parse response of type: {response.type} with message: {_message}");
                        break;
                }
            }
        }

        private async void Request(object payload)
        {
            bool TrySend()
            {
                if (webSocket.ReadyState != WebSocketState.Open) return false;

                var requestMessage = JsonConvert.SerializeObject(payload);
                webSocket.Send(requestMessage);
                return true;
            }

            if (!TrySend())
            {
                Connect();
                for (var attempt = 0; attempt < 10; attempt++)
                {
                    await Task.Delay(1000);
                    if (TrySend())
                    {
                        break;
                    }
                }
            }
        }

        public void RequestGetAnalyticsMode() => analyticsEnabled.RequestGet();
        public void RequestSetAnalyticsMode(bool enabled) => analyticsEnabled.RequestSet(enabled);

        public void RequestGetImageMask() => maskingData.RequestGet();
        public void RequestSetImageMask(double _left, double _right, double _top, double _bottom) =>
            maskingData.RequestSet(new ImageMaskData
            {
                device_id = connectedDeviceID.GetValueOrDefault(),
                left = _left,
                right = _right,
                upper = _top,
                lower = _bottom
            });

        public void RequestGetAllowImages() => allowImages.RequestGet();
        public void RequestSetAllowImages(bool enabled) => allowImages.RequestSet(enabled);

        public void RequestGetCameraOrientation() => cameraReversed.RequestGet();
        public void RequestSetCameraOrientation(bool reverseOrientation) => cameraReversed.RequestSet(reverseOrientation);

        public void RequestGetDeviceInfo()
        {
            // Only send the request if we have a device
            if (!connectedDeviceID.HasValue) return;

            var payload = new DeviceIdPayload { device_id = connectedDeviceID.Value };
            Request(new DApiPayloadMessage<DeviceIdPayload>(DApiMsgTypes.GetDeviceInfo, payload));
        }

        public void RequestGetDevices() => Request(new DApiMessage(DApiMsgTypes.GetDevices));

        public void RequestGetVersion() => Request(new DApiMessage(DApiMsgTypes.GetVersion));

        public void RequestGetServerInfo() => Request(new DApiMessage(DApiMsgTypes.GetServerInfo));

        void IDisposable.Dispose()
        {
            if (webSocket != null)
            {
                webSocket.Close();
                webSocket = null; // Used to signal end of lifecycle and break from infinite loop in message loop
                GC.SuppressFinalize(this);
            }
        }
    }
}
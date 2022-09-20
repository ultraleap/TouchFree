using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Leap;
using Ultraleap.TouchFree.Library.Configuration;
using WebSocketSharp;

namespace Ultraleap.TouchFree.Library.Connections
{
    public class TrackingDiagnosticApi : ITrackingDiagnosticApi, IDisposable
    {
        private static string uri = "ws://127.0.0.1:1024/";
        private const string minimumMaskingAPIVerison = "2.1.0";
        private IConfigManager configManager;
        private string trackingServiceVersion;
        private Version version;

        public uint? connectedDeviceID;
        public string connectedDeviceFirmware;
        public string connectedDeviceSerial;
        public bool maskingAllowed; // TODO: Is this relevant anymore? Unused.
        
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
            private readonly Action<ConfigurationVariable<TData>> _onSet;
            private TData _value;
            public ConfigurationVariable(Action<ConfigurationVariable<TData>> onSet, TData initial = default)
            {
                _onSet = onSet;
                _value = initial;
            }

            public TData Value
            {
                get => _value;
                set
                {
                    Initialized = true;
                    if (Equals(value, _value)) return;
                    _value = value;
                    _onSet?.Invoke(this);
                }
            }

            // Variable is not considered initialized until it has been set externally from the constructor.
            // Scenarios this is expected to happen:
            // a. Set by an incoming request
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
            maskingData = new ConfigurationVariable<ImageMaskData>(_ => TrySaveTrackingConfiguration());
            allowImages = new ConfigurationVariable<bool>(_ => TrySaveTrackingConfiguration());
            cameraReversed = new ConfigurationVariable<bool>(_ => TrySaveTrackingConfiguration());
            analyticsEnabled = new ConfigurationVariable<bool>(_ => TrySaveTrackingConfiguration());

            Connect();
#pragma warning disable CS4014
            MessageQueueReader();
#pragma warning restore CS4014
            
            void SetTrackingConfigurationOnDevice(TrackingConfig config)
            {
                // Methods that require a connected device will be no-ops
                RequestSetAllowImages(config.AllowImages);
                RequestSetAnalyticsMode(config.AnalyticsEnabled);
                RequestSetCameraOrientation(config.CameraReversed);
                RequestSetImageMask(config.Mask.Left, config.Mask.Right, config.Mask.Upper, config.Mask.Lower);
            }
            void ControllerOnDevice(object sender, DeviceEventArgs e) => RequestGetDevices(); // Get devices response will update the connected device and refresh tracking config
            void ControllerOnDeviceLost(object sender, DeviceEventArgs e) => RequestGetDevices(); // Works even when no device is connected

            _configManager.OnTrackingConfigUpdated += SetTrackingConfigurationOnDevice;
            _trackingConnectionManager.Controller.Device += ControllerOnDevice;
            _trackingConnectionManager.Controller.DeviceLost += ControllerOnDeviceLost;

            void OnCameraOrientationResponse(Result<bool> cameraReversedResult)
            {
                // TODO: Do something with any errors?
                if (!cameraReversedResult.TryGetValue(out var value)) return;
                cameraReversed.Value = value;
            }

            void OnAllowImagesResponse(Result<bool> allowImagesResult)
            {
                // TODO: Do something with any errors?
                if (!allowImagesResult.TryGetValue(out var value)) return;
                allowImages.Value = value;
            }

            void OnAnalyticsResponse(Result<bool> analyticsEnabledResult)
            {
                // TODO: Do something with any errors?
                if (!analyticsEnabledResult.TryGetValue(out var value)) return;
                analyticsEnabled.Value = value;
            }

            void OnMaskingResponse(Result<ImageMaskData> maskDataResult)
            {
                // TODO: Do something with any errors?
                if (!maskDataResult.TryGetValue(out var maskData)) return;
                maskingData.Value = maskData;
            }

            this.OnMaskingResponse += OnMaskingResponse;
            this.OnAnalyticsResponse += OnAnalyticsResponse;
            this.OnAllowImagesResponse += OnAllowImagesResponse;
            this.OnCameraOrientationResponse += OnCameraOrientationResponse;
        }
        
        // Used for equality comparison
        private readonly record struct ConfigRecord(ImageMaskData Mask, bool AllowImages, bool CameraReversed,
            bool AnalyticsEnabled)
        {
            public ConfigRecord(TrackingConfig config, uint deviceId) : this(new ImageMaskData
            {
                device_id = deviceId, // Device id must be set as it takes part in equality comparison
                left = config.Mask.Left,
                lower = config.Mask.Lower,
                right = config.Mask.Right,
                upper = config.Mask.Upper
            }, config.AllowImages, config.CameraReversed, config.AnalyticsEnabled)
            {}
        }

        private void TrySaveTrackingConfiguration()
        {
            // We don't save the tracking config until all values are initialized to prevent situations such as defaults
            // always being written to config when a device hasn't been connected yet and there's no existing config
            if (maskingData.Initialized &&
                allowImages.Initialized &&
                cameraReversed.Initialized &&
                analyticsEnabled.Initialized)
            {
                var newConfig = new ConfigRecord(maskingData.Value, allowImages.Value, cameraReversed.Value, analyticsEnabled.Value);

                // Get old config from the manager to check if config has changed
                // Definitely changed if null - also avoid creating record if null as it will error
                var configChanged = configManager.TrackingConfig == null
                                    // We use new config device id so it doesn't impact overall equality 
                                    || new ConfigRecord(configManager.TrackingConfig, newConfig.Mask.device_id) != newConfig;
                
                // If nothing changed, we don't need to save - this is to avoid infinite looping caused by events
                if (configChanged)
                {
                    TrackingConfigFile.SaveConfig(new TrackingConfig
                    {
                        Mask =
                        {
                            Left = (float)maskingData.Value.left,
                            Right = (float)maskingData.Value.right,
                            Lower = (float)maskingData.Value.lower,
                            Upper = (float)maskingData.Value.upper
                        },
                        AllowImages = allowImages.Value,
                        AnalyticsEnabled = analyticsEnabled.Value,
                        CameraReversed = cameraReversed.Value
                    });
                }
            }
        }

        private async Task MessageQueueReader()
        {
            while (true)
            {
                await Task.Delay(10);
                if (newMessages.TryDequeue(out var message))
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
                    Console.WriteLine("DiagnosticAPI open... ");
                    RequestGetServerInfo();
                    RequestGetDevices();
                    RequestGetVersion();
                };
                webSocket.OnError += (sender, e) =>
                {
                    Console.WriteLine("DiagnosticAPI error! " + e.Message + "\n");
                };
                webSocket.OnClose += (sender, e) =>
                {
                    Console.WriteLine("DiagnosticAPI closed. " + e.Reason);
                };
            }

            try
            {
                webSocket.Connect();
            }
            catch (Exception ex)
            {
                Console.WriteLine("DiagnosticAPI connection exception... " + "\n" + ex.ToString());
            }
        }

        private void OnMessage(object sender, MessageEventArgs e)
        {
            if (e.IsText)
            {
                // + " " is used to avoid stack overflowing by creating a new string to
                // avoiding keeping around the event args object
                // TODO: Different method of avoiding stack overflow exceptions?
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
                        Console.WriteLine($"DiagnosticAPI - Payload for {status.ToString()} failed to deserialize: {_message}");   
                    }
                    else
                    {
                        onSuccess(payload.payload);
                    }
                }
                catch
                {
                    Console.WriteLine($"DiagnosticAPI - Could not parse {status.ToString()} data: {_message}");
                    onFailure?.Invoke();
                }
            }

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
                                    }
                                }
                            });
                        break;

                    case DApiMsgTypes.GetVersion:
                        Handle<string>(HandleDiagnosticAPIVersion);
                        break;
                    
                    case DApiMsgTypes.GetServerInfo:
                        Handle<ServiceInfoPayload>(payload =>
                        {
                            trackingServiceVersion = payload.server_version;
                            OnTrackingServerInfoResponse?.Invoke();
                        });
                        break;

                    case DApiMsgTypes.GetDeviceInfo:
                        Handle<DiagnosticDeviceInformation>(information =>
                        {
                            connectedDeviceFirmware = information.device_firmware;
                            connectedDeviceSerial = information.device_serial;
                            OnTrackingDeviceInfoResponse?.Invoke();
                        });
                        break;
                    
                    default:
                        Console.WriteLine("DiagnosticAPI - Could not parse response of type: " + response.type + " with message: " + _message);
                        break;
                }
            }
        }

        private void HandleDiagnosticAPIVersion(string _version)
        {
            Version curVersion = new Version(_version);
            Version minVersion = new Version(minimumMaskingAPIVerison);
            version = curVersion;

            // Versions above min version allow masking
            maskingAllowed = curVersion >= minVersion;
            OnTrackingApiVersionResponse?.Invoke();
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

        public void RequestGetAnalyticsMode() => Request(new DApiMessage(DApiMsgTypes.GetAnalyticsEnabled));

        public void RequestSetAnalyticsMode(bool enabled)
        {
            analyticsEnabled.Value = enabled;
            Request(new DApiPayloadMessage<bool>(DApiMsgTypes.SetAnalyticsEnabled, enabled));
        }

        public void RequestGetImageMask()
        {
            // Only send the request if we have a device
            if (!connectedDeviceID.HasValue) return;
            
            var payload = new DeviceIdPayload { device_id = connectedDeviceID.Value };
            Request(new DApiPayloadMessage<DeviceIdPayload>(DApiMsgTypes.GetImageMask, payload));
        }

        public void RequestSetImageMask(double _left, double _right, double _top, double _bottom)
        {
            var data = new ImageMaskData
            {
                left = _left,
                right = _right,
                upper = _top,
                lower = _bottom
            };
            
            // Only send the request if we have a device
            if (!connectedDeviceID.HasValue) return;

            data.device_id = connectedDeviceID.Value;
            Request(new DApiPayloadMessage<ImageMaskData>(DApiMsgTypes.SetImageMask, data));
        }
        
        public void RequestGetAllowImages() => Request(new DApiMessage(DApiMsgTypes.GetAllowImages));

        public void RequestSetAllowImages(bool enabled)
        {
            allowImages.Value = enabled;
            Request(new DApiPayloadMessage<bool>(DApiMsgTypes.SetAllowImages, enabled));
        }

        public void RequestGetCameraOrientation()
        {
            // Only send the request if we have a device
            if (!connectedDeviceID.HasValue) return;
            
            var payload = new DeviceIdPayload { device_id = connectedDeviceID.Value };
            Request(new DApiPayloadMessage<DeviceIdPayload>(DApiMsgTypes.GetCameraOrientation, payload));
        }

        public void RequestSetCameraOrientation(bool reverseOrientation)
        {
            cameraReversed.Value = reverseOrientation;
            
            // Only send the request if we have a device
            if (!connectedDeviceID.HasValue) return;
            
            var payload = new CameraOrientationPayload { device_id = connectedDeviceID.Value, camera_orientation = reverseOrientation ? "fixed-inverted" : "fixed-normal" };
            Request(new DApiPayloadMessage<CameraOrientationPayload>(DApiMsgTypes.SetCameraOrientation, payload));
        }

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
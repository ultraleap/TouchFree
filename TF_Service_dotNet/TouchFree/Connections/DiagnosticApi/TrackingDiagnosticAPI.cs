using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Leap;
using Newtonsoft.Json;
using Ultraleap.TouchFree.Library.Configuration;
using Websocket.Client;

namespace Ultraleap.TouchFree.Library.Connections.DiagnosticApi;

public class TrackingDiagnosticApi : ITrackingDiagnosticApi, IDisposable
{
    private static readonly Uri url = new("ws://127.0.0.1:1024/");
    private readonly IConfigManager configManager;
    private Version version;

    public string trackingServiceVersion { get; private set; }

    private uint? connectedDeviceID;
    public string connectedDeviceFirmware { get; private set; }
    public string connectedDeviceSerial { get; private set; }
    public async Task<DiagnosticData> RequestGetAll(GetAllInfo request)
    {
        await EnsureConnection();
        if (request.GetMasking)
        {
            maskingData.RequestGet();
        }

        if (request.GetAllowImage)
        {
            allowImages.RequestGet();
        }

        if (request.GetOrientation)
        {
            cameraReversed.RequestGet();
        }

        if (request.GetAnalytics)
        {
            analyticsEnabled.RequestGet();
        }

        var data = new DiagnosticData(
            await maskingData.GetValueAsync(),
            await allowImages.GetValueAsync(),
            await cameraReversed.GetValueAsync(),
            await analyticsEnabled.GetValueAsync());

        return data;
    }

    public async Task RequestSetAll(DiagnosticData data)
    {
        await EnsureConnection();
        if (data.Masking.HasValue)
        {
            maskingData.RequestSet(data.Masking.Value);
        }

        if (data.AllowImages.HasValue)
        {
            allowImages.RequestSet(data.AllowImages.Value);
        }

        if (data.CameraOrientation.HasValue)
        {
            cameraReversed.RequestSet(data.CameraOrientation.Value);
        }

        if (data.Analytics.HasValue)
        {
            analyticsEnabled.RequestSet(data.Analytics.Value);
        }

        // Wait for all requests to complete
        await maskingData.GetValueAsync();
        await allowImages.GetValueAsync();
        await cameraReversed.GetValueAsync();
        await analyticsEnabled.GetValueAsync();
    }

    public event Action<Result<MaskingData>> OnMaskingResponse;
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
        private bool _expectingValueUpdate = false;

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

        public async Task<TData> GetValueAsync()
        {
            var totalWaited = 0;
            while(_expectingValueUpdate)
            {
                await Task.Delay(100);
                totalWaited++;

                // 20 seconds
                if (totalWaited >= 200)
                {
                    throw new Exception("No response received after 20 seconds. Timing out.");
                }
            }

            return Value;
        }

        public void RequestGet() {
            _expectingValueUpdate = true;
            _diagnosticApi.SendIfConnected(_getPayloadFunc(_diagnosticApi.connectedDeviceID));
        }

        public void RequestSet(TData value)
        {
            _expectingValueUpdate = true;
            Value = value;
            if (_connectedDeviceRequired && !_diagnosticApi.connectedDeviceID.HasValue) return; // Only send the request if we have a device, when one is required

            lock (_responsesToIgnore)
            {
                _responsesToIgnore.Add(value);
                _diagnosticApi.SendIfConnected(_setPayloadFunc(_value, _diagnosticApi.connectedDeviceID));
            }
        }

        public void HandleResponse(Result<TData> result)
        {
            // TODO: Do something with any errors?
            if (!result.TryGetValue(out var val)) return;
            lock (_responsesToIgnore)
            {
                var found = _responsesToIgnore.Remove(val);
                _expectingValueUpdate = false;
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

    private readonly ConfigurationVariable<MaskingData> maskingData;
    private readonly ConfigurationVariable<bool> allowImages;
    private readonly ConfigurationVariable<bool> cameraReversed;
    private readonly ConfigurationVariable<bool> analyticsEnabled;

    private WebsocketClient webSocket;
    private readonly ConcurrentQueue<string> newMessages = new();
        
    private bool IsConnected => webSocket is { IsStarted: true, NativeClient.State: WebSocketState.Connecting or WebSocketState.Open };

    private void SetupWebsocketSubscriptions()
    {
        webSocket.ReconnectionHappened.Subscribe(info =>
        {
            TouchFreeLog.WriteLine($"DiagnosticAPI connected - (re)connection type '{info.Type}'");
            RequestGetServerInfo();
            RequestGetDevices();
            RequestGetVersion();
        });
        webSocket.DisconnectionHappened.Subscribe(info =>
        {
            trackingServiceVersion = null;
            TouchFreeLog.ErrorWriteLine($"DiagnosticAPI disconnected - disconnection type '{info.Type}'");
            if (info.Exception != null)
            {
                TouchFreeLog.ErrorWriteLine($"Disconnection caused by exception: {info.Exception}");
            }

            if (info.CloseStatus.HasValue)
            {
                TouchFreeLog.ErrorWriteLine($"Disconnection close status '{info.CloseStatus.Value}': {info.CloseStatusDescription}");
            }
        });
        webSocket.MessageReceived.Subscribe(info =>
        {
            switch (info.MessageType)
            {
                case WebSocketMessageType.Text:
                    newMessages.Enqueue(info.Text);
                    break;
                case WebSocketMessageType.Binary:
                    // We don't care about binary messages here - API sends them for image data so we must ignore them
                    break;
                case WebSocketMessageType.Close:
                    // No-op
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        });
    }

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
        maskingData = new ConfigurationVariable<MaskingData>(this, true,
            DeviceIdPayloadFunc(DApiMsgTypes.GetImageMask),
            (maskData, deviceId) =>
            {
                var payload = (ImageMaskData)maskData with { device_id = deviceId.GetValueOrDefault() };
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
            
        webSocket = new WebsocketClient(url);
        SetupWebsocketSubscriptions();

        webSocket.Start();
#pragma warning disable CS4014
        MessageQueueReader();
#pragma warning restore CS4014

        void SetTrackingConfigurationOnDevice(TrackingConfig config)
        {
            allowImages.RequestSet(config.AllowImages);
            analyticsEnabled.RequestSet(config.AnalyticsEnabled);
            cameraReversed.RequestSet(config.CameraReversed);
            maskingData.RequestSet((MaskingData)config.Mask);
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
            await Task.Delay(10); // TODO: Use threading timer instead of loop with delay to reduce allocations
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
                    Handle<ImageMaskData>(payload => OnMaskingResponse?.Invoke((MaskingData)payload),
                        () => OnMaskingResponse?.Invoke(new Error($"Could not access Masking data. Tracking Response: \"{_message}\"")));
                    break;

                case DApiMsgTypes.GetAnalyticsEnabled:
                case DApiMsgTypes.SetAnalyticsEnabled:
                    Handle<bool>(payload => OnAnalyticsResponse?.Invoke(payload),
                        () => OnAnalyticsResponse?.Invoke(new Error($"Could not access Analytics state. Tracking Response: \"{_message}\"")));
                    break;

                case DApiMsgTypes.SetAllowImages:
                case DApiMsgTypes.GetAllowImages:
                    Handle<bool>(payload => OnAllowImagesResponse?.Invoke(payload),
                        () => OnAllowImagesResponse?.Invoke(new Error($"Could not access AllowImages state. Tracking Response: \"{_message}\"")));
                    break;

                case DApiMsgTypes.GetCameraOrientation:
                case DApiMsgTypes.SetCameraOrientation:
                    Handle<CameraOrientationPayload>(payload => OnCameraOrientationResponse?.Invoke(payload.camera_orientation == "fixed-inverted"),
                        () => OnCameraOrientationResponse?.Invoke(new Error($"Could not access CameraOrientation state. Tracking Response: \"{_message}\"")));
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
                            else
                            {
                                connectedDeviceFirmware = null;
                                connectedDeviceSerial = null;
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
                                    maskingData.Match(RequestSetImageMask, RequestGetImageMask);
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

    private static readonly SemaphoreSlim reconnectSemaphore = new SemaphoreSlim(1, 1);
    private bool _reconnecting;

    private async Task EnsureConnection()
    {
        // Locking to prevent race condition causing multiple Reconnect calls
        // if EnsureConnection is called at the same time from multiple threads
        try
        {
            await reconnectSemaphore.WaitAsync();
            if (IsConnected || _reconnecting) return;
            _reconnecting = true;
        
            await webSocket.Reconnect();
        }
        finally
        {
            _reconnecting = false;
            reconnectSemaphore.Release();
        }
    }

    private bool SendIfConnected(object payload)
    {
        if (!IsConnected && !_reconnecting) return false;
        var requestMessage = JsonConvert.SerializeObject(payload);
        webSocket.Send(requestMessage);
        return true;
    }

    public void RequestGetAnalyticsMode() => analyticsEnabled.RequestGet();
    public void RequestSetAnalyticsMode(bool enabled) => analyticsEnabled.RequestSet(enabled);
    
    public void RequestGetImageMask() => maskingData.RequestGet();
    public void RequestSetImageMask(MaskingData data) => maskingData.RequestSet(data);
    
    public void RequestGetAllowImages() => allowImages.RequestGet();
    public void RequestSetAllowImages(bool enabled) => allowImages.RequestSet(enabled);

    public void RequestGetCameraOrientation() => cameraReversed.RequestGet();
    public void RequestSetCameraOrientation(bool reverseOrientation) => cameraReversed.RequestSet(reverseOrientation);

    public bool RequestGetDeviceInfo() =>
        // Only send the request if we have a device
        connectedDeviceID.HasValue
        && SendIfConnected(new DApiPayloadMessage<DeviceIdPayload>(DApiMsgTypes.GetDeviceInfo, new DeviceIdPayload { device_id = connectedDeviceID.Value }));

    public void RequestGetDevices() => SendIfConnected(new DApiMessage(DApiMsgTypes.GetDevices));

    public void RequestGetVersion() => SendIfConnected(new DApiMessage(DApiMsgTypes.GetVersion));

    public void RequestGetServerInfo() => SendIfConnected(new DApiMessage(DApiMsgTypes.GetServerInfo));

    void IDisposable.Dispose()
    {
        if (webSocket != null)
        {
            webSocket.Dispose();
            webSocket = null; // Used to signal end of lifecycle and break from infinite loop in message loop
            GC.SuppressFinalize(this);
        }
    }
}
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
    public event Action<DeviceInfo> DeviceConnected;
    public event Action<DeviceInfo> DeviceDisconnected;
    public ApiInfo? ApiInfo { get; private set; }
    public DeviceInfo? ConnectedDevice { get; private set; }

    public async Task<DiagnosticData> RequestGet() => 
        new(await _maskingData.RequestGet(),
            await _allowImages.RequestGet(),
            await _cameraReversed.RequestGet(),
            await _analyticsEnabled.RequestGet());

    public async Task RequestSet(DiagnosticData data)
    {
        try
        {
            await _maskingData.RequestSet(data.Masking);
            await _allowImages.RequestSet(data.AllowImages);
            await _cameraReversed.RequestSet(data.CameraOrientation);
            await _analyticsEnabled.RequestSet(data.Analytics);
        }
        catch (OperationCanceledException canceledException)
        {
            TouchFreeLog.WriteLine($"DiagnosticAPI Set Request canceled: {canceledException.Message}");
        }
    }
    
    private delegate object GetPayloadFunc(DeviceInfo? connectedDevice);
    // Represents a variable that will be "initialized" once set at least once after construction
    private class ConfigurationVariable<TData> where TData : struct
    {
        public delegate object SetPayloadFunc(TData data, DeviceInfo? connectedDevice);
        
        private readonly TrackingDiagnosticApi _diagnosticApi;
        private readonly bool _connectedDeviceRequired;
        private readonly GetPayloadFunc _getPayloadFunc;
        private readonly SetPayloadFunc _setPayloadFunc;
        private readonly List<TaskCompletionSource<TData>> _tasks;
        private TData _value;

        public ConfigurationVariable(
            TrackingDiagnosticApi diagnosticApi,
            bool connectedDeviceRequired,
            GetPayloadFunc getPayloadFunc,
            SetPayloadFunc setPayloadFunc,
            TData initial = default)
        {
            _diagnosticApi = diagnosticApi;
            _connectedDeviceRequired = connectedDeviceRequired;
            _getPayloadFunc = getPayloadFunc;
            _setPayloadFunc = setPayloadFunc;
            _value = initial;
            _tasks = new List<TaskCompletionSource<TData>>();
        }

        public TData Value
        {
            get => _value;
            private set
            {
                Initialized = true;
                _value = value;
                _diagnosticApi.SaveConfigIfNoneExistsAndAllVariablesAreInitialized();
            }
        }

        public Task<TData> RequestGet()
        {
            var payload = _getPayloadFunc(_diagnosticApi.ConnectedDevice);
            if (!_diagnosticApi.SendIfConnected(payload)) return Task.FromResult(default(TData)); // Not connected, cannot get
            lock (_tasks)
            {
                var task = new TaskCompletionSource<TData>();
                _tasks.Add(task);
                return task.Task;
            }
        }

        public Task RequestSet(TData? value)
        {
            if (!value.HasValue) return Task.CompletedTask;
            Value = value.Value;
            if (_connectedDeviceRequired && !_diagnosticApi.ConnectedDevice.HasValue) return Task.CompletedTask; // Only send the request if we have a device, when one is required
            var payload = _setPayloadFunc(_value, _diagnosticApi.ConnectedDevice);
            if (!_diagnosticApi.SendIfConnected(payload)) return Task.CompletedTask; // Couldn't send to api, will be sent next connection
            lock (_tasks)
            {
                var task = new TaskCompletionSource<TData>();
                _tasks.Add(task);
                return task.Task;
            }
        }

        public void HandleResponse(TData val)
        {
            Value = val;
            lock (_tasks)
            {
                foreach (var t in _tasks)
                {
                    t.TrySetResult(val);
                }

                _tasks.Clear();
            }
        }

        public void CancelSendTasks()
        {
            lock (_tasks)
            {
                foreach (var t in _tasks)
                {
                    t.TrySetCanceled();
                }

                _tasks.Clear();
            }
        }

        // Variable is not considered initialized until it has been set externally from the constructor.
        // Scenarios this is expected to happen:
        // a. Set by a request
        // b. Loaded from an existing config
        // c. Loaded from connected device if not initialized before connecting
        public bool Initialized { get; private set; }

        public void DeviceConnected()
        {
            if (Initialized) RequestSet(_value);
            else RequestGet();
        }
    }
    
    private readonly ConfigurationVariable<MaskingData> _maskingData;
    private readonly ConfigurationVariable<bool> _allowImages;
    private readonly ConfigurationVariable<bool> _cameraReversed;
    private readonly ConfigurationVariable<bool> _analyticsEnabled;

    private static readonly Uri _url = new("ws://127.0.0.1:1024/");
    private readonly IConfigManager _configManager;
    private readonly WebsocketClient _webSocket;
    private readonly Timer _messageQueueTimer;
    private readonly ConcurrentQueue<string> _readMessageQueue = new();
        
    private bool IsConnected => _webSocket is { IsStarted: true, IsRunning: true };
    
    private void SaveConfigIfNoneExistsAndAllVariablesAreInitialized()
    {
        // Make sure to update below if adding another configuration variable!
        if (_configManager.TrackingConfig == null &&
            _maskingData.Initialized &&
            _allowImages.Initialized &&
            _cameraReversed.Initialized &&
            _analyticsEnabled.Initialized &&
            !TrackingConfigFile.DoesConfigFileExist())
        {
            var trackingConfigurationToStore = new TrackingConfig
            {
                AllowImages = _allowImages.Value,
                AnalyticsEnabled = _analyticsEnabled.Value,
                CameraReversed = _cameraReversed.Value,
                Mask = (Configuration.MaskingData)_maskingData.Value
            };

            _configManager.TrackingConfig = trackingConfigurationToStore;
            TrackingConfigFile.SaveConfig(trackingConfigurationToStore);
        }
    }

    private void SetupWebsocketSubscriptions()
    {
        _webSocket.ReconnectionHappened.Subscribe(info =>
        {
            TouchFreeLog.WriteLine($"DiagnosticAPI connected - (re)connection type '{info.Type}'");
            SendIfConnected(new DApiMessage(DApiMsgTypes.GetServerInfo));
            SendIfConnected(new DApiMessage(DApiMsgTypes.GetVersion));
            RefreshConnectedDevice();
        });
        _webSocket.DisconnectionHappened.Subscribe(info =>
        {
            ApiInfo = null;
            DeviceChanged(null);
            
            _allowImages.CancelSendTasks();
            _analyticsEnabled.CancelSendTasks();
            _cameraReversed.CancelSendTasks();
            _maskingData.CancelSendTasks();
                
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
        _webSocket.MessageReceived.Subscribe(info =>
        {
            switch (info.MessageType)
            {
                case WebSocketMessageType.Text:
                    _readMessageQueue.Enqueue(info.Text);
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

    public TrackingDiagnosticApi(IConfigManager configManager, ITrackingConnectionManager trackingConnectionManager)
    {
        _configManager = configManager;

        // Get Payload functions commonly used between multiple configuration variables
        GetPayloadFunc DefaultGetPayloadFunc(DApiMsgTypes requestType) =>
            _ => new DApiMessage(requestType);
        GetPayloadFunc DeviceIdPayloadFunc(DApiMsgTypes requestType) =>
            info => new DApiPayloadMessage<DeviceIdPayload>(requestType, new DeviceIdPayload { device_id = info.GetValueOrDefault().DeviceId });
        
        // Set Payload functions commonly used between multiple configuration variables
        ConfigurationVariable<T>.SetPayloadFunc DefaultSetPayloadFunc<T>(DApiMsgTypes requestType) where T : struct =>
            (val, _) => new DApiPayloadMessage<T>(requestType, val);

        // Configuration variable setup
        _maskingData = new ConfigurationVariable<MaskingData>(this, true,
            DeviceIdPayloadFunc(DApiMsgTypes.GetImageMask),
            (maskData, deviceInfo) =>
            {
                var payload = (ImageMaskData)maskData with { device_id = deviceInfo.GetValueOrDefault().DeviceId };
                return new DApiPayloadMessage<ImageMaskData>(DApiMsgTypes.SetImageMask, payload);
            });

        _allowImages = new ConfigurationVariable<bool>(this, false,
            DefaultGetPayloadFunc(DApiMsgTypes.GetAllowImages),
            DefaultSetPayloadFunc<bool>(DApiMsgTypes.SetAllowImages));

        _cameraReversed = new ConfigurationVariable<bool>(this, true,
            DeviceIdPayloadFunc(DApiMsgTypes.GetCameraOrientation),
            (reversed, deviceInfo) =>
            {
                var payload = new CameraOrientationPayload
                {
                    device_id = deviceInfo.GetValueOrDefault().DeviceId,
                    camera_orientation = reversed ? "fixed-inverted" : "fixed-normal"
                };
                return new DApiPayloadMessage<CameraOrientationPayload>(DApiMsgTypes.SetCameraOrientation, payload);
            });

        _analyticsEnabled = new ConfigurationVariable<bool>(this, false,
            DefaultGetPayloadFunc(DApiMsgTypes.GetAnalyticsEnabled),
            DefaultSetPayloadFunc<bool>(DApiMsgTypes.SetAnalyticsEnabled));
            
        _webSocket = new WebsocketClient(_url)
        {
            ReconnectTimeout = new TimeSpan(0, 1, 0),
            ErrorReconnectTimeout = new TimeSpan(0, 0, 10),
        };
        SetupWebsocketSubscriptions();
        
        // Setup event on timer - this will start the thread callbacks immediately
        _messageQueueTimer = new Timer(_ =>
        {
            // Read all messages incoming from the diagnostic api
            while (_readMessageQueue.TryDequeue(out var message))
            {
                HandleMessage(message);
            }

            // Queue next event after handling existing messages.
            // This prevents issues with multiple callbacks overlapping.
            _messageQueueTimer!.Change(TimeSpan.FromMilliseconds(10), Timeout.InfiniteTimeSpan);
        }, null, TimeSpan.FromMilliseconds(10), Timeout.InfiniteTimeSpan);

        _webSocket.Start();

#pragma warning disable CS4014
        configManager.OnTrackingConfigUpdated += config => RequestSet((DiagnosticData)config);
#pragma warning restore CS4014
        
        // Both of these do a GetDevices request to force refresh the diagnostic API device information
        // and perform any action for a newly device connected such as applying existing config or
        // retrieving uninitialized variables from the device
        void RefreshDeviceInfo(object sender, DeviceEventArgs e) => RefreshConnectedDevice();
        trackingConnectionManager.Controller.Device += RefreshDeviceInfo;
        trackingConnectionManager.Controller.DeviceLost += RefreshDeviceInfo;

        DeviceConnected += _ =>
        {
            _allowImages.DeviceConnected();
            _analyticsEnabled.DeviceConnected();
            _cameraReversed.DeviceConnected();
            _maskingData.DeviceConnected();
        };
    }

    private void HandleMessage(string message)
    {
        var response = JsonConvert.DeserializeObject<DApiMessage>(message);

        var parsed = Enum.TryParse(response.type, out DApiMsgTypes status);

        void Handle<TPayload>(Action<TPayload> onSuccess)
        {
            var payload = JsonConvert.DeserializeObject<DApiPayloadMessage<TPayload>>(message);
            if (payload == null)
            {
                TouchFreeLog.WriteLine($"DiagnosticAPI - Payload for {status.ToString()} failed to deserialize: {message}");
            }
            else
            {
                onSuccess(payload.payload);
            }
        }

        if (!parsed)
        {
            TouchFreeLog.WriteLine($"DiagnosticAPI - Could not parse response of type: {response.type} with message: {message}");
        }
        else
        {
            switch (status)
            {
                case DApiMsgTypes.GetImageMask:
                case DApiMsgTypes.SetImageMask:
                    Handle<ImageMaskData>(payload => _maskingData.HandleResponse((MaskingData)payload));
                    break;

                case DApiMsgTypes.GetAnalyticsEnabled:
                case DApiMsgTypes.SetAnalyticsEnabled:
                    Handle<bool>(_analyticsEnabled.HandleResponse);
                    break;

                case DApiMsgTypes.SetAllowImages:
                case DApiMsgTypes.GetAllowImages:
                    Handle<bool>(_allowImages.HandleResponse);
                    break;

                case DApiMsgTypes.GetCameraOrientation:
                case DApiMsgTypes.SetCameraOrientation:
                    Handle<CameraOrientationPayload>(payload => _cameraReversed.HandleResponse(payload.camera_orientation == "fixed-inverted"));
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

                            if (ConnectedDevice?.DeviceId != newConnectedDeviceId)
                            {
                                // Connection event
                                if (newConnectedDeviceId != null)
                                {
                                    // Get the rest of the info about the newly connected device
                                    // We wait for the response with full device information before firing a device
                                    // connected event or updating ConnectedDevice
                                    SendIfConnected(new DApiPayloadMessage<DeviceIdPayload>(DApiMsgTypes.GetDeviceInfo,
                                        new DeviceIdPayload { device_id = newConnectedDeviceId.Value }));
                                }
                                else // Disconnection event
                                {
                                    DeviceChanged(null);
                                }
                            }
                        });
                    break;

                case DApiMsgTypes.GetVersion:
                    Handle<string>(version =>
                        ApiInfo = ApiInfo.GetValueOrDefault() with
                        {
                            ProtocolVersion = version
                        });
                    break;

                case DApiMsgTypes.GetServerInfo:
                    Handle<ServiceInfoPayload>(info =>
                        ApiInfo = ApiInfo.GetValueOrDefault() with
                        {
                            ServiceVersion = info.server_version
                        });
                    break;

                case DApiMsgTypes.GetDeviceInfo:
                    Handle<DiagnosticDeviceInformation>(information => DeviceChanged(information));
                    break;

                default:
                    TouchFreeLog.WriteLine($"DiagnosticAPI - Could not parse response of type: {response.type} with message: {message}");
                    break;
            }
        }
    }

    private bool SendIfConnected(object payload)
    {
        if (!IsConnected) return false;
        var requestMessage = JsonConvert.SerializeObject(payload);
        _webSocket.Send(requestMessage);
        return true;
    }

    private void RefreshConnectedDevice() => SendIfConnected(new DApiMessage(DApiMsgTypes.GetDevices));

    private void DeviceChanged(DiagnosticDeviceInformation? newDevice)
    {
        // Fire a disconnection event for the previous device if there was one
        if (ConnectedDevice.HasValue)
        {
            DeviceDisconnected?.Invoke(ConnectedDevice.Value);
        }

        if (newDevice.HasValue)
        {
            ConnectedDevice = new DeviceInfo(newDevice.Value.device_id,
                newDevice.Value.device_firmware,
                newDevice.Value.device_serial,
                newDevice.Value.device_hardware);
            DeviceConnected?.Invoke(ConnectedDevice.Value);
        }
        else ConnectedDevice = null;
    }

    void IDisposable.Dispose()
    {
        if (_webSocket != null)
        {
            _webSocket.Dispose();
            _messageQueueTimer.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
using Leap;
using System;
using System.Threading.Tasks;
using Ultraleap.TouchFree.Library.Configuration;

namespace Ultraleap.TouchFree.Library.Connections;

public class TrackingConnectionManager : ITrackingConnectionManager
{
    public IController Controller { get; }

    private readonly IConfigManager _configManager;

    private const int _maximumWaitTimeSeconds = 30;
    private const int _initialWaitTimeSeconds = 1;
    private bool _shouldConnect = false;

    public bool ShouldSendHandData { get; private set; }
    public TrackingMode CurrentTrackingMode { get; private set; }

    public TrackingServiceState TrackingServiceState =>
        (Controller.IsServiceConnected, Controller.IsConnected) switch
        {
            (true, true) => TrackingServiceState.CONNECTED,
            (true, false) => TrackingServiceState.NO_CAMERA,
            // (false, true) => ???, Weird state that's currently impossible if you inspect the implementation of IsConnected
            _ => TrackingServiceState.UNAVAILABLE
        };

    public event Action<TrackingServiceState> ServiceStatusChange;

    // ReSharper disable once UnusedMember.Global
    public TrackingConnectionManager(IConfigManager configManager) : this(configManager, new Controller()) { }

    // ReSharper disable once MemberCanBePrivate.Global
    public TrackingConnectionManager(IConfigManager configManager, IController controller)
    {
        _configManager = configManager;
        Controller = controller;
        Controller.Connect += ControllerOnConnect;
        Controller.Disconnect += ControllerOnDisconnect;
        Controller.Device += ControllerOnDevice;
        Controller.DeviceLost += ControllerOnDeviceLost;
        UpdateTrackingMode(configManager.PhysicalConfig);
        configManager.OnPhysicalConfigUpdated += UpdateTrackingMode;
        Controller.StopConnection();
    }

    private void ControllerOnDevice(object sender, DeviceEventArgs e)
    {
        // More than 1 device connected now, ignore this event as we only care about at least one device connection
        if (Controller.Devices.Count > 1) return;
        ServiceStatusChange?.Invoke(TrackingServiceState.CONNECTED);
    }

    private void ControllerOnDeviceLost(object sender, DeviceEventArgs e)
    {
        // We still have at least one device, ignore this event as we only care about at least one device connection
        if (Controller.Devices.Count >= 1) return;
        ServiceStatusChange?.Invoke(TrackingServiceState.NO_CAMERA);
    }

    public void Connect()
    {
        _shouldConnect = true;
        CheckConnectionAndRetryOnFailure();
    }

    public void Disconnect()
    {
        _shouldConnect = false;
        if (Controller.IsServiceConnected)
        {
            Controller.StopConnection();
        }
    }

    private void ControllerOnConnect(object sender, ConnectionEventArgs e)
    {

        UpdateTrackingMode(_configManager.PhysicalConfig);
        ServiceStatusChange?.Invoke(TrackingServiceState.NO_CAMERA);

        CheckTrackingModeIsCorrectAfterDelay();
    }

    private async void CheckTrackingModeIsCorrectAfterDelay()
    {
        await Task.Delay(5000);
        if (Controller.IsServiceConnected)
        {
            var trackingMode = GetTrackingModeFromConfig(_configManager.PhysicalConfig);

            var inScreenTop = Controller.IsPolicySet(Leap.Controller.PolicyFlag.POLICY_OPTIMIZE_SCREENTOP);
            var inHmd = Controller.IsPolicySet(Leap.Controller.PolicyFlag.POLICY_OPTIMIZE_HMD);

            if (TrackingModeIsIncorrect(trackingMode, inScreenTop, inHmd))
            {
                UpdateTrackingMode(_configManager.PhysicalConfig);
            }
        }
    }

    public static bool TrackingModeIsIncorrect(TrackingMode trackingMode, bool inScreenTop, bool inHmd) =>
        (trackingMode == TrackingMode.SCREENTOP && !inScreenTop)
        || (trackingMode == TrackingMode.HMD && !inHmd)
        || (trackingMode == TrackingMode.DESKTOP && (inScreenTop || inHmd));

    private void ControllerOnDisconnect(object sender, Leap.ConnectionLostEventArgs e)
    {
        ServiceStatusChange?.Invoke(TrackingServiceState.UNAVAILABLE);
        if (_shouldConnect)
        {
            CheckConnectionAndRetryOnFailure(true);
        }
    }

    private async void CheckConnectionAndRetryOnFailure(bool includeInitialDelay = false)
    {
        var waitTimeSeconds = _initialWaitTimeSeconds;

        if (includeInitialDelay)
        {
            await Task.Delay(1000);
            waitTimeSeconds = IncreaseWaitTimeSeconds(waitTimeSeconds);
        }

        while (!Controller.IsServiceConnected && _shouldConnect)
        {
            Controller.StartConnection();

            await Task.Delay(1000 * waitTimeSeconds);
            waitTimeSeconds = IncreaseWaitTimeSeconds(waitTimeSeconds);
        }
    }

    private static int IncreaseWaitTimeSeconds(int currentWaitTime)
    {
        if (currentWaitTime == _maximumWaitTimeSeconds)
        {
            return currentWaitTime;
        }

        var updatedWaitTime = currentWaitTime * 2;
        return updatedWaitTime > _maximumWaitTimeSeconds ? _maximumWaitTimeSeconds : updatedWaitTime;
    }

    private void UpdateTrackingMode(PhysicalConfigInternal config) => SetTrackingMode(GetTrackingModeFromConfig(config));

    private static TrackingMode GetTrackingModeFromConfig(PhysicalConfigInternal config) =>
        Math.Abs(config.LeapRotationD.Z) switch
        {
            // leap is looking down
            > 90f when config.LeapRotationD.X <= 0f => TrackingMode.SCREENTOP,
            > 90f => TrackingMode.HMD,
            _ => TrackingMode.DESKTOP
        };

    private void SetTrackingMode(TrackingMode trackingMode)
    {
        TouchFreeLog.WriteLine($"Requesting {trackingMode} tracking mode");

        CurrentTrackingMode = trackingMode;

        switch (trackingMode)
        {
            case TrackingMode.DESKTOP:
                Controller.ClearPolicy(Leap.Controller.PolicyFlag.POLICY_OPTIMIZE_SCREENTOP);
                Controller.ClearPolicy(Leap.Controller.PolicyFlag.POLICY_OPTIMIZE_HMD);
                break;
            case TrackingMode.HMD:
                Controller.ClearPolicy(Leap.Controller.PolicyFlag.POLICY_OPTIMIZE_SCREENTOP);
                Controller.SetPolicy(Leap.Controller.PolicyFlag.POLICY_OPTIMIZE_HMD);
                break;
            case TrackingMode.SCREENTOP:
                Controller.SetPolicy(Leap.Controller.PolicyFlag.POLICY_OPTIMIZE_SCREENTOP);
                Controller.ClearPolicy(Leap.Controller.PolicyFlag.POLICY_OPTIMIZE_HMD);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(trackingMode), trackingMode, null);
        }
    }

    public void SetImagesState(bool enabled)
    {
        ShouldSendHandData = enabled;
        if (enabled)
        {
            Controller.SetPolicy(Leap.Controller.PolicyFlag.POLICY_IMAGES);
        }
        else
        {
            Controller.ClearPolicy(Leap.Controller.PolicyFlag.POLICY_IMAGES);
        }
    }
}
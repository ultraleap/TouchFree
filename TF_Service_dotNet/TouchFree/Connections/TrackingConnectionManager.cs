using Leap;
using System;
using System.Threading.Tasks;
using Ultraleap.TouchFree.Library.Configuration;

namespace Ultraleap.TouchFree.Library.Connections
{
    public class TrackingConnectionManager : ITrackingConnectionManager
    {
        public IController Controller { get; }

        private readonly IConfigManager configManager;

        private const int maximumWaitTimeSeconds = 30;
        private const int initialWaitTimeSeconds = 1;
        private bool ShouldConnect = false;

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

        public TrackingConnectionManager(IConfigManager _configManager) : this(_configManager, new Controller()) { }

        public TrackingConnectionManager(IConfigManager _configManager, IController _controller)
        {
            configManager = _configManager;
            Controller = _controller;
            Controller.Connect += ControllerOnConnect;
            Controller.Disconnect += ControllerOnDisconnect;
            Controller.Device += ControllerOnDevice;
            Controller.DeviceLost += ControllerOnDeviceLost;
            UpdateTrackingMode(_configManager.PhysicalConfig);
            _configManager.OnPhysicalConfigUpdated += UpdateTrackingMode;
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
            ShouldConnect = true;
            CheckConnectionAndRetryOnFailure();
        }

        public void Disconnect()
        {
            ShouldConnect = false;
            if (Controller.IsServiceConnected)
            {
                Controller.StopConnection();
            }
        }

        private void ControllerOnConnect(object sender, ConnectionEventArgs e)
        {

            UpdateTrackingMode(configManager.PhysicalConfig);
            ServiceStatusChange?.Invoke(TrackingServiceState.NO_CAMERA);

            CheckTrackingModeIsCorrectAfterDelay();
        }

        private async void CheckTrackingModeIsCorrectAfterDelay()
        {
            await Task.Delay(5000);
            if (Controller.IsServiceConnected)
            {
                var trackingMode = GetTrackingModeFromConfig(configManager.PhysicalConfig);

                var inScreenTop = Controller.IsPolicySet(Leap.Controller.PolicyFlag.POLICY_OPTIMIZE_SCREENTOP);
                var inHmd = Controller.IsPolicySet(Leap.Controller.PolicyFlag.POLICY_OPTIMIZE_HMD);

                if (TrackingModeIsIncorrect(trackingMode, inScreenTop, inHmd))
                {
                    UpdateTrackingMode(configManager.PhysicalConfig);
                }
            }
        }

        public static bool TrackingModeIsIncorrect(TrackingMode trackingMode, bool inScreenTop, bool inHmd)
        {
            return (trackingMode == TrackingMode.SCREENTOP && !inScreenTop) ||
                (trackingMode == TrackingMode.HMD && !inHmd) ||
                (trackingMode == TrackingMode.DESKTOP && (inScreenTop || inHmd));
        }

        private void ControllerOnDisconnect(object sender, Leap.ConnectionLostEventArgs e)
        {
            ServiceStatusChange?.Invoke(TrackingServiceState.UNAVAILABLE);
            if (ShouldConnect)
            {
                CheckConnectionAndRetryOnFailure(true);
            }
        }

        private async void CheckConnectionAndRetryOnFailure(bool includeInitialDelay = false)
        {
            var waitTimeSeconds = initialWaitTimeSeconds;

            if (includeInitialDelay)
            {
                await Task.Delay(1000);
                waitTimeSeconds = IncreaseWaitTimeSeconds(waitTimeSeconds);
            }

            while (!Controller.IsServiceConnected && ShouldConnect)
            {
                Controller.StartConnection();

                await Task.Delay(1000 * waitTimeSeconds);
                waitTimeSeconds = IncreaseWaitTimeSeconds(waitTimeSeconds);
            }
        }

        private static int IncreaseWaitTimeSeconds(int currentWaitTime)
        {
            if (currentWaitTime == maximumWaitTimeSeconds)
            {
                return currentWaitTime;
            }

            var updatedWaitTime = currentWaitTime * 2;
            return updatedWaitTime > maximumWaitTimeSeconds ? maximumWaitTimeSeconds : updatedWaitTime;
        }

        private void UpdateTrackingMode(PhysicalConfigInternal _config)
        {
            SetTrackingMode(GetTrackingModeFromConfig(_config));
        }

        private static TrackingMode GetTrackingModeFromConfig(PhysicalConfigInternal _config)
        {
            // leap is looking down
            if (Math.Abs(_config.LeapRotationD.Z) > 90f)
            {
                if (_config.LeapRotationD.X <= 0f)
                {
                    return TrackingMode.SCREENTOP;
                }
                else
                {
                    return TrackingMode.HMD;
                }
            }
            else
            {
                return TrackingMode.DESKTOP;
            }
        }

        private void SetTrackingMode(TrackingMode _mode)
        {
            TouchFreeLog.WriteLine($"Requesting {_mode} tracking mode");

            CurrentTrackingMode = _mode;

            switch (_mode)
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
}
using System;
using System.Threading.Tasks;
using Ultraleap.TouchFree.Library.Configuration;

namespace Ultraleap.TouchFree.Library.Connections
{
    public class TrackingConnectionManager : ITrackingConnectionManager
    {
        public Leap.Controller controller { get; private set; }

        private readonly ITrackingDiagnosticApi diagnosticApi;
        private readonly IConfigManager configManager;

        private const int maximumWaitTimeSeconds = 30;
        private const int initialWaitTimeSeconds = 1;
        private bool ShouldConnect = false;

        public bool ShouldSendHandData { get; set; }

        private TrackingMode currentTrackingMode;

        public TrackingMode CurrentTrackingMode
        {
            get { return currentTrackingMode; }
        }

        public TrackingConnectionManager(IConfigManager _configManager, ITrackingDiagnosticApi _diagnosticApi)
        {
            diagnosticApi = _diagnosticApi;
            configManager = _configManager;
            controller = new Leap.Controller();
            controller.Connect += Controller_Connect;
            controller.Disconnect += Controller_Disconnect;
            UpdateTrackingMode(_configManager.PhysicalConfig);
            _configManager.OnPhysicalConfigUpdated += UpdateTrackingMode;
            controller.StopConnection();

            controller.Device += Controller_CameraConnected;
        }

        public void Connect()
        {
            ShouldConnect = true;
            CheckConnectionAndRetryOnFailure();
        }

        public void Disconnect()
        {
            ShouldConnect = false;
            if (controller.IsServiceConnected)
            {
                controller.StopConnection();
            }
        }

        public void Controller_CameraConnected(object sender, Leap.DeviceEventArgs e)
        {
            diagnosticApi.TriggerUpdatingTrackingConfiguration();
        }

        private void Controller_Connect(object sender, Leap.ConnectionEventArgs e)
        {
            UpdateTrackingMode(configManager.PhysicalConfig);

            CheckTrackingModeIsCorrectAfterDelay();
        }

        private async void CheckTrackingModeIsCorrectAfterDelay()
        {
            await Task.Delay(5000);
            if (controller.IsServiceConnected)
            {
                var trackingMode = GetTrackingModeFromConfig(configManager.PhysicalConfig);

                var inScreenTop = controller.IsPolicySet(Leap.Controller.PolicyFlag.POLICY_OPTIMIZE_SCREENTOP);
                var inHmd = controller.IsPolicySet(Leap.Controller.PolicyFlag.POLICY_OPTIMIZE_HMD);

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

        private void Controller_Disconnect(object sender, Leap.ConnectionLostEventArgs e)
        {
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
                await Task.Delay(1000 * waitTimeSeconds);
                waitTimeSeconds = IncreaseWaitTimeSeconds(waitTimeSeconds);
            }

            while (!controller.IsServiceConnected && ShouldConnect)
            {
                controller.StartConnection();

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

        public void UpdateTrackingMode(PhysicalConfigInternal _config)
        {
            SetTrackingMode(GetTrackingModeFromConfig(_config));
        }

        TrackingMode GetTrackingModeFromConfig(PhysicalConfigInternal _config)
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

        void SetTrackingMode(TrackingMode _mode)
        {
            TouchFreeLog.WriteLine($"Requesting {_mode} tracking mode");

            currentTrackingMode = _mode;

            switch (_mode)
            {
                case TrackingMode.DESKTOP:
                    controller.ClearPolicy(Leap.Controller.PolicyFlag.POLICY_OPTIMIZE_SCREENTOP);
                    controller.ClearPolicy(Leap.Controller.PolicyFlag.POLICY_OPTIMIZE_HMD);
                    break;
                case TrackingMode.HMD:
                    controller.ClearPolicy(Leap.Controller.PolicyFlag.POLICY_OPTIMIZE_SCREENTOP);
                    controller.SetPolicy(Leap.Controller.PolicyFlag.POLICY_OPTIMIZE_HMD);
                    break;
                case TrackingMode.SCREENTOP:
                    controller.SetPolicy(Leap.Controller.PolicyFlag.POLICY_OPTIMIZE_SCREENTOP);
                    controller.ClearPolicy(Leap.Controller.PolicyFlag.POLICY_OPTIMIZE_HMD);
                    break;
            }
        }

        public void SetImagesState(bool enabled)
        {
            if (enabled)
            {
                controller.SetPolicy(Leap.Controller.PolicyFlag.POLICY_IMAGES);
            }
            else
            {
                controller.ClearPolicy(Leap.Controller.PolicyFlag.POLICY_IMAGES);
            }
        }
    }
}
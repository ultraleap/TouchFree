using System;
using System.Threading.Tasks;
using Ultraleap.TouchFree.Library.Configuration;

namespace Ultraleap.TouchFree.Library
{
    public class TrackingConnectionManager
    {
        public Leap.Controller controller;
        IConfigManager configManager;

        private int maximumWaitTimeSeconds = 30;
        private int initialWaitTimeSeconds = 1;
        private int waitTimeSeconds = 1;
        private bool ShouldConnect = false;

        public TrackingConnectionManager(IConfigManager _configManager)
        {
            configManager = _configManager;
            controller = new Leap.Controller();
            controller.Connect += Controller_Connect;
            controller.Disconnect += Controller_Disconnect;
            UpdateTrackingMode(_configManager.PhysicalConfig);
            _configManager.OnPhysicalConfigUpdated += UpdateTrackingMode;
            //controller.StopConnection();
        }

        public void Connect()
        {
            ShouldConnect = true;
            //CheckConnectionAndRetryOnFailure();
        }

        public void Disconnect()
        {
            ShouldConnect = false;
            if (controller.IsServiceConnected)
            {
                //controller.StopConnection();
            }
        }

        private void Controller_Connect(object sender, Leap.ConnectionEventArgs e)
        {
            UpdateTrackingMode(configManager.PhysicalConfig);
        }

        private void Controller_Disconnect(object sender, Leap.ConnectionLostEventArgs e)
        {
            waitTimeSeconds = initialWaitTimeSeconds;
            if (ShouldConnect)
            {
                CheckConnectionAndRetryOnFailure();
            }
        }

        private async Task CheckConnectionAndRetryOnFailure()
        {
            while (true)
            {
                await Task.Delay(1000 * waitTimeSeconds);

                if (!controller.IsServiceConnected && ShouldConnect)
                {
                    controller.StartConnection();
                }
                else
                {
                    break;
                }

                waitTimeSeconds *= 2;
                waitTimeSeconds = waitTimeSeconds > maximumWaitTimeSeconds ? maximumWaitTimeSeconds : waitTimeSeconds;
            }
        }

        public void UpdateTrackingMode(PhysicalConfigInternal _config)
        {
            // leap is looking down
            if (Math.Abs(_config.LeapRotationD.Z) > 90f)
            {
                if (_config.LeapRotationD.X <= 0f)
                {
                    SetTrackingMode(TrackingMode.SCREENTOP);
                }
                else
                {
                    SetTrackingMode(TrackingMode.HMD);
                }
            }
            else
            {
                SetTrackingMode(TrackingMode.DESKTOP);
            }
        }

        void SetTrackingMode(TrackingMode _mode)
        {
            TouchFreeLog.WriteLine($"Requesting {_mode} tracking mode");

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

        enum TrackingMode
        {
            DESKTOP,
            HMD,
            SCREENTOP
        }
    }
}
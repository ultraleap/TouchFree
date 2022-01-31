using System;
using Ultraleap.TouchFree.Library.Configuration;

namespace Ultraleap.TouchFree.Library
{
    public class TrackingConnectionManager
    {
        public Leap.Controller controller;

        public TrackingConnectionManager(IConfigManager _configManager)
        {
            controller = new Leap.Controller();
            UpdateTrackingMode(_configManager.PhysicalConfig);
            _configManager.OnPhysicalConfigUpdated += UpdateTrackingMode;
        }

        public void UpdateTrackingMode(PhysicalConfig _config)
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
            Console.WriteLine($"Requesting {_mode} tracking mode");

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
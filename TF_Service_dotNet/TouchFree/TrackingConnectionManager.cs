using System;
using Ultraleap.TouchFree.Library.Configuration;

namespace Ultraleap.TouchFree.Library
{
    public class TrackingConnectionManager
    {
        public Leap.Controller controller;

        public TrackingConnectionManager()
        {
            controller = new Leap.Controller();
            UpdateTrackingMode(ConfigManager.PhysicalConfig);
            ConfigManager.PhysicalConfig.OnConfigUpdated += UpdateTrackingMode;
        }

        public void UpdateTrackingMode(BaseConfig _config)
        {
            PhysicalConfig config = _config as PhysicalConfig;

            // leap is looking down
            if (Math.Abs(config.LeapRotationD.Z) > 90f)
            {
                if (config.LeapRotationD.X <= 0f)
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
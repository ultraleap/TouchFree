using System;
using Leap;
using Ultraleap.TouchFree.Service.Configuration;

namespace Ultraleap.TouchFree.Service
{
    class TrackingConnectionManager
    {
        Controller controller;

        public TrackingConnectionManager()
        {
            controller = new Controller();
            UpdateTrackingMode();
        }

        public void UpdateTrackingMode()
        {
            // leap is looking down
            if (Math.Abs(ConfigManager.PhysicalConfig.LeapRotationD.Z) > 90f)
            {
                if (ConfigManager.PhysicalConfig.LeapRotationD.X <= 0f)
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
                    controller.ClearPolicy(Controller.PolicyFlag.POLICY_OPTIMIZE_SCREENTOP);
                    controller.ClearPolicy(Controller.PolicyFlag.POLICY_OPTIMIZE_HMD);
                    break;
                case TrackingMode.HMD:
                    controller.ClearPolicy(Controller.PolicyFlag.POLICY_OPTIMIZE_SCREENTOP);
                    controller.SetPolicy(Controller.PolicyFlag.POLICY_OPTIMIZE_HMD);
                    break;
                case TrackingMode.SCREENTOP:
                    controller.SetPolicy(Controller.PolicyFlag.POLICY_OPTIMIZE_SCREENTOP);
                    controller.ClearPolicy(Controller.PolicyFlag.POLICY_OPTIMIZE_HMD);
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
using System;
using Leap;
using Ultraleap.TouchFree.Service.Configuration;

namespace Ultraleap.TouchFree.Service
{
    class TrackingModeManager
    {
        public static void UpdateTrackingMode()
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

        static void SetTrackingMode(TrackingMode _mode)
        {
            Console.WriteLine($"Requesting {_mode} tracking mode");

            switch (_mode)
            {
                case TrackingMode.DESKTOP:
                    Program.controller.ClearPolicy(Controller.PolicyFlag.POLICY_OPTIMIZE_SCREENTOP);
                    Program.controller.ClearPolicy(Controller.PolicyFlag.POLICY_OPTIMIZE_HMD);
                    break;
                case TrackingMode.HMD:
                    Program.controller.ClearPolicy(Controller.PolicyFlag.POLICY_OPTIMIZE_SCREENTOP);
                    Program.controller.SetPolicy(Controller.PolicyFlag.POLICY_OPTIMIZE_HMD);
                    break;
                case TrackingMode.SCREENTOP:
                    Program.controller.SetPolicy(Controller.PolicyFlag.POLICY_OPTIMIZE_SCREENTOP);
                    Program.controller.ClearPolicy(Controller.PolicyFlag.POLICY_OPTIMIZE_HMD);
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

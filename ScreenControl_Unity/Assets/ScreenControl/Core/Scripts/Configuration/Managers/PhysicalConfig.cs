using System;
using UnityEngine;

namespace Ultraleap.ScreenControl.Core
{
    [Serializable]
    public class PhysicalConfig : BaseSettings
    {
        public float ScreenHeightM = 0.33f;
        public Vector3 LeapPositionRelativeToScreenBottomM = new Vector3(0f, -0.12f, -0.25f);
        public Vector3 LeapRotationD = Vector3.zero;
        public float ScreenRotationD = 0f;

        public int ScreenWidthPX = 0;
        public int ScreenHeightPX = 0;

        public void SaveConfig()
        {
            PhysicalConfigFile.SaveConfig(this);
        }

        public override void SetAllValuesToDefault()
        {
            var defaults = new PhysicalConfig();

            ScreenHeightM = defaults.ScreenHeightM;
            LeapPositionRelativeToScreenBottomM = defaults.LeapPositionRelativeToScreenBottomM;
            LeapRotationD = defaults.LeapRotationD;
            ScreenRotationD = defaults.ScreenRotationD;
        }
    }
}
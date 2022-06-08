using System;
using System.Numerics;

namespace Ultraleap.TouchFree.Library.Configuration
{
    public class PhysicalConfigFile : ConfigFile<PhysicalConfig, PhysicalConfigFile>
    {
        protected override string _ConfigFileName => "PhysicalConfig.json";
    }

    [Serializable]
    public class PhysicalConfig
    {
        public float ScreenHeightM = 0.33f;
        public Vector3 LeapPositionRelativeToScreenBottomM = new Vector3(0f, -0.12f, -0.25f);
        public Vector3 LeapRotationD = Vector3.Zero;
        public float ScreenRotationD = 0f;

        public int ScreenWidthPX = 0;
        public int ScreenHeightPX = 0;

        public PhysicalConfig()
        {
            this.ScreenHeightM = 0.33f;
            this.LeapPositionRelativeToScreenBottomM = new Vector3(0f, -0.12f, -0.25f);
            this.LeapRotationD = Vector3.Zero;
            this.ScreenRotationD = 0f;

            this.ScreenWidthPX = 0;
            this.ScreenHeightPX = 0;
        }

        public PhysicalConfig(PhysicalConfigInternal _internal)
        {
            this.ScreenHeightM = _internal.ScreenHeightMm / 1000f;
            this.LeapPositionRelativeToScreenBottomM = _internal.LeapPositionRelativeToScreenBottomMm / 1000f;

            this.LeapRotationD = _internal.LeapRotationD;
            this.ScreenRotationD = _internal.ScreenRotationD;

            this.ScreenWidthPX = _internal.ScreenWidthPX;
            this.ScreenHeightPX = _internal.ScreenHeightPX;
        }
    }
}
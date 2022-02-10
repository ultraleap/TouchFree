using System;
using System.Numerics;

namespace Ultraleap.TouchFree.Library.Configuration
{
    public class PhysicalConfigFile : ConfigFile<PhysicalConfigForFile, PhysicalConfigFile>
    {
        protected override string _ConfigFileName => "PhysicalConfig.json";
    }

    [Serializable]
    public class PhysicalConfigForFile
    {
        public float ScreenHeightM = 330f;
        public Vector3 LeapPositionRelativeToScreenBottomM = new Vector3(0f, -120f, -250f);
        public Vector3 LeapRotationD = Vector3.Zero;
        public float ScreenRotationD = 0f;

        public int ScreenWidthPX = 0;
        public int ScreenHeightPX = 0;
    }
}
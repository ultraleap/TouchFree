using System;
using System.Numerics;

namespace Ultraleap.TouchFree.Service.Configuration
{
    [Serializable]
    public class PhysicalConfig : BaseConfig
    {
        public float ScreenHeightM = 0.33f;
        public Vector3 LeapPositionRelativeToScreenBottomM = new Vector3(0f, -0.12f, -0.25f);
        public Vector3 LeapRotationD = Vector3.Zero;
        public float ScreenRotationD = 0f;

        public int ScreenWidthPX = 0;
        public int ScreenHeightPX = 0;
    }
}
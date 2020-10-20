using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ultraleap.ScreenControl.Client
{
    public class ConfigurationData
    {
    }

    public class PhysicalConfiguration
    {
        public float ScreenHeightM = 0.33f;
        public Vector3 LeapPositionRelativeToScreenBottomM = new Vector3(0f, -0.12f, -0.25f);
        public Vector3 LeapRotationD = Vector3.zero;
        public float ScreenRotationD = 0f;
    }
}
using System;

namespace Ultraleap.TouchFree.Library.Interactions
{
    [Serializable]
    public class AirPushSettings
    {
        public float SpeedMin { get; set; }
        public float SpeedMax { get; set; }
        public float DistAtSpeedMinMm { get; set; }
        public float DistAtSpeedMaxMm { get; set; }
        public float HorizontalDecayDistMm { get; set; }
        public float ThetaOne { get; set; }
        public float ThetaTwo { get; set; }
        public float UnclickThreshold { get; set; }
        public float UnclickThresholdDrag { get; set; }
    }
}

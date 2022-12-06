using System;

namespace Ultraleap.TouchFree.Library.Interactions
{
    [Serializable]
    public class VelocitySwipeSettings
    {
        public float MinScrollVelocity_mmps { get; set; }
        public float UpwardsMinVelocityDecrease_mmps { get; set; }
        public float DownwardsMinVelocityIncrease_mmps { get; set; }
        public float MaxReleaseVelocity_mmps { get; set; }
        public float MaxLateralVelocity_mmps { get; set; }
        public float MaxOpposingVelocity_mmps { get; set; }
        public float ScrollDelayMs { get; set; }
        public float MinSwipeLength { get; set; }
        public float MaxSwipeWidth { get; set; }
        public float SwipeWidthScaling { get; set; }
    }
}

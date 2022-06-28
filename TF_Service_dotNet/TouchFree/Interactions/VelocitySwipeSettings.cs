using System;

namespace Ultraleap.TouchFree.Library.Interactions
{
    [Serializable]
    public class VelocitySwipeSettings
    {
        public float MinScrollVelocity_mmps { get; set; }
        public float MaxReleaseVelocity_mmps { get; set; }
        public float MaxOpposingVelocity_mmps { get; set; }
        public float ScrollDelayMs { get; set; }
        public bool AllowVerticalScroll { get; set; }
        public bool AllowHorizontalScroll { get; set; }
        public bool AllowBidirectionalScroll { get; set; }
    }
}

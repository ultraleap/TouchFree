using System;

namespace Ultraleap.TouchFree.Library.Interactions
{
    [Serializable]
    public class InteractionTuning
    {
        public VelocitySwipeSettings VelocitySwipeSettings { get; set; }
        public AirPushSettings AirPushSettings { get; set; }
        public bool EnableInteractionConfidence { get; set; }
        public bool EnableAirClickWithAirPush { get; set; }
        public bool EnableVelocitySwipeWithAirPush { get; set; }
        public bool EnableOneEuroFilter { get; set; }
        public bool EnableExtrapolation { get; set; }
    }
}

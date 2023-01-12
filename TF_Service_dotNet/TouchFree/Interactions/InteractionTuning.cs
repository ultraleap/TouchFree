using System;

namespace Ultraleap.TouchFree.Library.Interactions
{
    [Serializable]
    public class InteractionTuning
    {
        public bool EnableInteractionConfidence { get; set; }
        public bool EnableAirClickWithAirPush { get; set; }
        public bool EnableOneEuroFilter { get; set; }
        public bool EnableExtrapolation { get; set; }
    }
}

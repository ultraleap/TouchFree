using System;

namespace Ultraleap.TouchFree.Library.Interactions;

[Serializable]
public record InteractionTuning(
    bool EnableInteractionConfidence,
    bool EnableAirClickWithAirPush,
    bool EnableOneEuroFilter,
    bool EnableExtrapolation)
{
    public InteractionTuning()
        : this(default, default, default, default)
    {}
}
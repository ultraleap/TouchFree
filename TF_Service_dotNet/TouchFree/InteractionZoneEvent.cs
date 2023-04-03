using System;

namespace Ultraleap.TouchFree.Library;

[Serializable]
public readonly record struct InteractionZoneEvent(InteractionZoneState state);

public enum InteractionZoneState
{
    HAND_ENTERED,
    HAND_EXITED
}
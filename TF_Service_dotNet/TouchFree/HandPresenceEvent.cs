using System;

namespace Ultraleap.TouchFree.Library;

[Serializable]
public readonly record struct HandPresenceEvent(HandPresenceState state);

public enum HandPresenceState
{
    HAND_FOUND,
    HANDS_LOST
}
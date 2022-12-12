using System;

namespace Ultraleap.TouchFree.Library
{

    [Serializable]
    public struct HandPresenceEvent
    {
        public HandPresenceState state;

        public HandPresenceEvent(HandPresenceState _state)
        {
            state = _state;
        }
    }

    public enum HandPresenceState
    {
        HAND_FOUND,
        HANDS_LOST
    }
}

using System;

namespace Ultraleap.TouchFree.Library
{

    [Serializable]
    public struct InteractionZoneEvent
    {
        public InteractionZoneState state;

        public InteractionZoneEvent(InteractionZoneState _state)
        {
            state = _state;
        }
    }

    public enum InteractionZoneState
    {
        HAND_ENTERED,
        HAND_EXITED
    }
}

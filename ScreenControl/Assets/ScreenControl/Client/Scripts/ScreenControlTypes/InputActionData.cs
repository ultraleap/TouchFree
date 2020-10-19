using UnityEngine;

namespace Ultraleap.ScreenControl.Client
{
    namespace ScreenControlTypes
    {
        public enum InputType
        {
            CANCEL,
            DOWN,
            MOVE,
            UP,
        }

        public enum HandChirality
        {
            LEFT,
            RIGHT,
            UNKNOWN,
        }

        // TODO: This needs to feature _all_ interactions available in core - what are those interactions???
        public enum InteractionType
        {
            Undefined,
            Push,           // ForcePoke  - called airpush in docs
            Grab,           // PinchGrab  - should rename to just grab
            Hover,          // HoverAndHold

            // Do we keep these?
            // OneToOne / TouchPlane - maybe? (distinct to airpush)
            // BitePoint             - probably not (airpush is similar but better)
            // PokeToInvoke          - probably not (airpush is similar but better)
        }

        public readonly struct ClientInputAction
        {
            public readonly long Timestamp;
            public readonly InteractionType Source;
            public readonly HandChirality Chirality;
            public readonly InputType Type;
            public readonly Vector2 CursorPosition;
            public readonly float ProgressToClick;

            public ClientInputAction(
                long _timestamp,
                InteractionType _source,
                HandChirality _chirality,
                InputType _type,
                Vector2 _cursorPosition,
                float _progressToClick)
            {
                Timestamp = _timestamp;
                Source = _source;
                Chirality = _chirality;
                Type = _type;
                CursorPosition = _cursorPosition;
                ProgressToClick = _progressToClick;
            }
        }
    }
}
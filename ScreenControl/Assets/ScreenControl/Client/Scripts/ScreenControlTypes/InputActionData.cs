using UnityEngine;

namespace Ultraleap.ScreenControl.Client
{
    namespace ScreenControlTypes
    {
        public readonly struct ClientInputAction
        {
            public readonly long Timestamp;
            public readonly InteractionType InteractionType;
            public readonly HandType HandType;
            public readonly HandChirality Chirality;
            public readonly InputType InputType;
            public readonly Vector2 CursorPosition;
            public readonly float ProgressToClick;

            public ClientInputAction(
                long _timestamp,
                InteractionType _interactionType,
                HandType _handType,
                HandChirality _chirality,
                InputType _inputType,
                Vector2 _cursorPosition,
                float _progressToClick)
            {
                Timestamp = _timestamp;
                InteractionType = _interactionType;
                HandType = _handType;
                Chirality = _chirality;
                InputType = _inputType;
                CursorPosition = _cursorPosition;
                ProgressToClick = _progressToClick;
            }
        }

        public enum HandChirality
        {
            LEFT,
            RIGHT
        }

        public enum HandType
        {
            PRIMARY,
            SECONDARY,
        }

        public enum InputType
        {
            CANCEL,
            DOWN,
            MOVE,
            UP,
        }

        public enum InteractionType
        {
            GRAB,
            HOVER,
            PUSH,
        }
    }
}
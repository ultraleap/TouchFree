using UnityEngine;

namespace Ultraleap.ScreenControl.Core
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

        public enum InteractionType
        {
            Undefined,
            Push,
            Grab,
            Hover,
        }

        public enum HandType
        {
            PRIMARY,
            SECONDARY,
        }

        public readonly struct InputActionData
        {
            public readonly long Timestamp;
            public readonly InteractionType SourceInteraction;
            public readonly HandType HandType;
            public readonly HandChirality Chirality;
            public readonly InputType InputType;
            public readonly Vector2 CursorPosition;
            public readonly float DistanceFromScreen;
            public readonly float ProgressToClick;
            public InputActionData(long _timestamp, InteractionType _interactionType, HandType _handType, HandChirality _chirality, InputType _inputType, Positions _positions, float _progressToClick)
            {
                Timestamp = _timestamp;
                SourceInteraction = _interactionType;
                HandType = _handType;
                Chirality = _chirality;
                InputType = _inputType;
                CursorPosition = _positions.CursorPosition;
                DistanceFromScreen = _positions.DistanceFromScreen;
                ProgressToClick = _progressToClick;
            }
        }
    }
}
using System;
using System.Numerics;

using Leap;

namespace Ultraleap.TouchFree.Library
{
    public static class VersionInfo
    {
        public static readonly Version ApiVersion = new Version("1.2.0");
        public const string API_HEADER_NAME = "TfApiVersion";
    }

    public readonly struct InputAction
    {
        public readonly long Timestamp;
        public readonly InteractionType InteractionType;
        public readonly HandType HandType;
        public readonly HandChirality Chirality;
        public readonly InputType InputType;
        public readonly Vector2 CursorPosition;
        public readonly float DistanceFromScreen;
        public readonly float ProgressToClick;

        public InputAction(long _timestamp, InteractionType _interactionType, HandType _handType, HandChirality _chirality, InputType _inputType, Positions _positions, float _progressToClick)
        {
            Timestamp = _timestamp;
            InteractionType = _interactionType;
            HandType = _handType;
            Chirality = _chirality;
            InputType = _inputType;
            CursorPosition = _positions.CursorPosition;
            DistanceFromScreen = _positions.DistanceFromScreen;
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
        NONE,
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
        TOUCHPLANE,
        VELOCITYSWIPE,
    }

    [Flags]
    public enum BitmaskFlags
    {
        NONE = 0,

        // HandChirality
        LEFT = 1,
        RIGHT = 2,

        // Hand Type
        PRIMARY = 4,
        SECONDARY = 8,

        // Input Types
        NONE_INPUT = 16,
        CANCEL = 32,
        DOWN = 64,
        MOVE = 128,
        UP = 256,

        // Interaction Types
        GRAB = 512,
        HOVER = 1024,
        PUSH = 2048,
        TOUCHPLANE = 4096,

        // Adding elements to this list is a breaking change, and should cause at
        // least a minor iteration of the API version UNLESS adding them at the end
    }

    [Serializable]
    public struct WebsocketInputAction
    {
        public long Timestamp;
        public BitmaskFlags InteractionFlags;
        public WebSocketVector2 CursorPosition;
        public float DistanceFromScreen;
        public float ProgressToClick;

        public WebsocketInputAction(InputAction _data)
        {
            Timestamp = _data.Timestamp;
            InteractionFlags = Utilities.GetInteractionFlags(_data.InteractionType,
                                                                _data.HandType,
                                                                _data.Chirality,
                                                                _data.InputType);
            CursorPosition = new WebSocketVector2(_data.CursorPosition);
            DistanceFromScreen = _data.DistanceFromScreen;
            ProgressToClick = _data.ProgressToClick;
        }
    }

    public struct Positions
    {
        /**
         * Cursor position is used to guide the position of the cursor representation.
         * It is calculated in Screen Space
         */
        public Vector2 CursorPosition;

        /**
         * Distance from screen is the physical distance of the hand from the screen.
         * It is calculated in meters.
         */
        public float DistanceFromScreen;

        public Positions(Vector2 _cursorPosition, float _distanceFromScreen)
        {
            CursorPosition = _cursorPosition;
            DistanceFromScreen = _distanceFromScreen;
        }
    }

    [Serializable]
    public enum TrackedPosition
    {
        INDEX_STABLE,
        INDEX_TIP,
        WRIST,
        NEAREST,
        HAND_POINTING
    }

    [Serializable]
    public struct WebSocketVector2
    {
        public float x;
        public float y;

        public WebSocketVector2(Vector2 _vector)
        {
            x = _vector.X;
            y = _vector.Y;
        }

        public WebSocketVector2(float _x, float _y)
        {
            x = _x;
            y = _y;
        }
    }
}
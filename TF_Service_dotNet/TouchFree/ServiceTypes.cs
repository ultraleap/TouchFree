using System;
using System.Diagnostics;
using System.Numerics;

using Leap;

namespace Ultraleap.TouchFree.Library
{
    public static class VersionInfo
    {
        public static readonly Version ApiVersion = new Version("1.1.0");
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

    public struct ClientInputAction
    {
        public readonly long Timestamp;
        public readonly InteractionType InteractionType;
        public readonly HandType HandType;
        public readonly HandChirality Chirality;
        public readonly InputType InputType;
        public WebSocketVector2 CursorPosition;
        public readonly float DistanceFromScreen;
        public readonly float ProgressToClick;

        public ClientInputAction(WebsocketInputAction _wsInput)
        {
            Timestamp = _wsInput.Timestamp;
            InteractionType = FlagUtilities.GetInteractionTypeFromFlags(_wsInput.InteractionFlags);
            HandType = FlagUtilities.GetHandTypeFromFlags(_wsInput.InteractionFlags);
            Chirality = FlagUtilities.GetChiralityFromFlags(_wsInput.InteractionFlags);
            InputType = FlagUtilities.GetInputTypeFromFlags(_wsInput.InteractionFlags);
            CursorPosition = _wsInput.CursorPosition;
            DistanceFromScreen = _wsInput.DistanceFromScreen;
            ProgressToClick = _wsInput.ProgressToClick;
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
        NEAREST
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

    // Class: FlagUtilities
    // A collection of Utilities to be used when working with <BitmaskFlags>.
    public static class FlagUtilities
    {
        // Group: Functions

        // Function: GetInteractionFlags
        // Used to convert a collection of interaction enums to flags for sending
        // to the Service.
        internal static BitmaskFlags GetInteractionFlags(
            InteractionType _interactionType,
            HandType _handType,
            HandChirality _chirality,
            InputType _inputType)
        {
            BitmaskFlags returnVal = BitmaskFlags.NONE;

            switch (_handType)
            {
                case HandType.PRIMARY:
                    returnVal ^= BitmaskFlags.PRIMARY;
                    break;

                case HandType.SECONDARY:
                    returnVal ^= BitmaskFlags.SECONDARY;
                    break;
            }

            switch (_chirality)
            {
                case HandChirality.LEFT:
                    returnVal ^= BitmaskFlags.LEFT;
                    break;

                case HandChirality.RIGHT:
                    returnVal ^= BitmaskFlags.RIGHT;
                    break;
            }

            switch (_inputType)
            {
                case InputType.NONE:
                    returnVal ^= BitmaskFlags.NONE_INPUT;
                    break;

                case InputType.CANCEL:
                    returnVal ^= BitmaskFlags.CANCEL;
                    break;

                case InputType.MOVE:
                    returnVal ^= BitmaskFlags.MOVE;
                    break;

                case InputType.UP:
                    returnVal ^= BitmaskFlags.UP;
                    break;

                case InputType.DOWN:
                    returnVal ^= BitmaskFlags.DOWN;
                    break;
            }

            switch (_interactionType)
            {
                case InteractionType.PUSH:
                    returnVal ^= BitmaskFlags.PUSH;
                    break;

                case InteractionType.HOVER:
                    returnVal ^= BitmaskFlags.HOVER;
                    break;

                case InteractionType.GRAB:
                    returnVal ^= BitmaskFlags.GRAB;
                    break;

                case InteractionType.TOUCHPLANE:
                    returnVal ^= BitmaskFlags.TOUCHPLANE;
                    break;
            }

            return returnVal;
        }

        // Function: GetChiralityFromFlags
        // Used to find which <HandChirality> _flags contains. Favours RIGHT if none or both are found.
        internal static HandChirality GetChiralityFromFlags(BitmaskFlags _flags)
        {
            HandChirality chirality = HandChirality.RIGHT;

            if (_flags.HasFlag(BitmaskFlags.RIGHT))
            {
                chirality = HandChirality.RIGHT;
            }
            else if (_flags.HasFlag(BitmaskFlags.LEFT))
            {
                chirality = HandChirality.LEFT;
            }
            else
            {
                Debug.WriteLine("InputActionData missing: No Chirality found. Defaulting to 'RIGHT'");
            }

            return chirality;
        }

        // Function: GetHandTypeFromFlags
        // Used to find which <HandType> _flags contains. Favours PRIMARY if none or both are found.
        internal static HandType GetHandTypeFromFlags(BitmaskFlags _flags)
        {
            HandType handType = HandType.PRIMARY;

            if (_flags.HasFlag(BitmaskFlags.PRIMARY))
            {
                handType = HandType.PRIMARY;
            }
            else if (_flags.HasFlag(BitmaskFlags.SECONDARY))
            {
                handType = HandType.SECONDARY;
            }
            else
            {
                Debug.WriteLine("InputActionData missing: No HandData found. Defaulting to 'PRIMARY'");
            }

            return handType;
        }

        // Function: GetInputTypeFromFlags
        // Used to find which <InputType> _flags contains. Favours NONE if none are found.
        internal static InputType GetInputTypeFromFlags(BitmaskFlags _flags)
        {
            InputType inputType = InputType.NONE;

            if (_flags.HasFlag(BitmaskFlags.NONE_INPUT))
            {
                inputType = InputType.NONE;
            }
            else if (_flags.HasFlag(BitmaskFlags.CANCEL))
            {
                inputType = InputType.CANCEL;
            }
            else if (_flags.HasFlag(BitmaskFlags.UP))
            {
                inputType = InputType.UP;
            }
            else if (_flags.HasFlag(BitmaskFlags.DOWN))
            {
                inputType = InputType.DOWN;
            }
            else if (_flags.HasFlag(BitmaskFlags.MOVE))
            {
                inputType = InputType.MOVE;
            }
            else
            {
                Debug.WriteLine("InputActionData missing: No InputType found. Defaulting to 'NONE'");
            }

            return inputType;
        }

        // Function: GetInteractionTypeFromFlags
        // Used to find which <InteractionType> _flags contains. Favours PUSH if none are found.
        internal static InteractionType GetInteractionTypeFromFlags(BitmaskFlags _flags)
        {
            InteractionType interactionType = InteractionType.PUSH;

            if (_flags.HasFlag(BitmaskFlags.PUSH))
            {
                interactionType = InteractionType.PUSH;
            }
            else if (_flags.HasFlag(BitmaskFlags.HOVER))
            {
                interactionType = InteractionType.HOVER;
            }
            else if (_flags.HasFlag(BitmaskFlags.GRAB))
            {
                interactionType = InteractionType.GRAB;
            }
            else if (_flags.HasFlag(BitmaskFlags.TOUCHPLANE))
            {
                interactionType = InteractionType.TOUCHPLANE;
            }
            else
            {
                Debug.WriteLine("InputActionData missing: No InteractionType found. Defaulting to 'PUSH'");
            }

            return interactionType;
        }
    }
}
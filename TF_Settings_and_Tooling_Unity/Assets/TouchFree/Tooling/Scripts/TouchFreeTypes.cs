using System;
using UnityEngine;

namespace Ultraleap.TouchFree.Tooling
{
    // Class: VersionInfo
    // This class is used when comparing the <ApiVersion> of the Tooling and the Service.
    public static class VersionInfo
    {
        // Group: Variables

        // Variable: ApiVersion
        // The current API version of the Tooling.
        public static readonly Version ApiVersion = new Version("1.3.0");

        // Variable: API_HEADER_NAME
        // The name of the header we wish the Service to compare our version with.
        public const string API_HEADER_NAME = "TfApiVersion";
    }

    // Struct: InputAction
    // The Tooling representation of an InputAction. This is used to pass
    // key information relating to an action that has happened on the Service.
    public struct InputAction
    {
        public long Timestamp;
        public InteractionType InteractionType;
        public HandType HandType;
        public HandChirality Chirality;
        public InputType InputType;
        public Vector2 CursorPosition;
        public float DistanceFromScreen;
        public float ProgressToClick;

        public InputAction(
            long _timestamp,
            InteractionType _interactionType,
            HandType _handType,
            HandChirality _chirality,
            InputType _inputType,
            Vector2 _cursorPosition,
            float _distanceFromScreen,
            float _progressToClick)
        {
            Timestamp = _timestamp;
            InteractionType = _interactionType;
            HandType = _handType;
            Chirality = _chirality;
            InputType = _inputType;
            CursorPosition = _cursorPosition;
            DistanceFromScreen = _distanceFromScreen;
            ProgressToClick = _progressToClick;
        }

        public InputAction(WebsocketInputAction _wsInput)
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

    // Enum: HandChirality
    // LEFT - The left hand
    // RIGHT - The right hand
    public enum HandChirality
    {
        LEFT,
        RIGHT
    }

    // Enum: HandType
    // PRIMARY - The first hand found
    // SECONDARY - The second hand found
    public enum HandType
    {
        PRIMARY,
        SECONDARY,
    }

    // Enum: InputType
    // NONE - Used to be ignored by the input system but to still receive information such as distance to screen
    // CANCEL - Used to cancel the current input if an issue occurs. Particularly when a DOWN has happened before an UP
    // DOWN - Used to begin a 'Touch' or a 'Drag'
    // MOVE - Used to move a cursor or to perform a 'Drag' after a DOWN
    // UP - Used to complete a 'Touch' or a 'Drag'
    public enum InputType
    {
        NONE,
        CANCEL,
        DOWN,
        MOVE,
        UP,
    }

    // Enum: InteractionType
    // GRAB - The user must perform a GRAB gesture to 'Touch' by bringing their fingers and thumb together
    // HOVER - The user must perform a HOVER gesture to 'Touch' by holding their hand still for a fixed time
    // PUSH - The user must perform a PUSH gesture to 'Touch' by pushing their hand toward the screen
    public enum InteractionType
    {
        GRAB,
        HOVER,
        PUSH,
        TOUCHPLANE,
        VELOCITYSWIPE,
    }

    // Enum: BitmaskFlags
    // This is used to request any combination of the <HandChiralities>, <HandTypes>, <InputTypes>,
    // and <InteractionTypes> flags from the Service at once.
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
        VELOCITYSWIPE = 8192,

        // Adding elements to this list is a breaking change, and should cause at
        // least a minor iteration of the API version UNLESS adding them at the end
    }

    // Struct: WebsocketInputAction
    // The version of an InputAction received via the WebSocket. This must be converted into a
    // <InputAction> to be used by the Tooling and can be done so via its constructor.
    [Serializable]
    public struct WebsocketInputAction
    {
        public long Timestamp;
        public BitmaskFlags InteractionFlags;
        public Vector2 CursorPosition;
        public float DistanceFromScreen;
        public float ProgressToClick;
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
                    
                case InteractionType.VELOCITYSWIPE:
                    returnVal ^= BitmaskFlags.VELOCITYSWIPE;
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
                Debug.LogError("InputActionData missing: No Chirality found. Defaulting to 'RIGHT'");
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
                Debug.LogError("InputActionData missing: No HandData found. Defaulting to 'PRIMARY'");
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
                Debug.LogError("InputActionData missing: No InputType found. Defaulting to 'NONE'");
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
            else if (_flags.HasFlag(BitmaskFlags.VELOCITYSWIPE))
            {
                interactionType = InteractionType.VELOCITYSWIPE;
            }
            else
            {
                Debug.LogError("InputActionData missing: No InteractionType found. Defaulting to 'PUSH'");
            }

            return interactionType;
        }
    }
}
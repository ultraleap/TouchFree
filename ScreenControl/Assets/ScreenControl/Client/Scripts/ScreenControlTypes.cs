using System;
using UnityEngine;

namespace Ultraleap.ScreenControl.Client
{
    namespace ScreenControlTypes
    {
        public static class VersionInfo
        {
            public static readonly Version ApiVersion = new Version("1.0.0");
            public const string API_HEADER_NAME = "ScApiVersion";
        }

        public readonly struct ClientInputAction
        {
            public readonly long Timestamp;
            public readonly InteractionType InteractionType;
            public readonly HandType HandType;
            public readonly HandChirality Chirality;
            public readonly InputType InputType;
            public readonly Vector2 CursorPosition;
            public readonly float DistanceFromScreen;
            public readonly float ProgressToClick;

            public ClientInputAction(
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

            public ClientInputAction(WebsocketInputAction _wsInput)
            {
                Timestamp = _wsInput.Timestamp;
                InteractionType = Utilities.GetInteractionTypeFromFlags(_wsInput.InteractionFlags);
                HandType = Utilities.GetHandTypeFromFlags(_wsInput.InteractionFlags);
                Chirality = Utilities.GetChiralityFromFlags(_wsInput.InteractionFlags);
                InputType = Utilities.GetInputTypeFromFlags(_wsInput.InteractionFlags);
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
            CANCEL = 16,
            DOWN = 32,
            MOVE = 64,
            UP = 128,

            // Interaction Types
            GRAB = 256,
            HOVER = 512,
            PUSH = 1024,

            // Adding elements to this list is a breaking change, and should cause at
            // least a minor iteration of the API version UNLESS adding them at the end
        }

        [Serializable]
        public struct WebsocketInputAction
        {
            public long Timestamp;
            public BitmaskFlags InteractionFlags;
            public Vector2 CursorPosition;
            public float DistanceFromScreen;
            public float ProgressToClick;

            public WebsocketInputAction(ClientInputAction _data)
            {
                Timestamp = _data.Timestamp;
                InteractionFlags = Utilities.GetInteractionFlags(_data.InteractionType,
                                                                 _data.HandType,
                                                                 _data.Chirality,
                                                                 _data.InputType);
                CursorPosition = _data.CursorPosition;
                DistanceFromScreen = _data.DistanceFromScreen;
                ProgressToClick = _data.ProgressToClick;
            }
        }
        
        public static class Utilities
        {
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
                }

                return returnVal;
            }

            internal static HandChirality GetChiralityFromFlags (BitmaskFlags _flags)
            {
                HandChirality chirality = HandChirality.RIGHT;

                if(_flags.HasFlag(BitmaskFlags.LEFT))
                {
                    chirality = HandChirality.LEFT;
                }
                else if(_flags.HasFlag(BitmaskFlags.RIGHT))
                {
                    chirality = HandChirality.RIGHT;
                }
                else
                {
                    Debug.LogError("InputActionData missing: No Chirality found. Defaulting to 'RIGHT'");
                }

                return chirality;
            }

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

            internal static InputType GetInputTypeFromFlags(BitmaskFlags _flags)
            {
                InputType inputType = InputType.CANCEL;

                if (_flags.HasFlag(BitmaskFlags.CANCEL))
                {
                    inputType = InputType.CANCEL;
                }
                else if (_flags.HasFlag(BitmaskFlags.DOWN))
                {
                    inputType = InputType.DOWN;
                }
                else if (_flags.HasFlag(BitmaskFlags.MOVE))
                {
                    inputType = InputType.MOVE;
                }
                else if (_flags.HasFlag(BitmaskFlags.UP))
                {
                    inputType = InputType.UP;
                }
                else
                {
                    Debug.LogError("InputActionData missing: No InputType found. Defaulting to 'CANCEL'");
                }

                return inputType;
            }

            internal static InteractionType GetInteractionTypeFromFlags(BitmaskFlags _flags)
            {
                InteractionType interactionType = InteractionType.PUSH;

                if (_flags.HasFlag(BitmaskFlags.GRAB))
                {
                    interactionType = InteractionType.GRAB;
                }
                else if (_flags.HasFlag(BitmaskFlags.HOVER))
                {
                    interactionType = InteractionType.HOVER;
                }
                else if (_flags.HasFlag(BitmaskFlags.PUSH))
                {
                    interactionType = InteractionType.PUSH;
                }
                else
                {
                    Debug.LogError("InputActionData missing: No InteractionType found. Defaulting to 'PUSH'");
                }

                return interactionType;
            }
        }
    }
}
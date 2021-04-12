using System;

using UnityEngine;

namespace Ultraleap.ScreenControl.Core
{
    namespace ScreenControlTypes
    {
        public static class VersionInfo
        {
            public static readonly Version ApiVersion = new Version("1.0.2");
            public const string API_HEADER_NAME = "ScApiVersion";
        }

        public readonly struct CoreInputAction
        {
            public readonly long Timestamp;
            public readonly InteractionType InteractionType;
            public readonly HandType HandType;
            public readonly HandChirality Chirality;
            public readonly InputType InputType;
            public readonly Vector2 CursorPosition;
            public readonly float DistanceFromScreen;
            public readonly float ProgressToClick;
            public CoreInputAction(long _timestamp, InteractionType _interactionType, HandType _handType, HandChirality _chirality, InputType _inputType, Positions _positions,float _distanceFromScreen, float _progressToClick)
            {
                Timestamp = _timestamp;
                InteractionType = _interactionType;
                HandType = _handType;
                Chirality = _chirality;
                InputType = _inputType;
                CursorPosition = _positions.CursorPosition;
                DistanceFromScreen = _distanceFromScreen;
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

            public WebsocketInputAction(CoreInputAction _data)
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
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ultraleap.ScreenControl.Core
{
    namespace ScreenControlTypes
    {
        //Touch Hover/Update, Touch Begin/Down, Touch Hold, Touch Up, Touch Cancel
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
    public class InteractionManager : MonoBehaviour
    {
        public delegate void InputAction(ScreenControlTypes.InputActionData _inputData);
        public static event InputAction HandleInputAction;  // For Primary hand data
        public static event InputAction HandleInputActionLeftHand;  // For left-hand data only
        public static event InputAction HandleInputActionRightHand; // For right-hand data only
        public static event InputAction HandleInputActionAll;   // For all hand data

        private void Awake()
        {
            InteractionModule.HandleInputAction += HandleInteractionModuleInputAction;
        }

        private void OnDestroy()
        {
            InteractionModule.HandleInputAction -= HandleInteractionModuleInputAction;
        }

        private void HandleInteractionModuleInputAction(ScreenControlTypes.HandChirality _chirality, ScreenControlTypes.HandType _handType, ScreenControlTypes.InputActionData _inputData)
        {
            HandleInputActionAll?.Invoke(_inputData);

            // Determine which conditional events to send
            if (_handType == ScreenControlTypes.HandType.PRIMARY)
            {
                HandleInputAction?.Invoke(_inputData);
            }

            if (_inputData.Chirality == ScreenControlTypes.HandChirality.LEFT)
            {
                // Send the left-hand action if the hand chirality is LEFT. This is always true if
                // _trackedHand == TrackedHand.LEFT, and may be true if _trackedHand == TrackedHand.PRIMARY
                HandleInputActionLeftHand?.Invoke(_inputData);
            }
            else if (_inputData.Chirality == ScreenControlTypes.HandChirality.RIGHT)
            {
                HandleInputActionRightHand?.Invoke(_inputData);
            }
        }
    }
}
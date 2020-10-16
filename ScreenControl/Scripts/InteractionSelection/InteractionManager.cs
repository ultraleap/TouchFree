using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Touch Hover/Update, Touch Begin/Down, Touch Hold, Touch Up, Touch Cancel
public enum InputType
{
    HOVER,
    DOWN,
    HOLD,
    DRAG,
    UP,
    CANCEL,
    MOVE
}

public enum HandChirality
{
    UNKNOWN,
    LEFT,
    RIGHT,
}

public readonly struct InputActionData
{
    public readonly long Timestamp;
    public readonly InteractionType Source;
    public readonly HandChirality Chirality;
    public readonly InputType Type;
    public readonly Vector2 CursorPosition;
    public readonly Vector2 ClickPosition;
    public readonly float DistanceFromScreen;
    public readonly float ProgressToClick;
    public InputActionData(long _timestamp, InteractionType _source, HandChirality _chirality, InputType _type, Positions _positions, float _progressToClick)
    {
        Timestamp = _timestamp;
        Source = _source;
        Chirality = _chirality;
        Type = _type;
        CursorPosition = _positions.CursorPosition;
        ClickPosition = _positions.ClickPosition;
        DistanceFromScreen = _positions.DistanceFromScreen;
        ProgressToClick = _progressToClick;
    } 

}

public class InteractionManager : MonoBehaviour
{
    public delegate void InputAction(InputActionData _inputData);
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

    private void HandleInteractionModuleInputAction(TrackedHand _trackedHand, InputActionData _inputData)
    {
        HandleInputActionAll?.Invoke(_inputData);

        // Determine which conditional events to send
        if (_trackedHand == TrackedHand.PRIMARY)
        {
            HandleInputAction?.Invoke(_inputData);
        }

        if (_inputData.Chirality == HandChirality.LEFT)
        {
            // Send the left-hand action if the hand chirality is LEFT. This is always true if
            // _trackedHand == TrackedHand.LEFT, and may be true if _trackedHand == TrackedHand.PRIMARY
            HandleInputActionLeftHand?.Invoke(_inputData);
        }
        else if (_inputData.Chirality == HandChirality.RIGHT)
        {
            HandleInputActionRightHand?.Invoke(_inputData);
        }
    }
}
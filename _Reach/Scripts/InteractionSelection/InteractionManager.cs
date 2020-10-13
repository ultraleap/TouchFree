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
    MOVE,
    SETLEFT,
    SETRIGHT
}

public readonly struct InputActionData
{
    public readonly InteractionType Source;
    public readonly InputType Type;
    public readonly Vector2 CursorPosition;
    public readonly Vector2 ClickPosition;
    public readonly float ProgressToClick;
    public InputActionData(InteractionType _source, InputType _type, Vector2 _cursorPosition, Vector2 _clickPosition, float _progressToClick)
    {
        Source = _source;
        Type = _type;
        CursorPosition = _cursorPosition;
        ClickPosition = _clickPosition;
        ProgressToClick = _progressToClick;
    } 

}

public class InteractionManager : MonoBehaviour
{
    public delegate void InputAction(InputActionData _inputData);
    public static event InputAction HandleInputAction;

    private void Awake()
    {
        InteractionModule.HandleInputAction += HandleInteractionModuleInputAction;
    }

    private void OnDestroy()
    {
        InteractionModule.HandleInputAction -= HandleInteractionModuleInputAction;
    }

    private void HandleInteractionModuleInputAction(InputActionData _inputData)
    {
        HandleInputAction?.Invoke(_inputData);
    }
}
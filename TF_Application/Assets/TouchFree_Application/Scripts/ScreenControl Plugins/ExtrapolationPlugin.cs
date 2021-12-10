using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ultraleap.TouchFree.Tooling;

public class ExtrapolationPlugin : InputActionPlugin
{
    InputAction lastAction;

    [Range(0, 10)]
    public float extrapolationAmount = 2;
    float actionTime;

    protected override InputAction? ModifyInputAction(InputAction action)
    {
        InputAction newAction = action;
        actionTime = ((action.Timestamp - lastAction.Timestamp) / 1000000f) / Time.deltaTime;

        newAction.CursorPosition = Vector2.Lerp(newAction.CursorPosition, newAction.CursorPosition + (newAction.CursorPosition - lastAction.CursorPosition) * extrapolationAmount / actionTime, 0.2f);

        lastAction = action;

        return newAction;
    }
}
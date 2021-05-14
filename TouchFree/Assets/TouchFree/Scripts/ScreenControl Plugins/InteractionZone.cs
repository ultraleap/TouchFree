using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ultraleap.ScreenControl.Client;
using System;

public class InteractionZone : InputActionPlugin
{
    public static event InputActionManager.ClientInputActionEvent InputOverrideInputAction;

    public float minInteractionDistance = 0.02f;
    public float maxInteractionDistance = 0.2f;

    public bool useInteractionZone = true;

    protected override Nullable<ClientInputAction> ModifyInputAction(ClientInputAction _inputAction)
    {
        if (useInteractionZone)
        {
            ClientInputAction overrideInputAction = HandleDelayedDownAndUp(_inputAction);
            InputOverrideInputAction?.Invoke(overrideInputAction);
        }
        else
        {
            InputOverrideInputAction?.Invoke(_inputAction);
        }

        return _inputAction;
    }


    Vector2 downPos;
    Vector2 upPos;

    bool delayedDown = false;
    bool delayedUp = false;

    ClientInputAction HandleDelayedDownAndUp(ClientInputAction _inputAction)
    {
        if(!delayedDown)
        {
            if (_inputAction.InputType == InputType.DOWN)
            {
                // This requires a 'fake' down event
                delayedDown = true;

                _inputAction.InputType = InputType.MOVE;
                downPos = _inputAction.CursorPosition;
            }

            if(delayedUp)
            {
                // we prepared a delayed up, so perform it no matter what.
                _inputAction.InputType = InputType.UP;
                _inputAction.CursorPosition = upPos;
                delayedUp = false;
            }
        }
        else
        {
            if (_inputAction.InputType == InputType.MOVE || _inputAction.InputType == InputType.UP)
            {
                // we need to perform the real down event
                if (_inputAction.InputType == InputType.UP)
                {
                    // prepare a delayed up event
                    upPos = _inputAction.CursorPosition;
                    delayedUp = true;
                }

                delayedDown = false;

                _inputAction.InputType = InputType.DOWN;
                _inputAction.CursorPosition = downPos;
            }
        }

        if (_inputAction.DistanceFromScreen < minInteractionDistance ||
                _inputAction.DistanceFromScreen > maxInteractionDistance)
        {
            delayedDown = false;

            if (delayedUp)
            {
                // we prepared a delayed up, so perform it no matter what.
                _inputAction.InputType = InputType.UP;
                _inputAction.CursorPosition = upPos;
                delayedUp = false;
            }
        }

        return _inputAction;
    }
}

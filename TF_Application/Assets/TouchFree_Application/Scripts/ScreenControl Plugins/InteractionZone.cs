using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ultraleap.TouchFree.Tooling;
using Ultraleap.TouchFree;

public class InteractionZone : InputActionPlugin
{
    public static event InputActionManager.InputActionEvent InputOverrideInputAction;

    // function: ModifyInputAction
    // Used to produce a modified <InputAction> which, in this case, is specificly used
    // for input and not for cursors. This is achieved by invoking the <InputOverrideInputAction>.
    // 
    // One exception to this rule is if a <InputAction> has been 'cancelled' which should
    // be passed back to the <InputActionManager>.
    protected override InputAction? ModifyInputAction(InputAction _inputAction)
    {
        if (ConfigManager.Config.interactionZoneEnabled)
        {
            InputAction? overrideInputAction = HandleDelayedDownAndUp(_inputAction);

            if (overrideInputAction.HasValue)
            {
                InputOverrideInputAction?.Invoke(overrideInputAction.Value);

                if(overrideInputAction.Value.InputType == InputType.CANCEL)
                {
                    // Only return modified inputactions if they have been cancelled as
                    // this relates to both input and cursors.
                    return overrideInputAction.Value;
                }
            }
            else
            {
                return null;
            }
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

    InputAction? HandleDelayedDownAndUp(InputAction _inputAction)
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

        if (_inputAction.DistanceFromScreen < (ConfigManager.Config.interactionMinDistanceCm / 100) ||
                _inputAction.DistanceFromScreen > (ConfigManager.Config.interactionMaxDistanceCm / 100))
        {
            delayedDown = false;

            if (delayedUp)
            {
                // we prepared a delayed up, so perform it no matter what.
                _inputAction.InputType = InputType.UP;
                _inputAction.CursorPosition = upPos;
                delayedUp = false;
            }
            else if (!cancelledInput)
            {
                // change to a cancel event as we have just exited the interaction zone
                _inputAction.InputType = InputType.CANCEL;
                cancelledInput = true;
            }
            else
            {
                return null;
            }
        }
        else
        {
            cancelledInput = false;
        }

        return _inputAction;
    }

    bool cancelledInput = false;
}

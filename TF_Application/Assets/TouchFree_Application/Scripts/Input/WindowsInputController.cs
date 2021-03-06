﻿using UnityEngine;
using Ultraleap.TouchFree.Tooling.InputControllers;
using Ultraleap.TouchFree.Tooling;

public class WindowsInputController : InputController
{
    PointerTouchInfo[] touches;

    bool pressing = false;

    protected override void Start()
    {
        DelayedDown.InputOverrideInputAction += HandleInputAction;
        TouchInjection.Initialize(10, TouchFeedback.NONE);

        touches = new PointerTouchInfo[1];
        touches[0].PointerInfo.PointerInputType = PointerInputType.TOUCH;
        touches[0].TouchFlags = TouchFlags.NONE;
        touches[0].TouchMasks = TouchMask.NONE;
        touches[0].PointerInfo.PointerId = 1;
    }

    protected override void OnDestroy()
    {
        DelayedDown.InputOverrideInputAction -= HandleInputAction;
    }

    protected override void HandleInputAction(InputAction _inputData)
    {
        // CTI can block input as we do not require input while CTI is active
        if(CallToInteractController.isShowing)
        {
            return;
        }

        var x = (int)_inputData.CursorPosition.x;
        var y = Display.main.systemHeight - (int)_inputData.CursorPosition.y;

        switch (_inputData.InputType)
        {
            case InputType.DOWN:
                touches[0].PointerInfo.PointerFlags = PointerFlags.DOWN | PointerFlags.INCONTACT | PointerFlags.INRANGE;
                touches[0].PointerInfo.PtPixelLocation.X = x;
                touches[0].PointerInfo.PtPixelLocation.Y = y;
                TouchInjection.Send(touches);
                pressing = true;
                break;
            case InputType.MOVE:
                if (pressing)
                {
                    touches[0].PointerInfo.PointerFlags = PointerFlags.UPDATE | PointerFlags.INCONTACT | PointerFlags.INRANGE;
                }
                else
                {
                    touches[0].PointerInfo.PointerFlags = PointerFlags.UPDATE | PointerFlags.INRANGE;
                }
                touches[0].PointerInfo.PtPixelLocation.X = x;
                touches[0].PointerInfo.PtPixelLocation.Y = y;
                TouchInjection.Send(touches);
                break;
            case InputType.UP:
                touches[0].PointerInfo.PointerFlags = PointerFlags.UP | PointerFlags.INRANGE;
                touches[0].PointerInfo.PtPixelLocation.X = x;
                touches[0].PointerInfo.PtPixelLocation.Y = y;
                TouchInjection.Send(touches);
                pressing = false;
                break;
            case InputType.CANCEL:
                touches[0].PointerInfo.PointerFlags = PointerFlags.CANCELLED | PointerFlags.UP;
                touches[0].PointerInfo.PtPixelLocation.X = x;
                touches[0].PointerInfo.PtPixelLocation.Y = y;
                TouchInjection.Send(touches);
                pressing = false;
                break;
        }
    }
}
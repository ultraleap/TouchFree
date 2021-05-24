using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ultraleap.ScreenControl.Client;

public class LockCursor : InputActionPlugin
{
    public TransparentWindow window;
    private Vector2 screenMiddle;

    void Start()
    {
        screenMiddle = new Vector2(TouchFreeMain.CursorWindowSize / 2, TouchFreeMain.CursorWindowSize / 2);
    }

    protected override ClientInputAction? ModifyInputAction(ClientInputAction _inputAction)
    {
        if (window.clickThroughEnabled)
        {
            window.SetPosition(new Vector2(_inputAction.CursorPosition.x, _inputAction.CursorPosition.y));
            _inputAction.CursorPosition = screenMiddle;
        }

        return _inputAction;
    }
}
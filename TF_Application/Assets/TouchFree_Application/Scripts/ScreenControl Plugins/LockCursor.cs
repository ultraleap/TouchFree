using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ultraleap.TouchFree.Tooling;

namespace Ultraleap.TouchFree
{
    public class LockCursor : InputActionPlugin
    {
        public TransparentWindow window;

        protected override InputAction? ModifyInputAction(InputAction _inputAction)
        {
            if (TransparentWindow.clickThroughEnabled)
            {
                window.SetPosition(new Vector2(_inputAction.CursorPosition.x, _inputAction.CursorPosition.y));
                _inputAction.CursorPosition = TouchFreeMain.CursorWindowMiddle;
            }

            return _inputAction;
        }
    }
}
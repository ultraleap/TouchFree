using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ultraleap.TouchFree.Tooling;

namespace Ultraleap.TouchFree
{
    public class LockCursor : InputActionPlugin
    {
        public TransparentWindow window;
        
        bool useSecondMonitor = false;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                useSecondMonitor = !useSecondMonitor;
            }
        }

        protected override InputAction? ModifyInputAction(InputAction _inputAction)
        {
            if (TransparentWindow.clickThroughEnabled)
            {
                int x = (useSecondMonitor ? Display.main.systemWidth : 0) + (int)_inputAction.CursorPosition.x;
                int y = (int)_inputAction.CursorPosition.y;

                window.SetPosition(new Vector2(x, y));
                _inputAction.CursorPosition = TouchFreeMain.CursorWindowMiddle;
            }

            return _inputAction;
        }
    }
}
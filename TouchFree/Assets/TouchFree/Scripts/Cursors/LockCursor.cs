using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockCursor : MonoBehaviour
{
    public RectTransform cursorCanvas;
    
    public TransparentWindow window;

    private TouchFreeRingCursor cursor;
    private Vector2 screenMiddle;

    void Start()
    {
        screenMiddle = new Vector2(TouchFreeMain.CursorWindowSize / 2, TouchFreeMain.CursorWindowSize / 2);
    }

    void LateUpdate()
    {
        if (cursor == null)
        {
            cursor = cursorCanvas.GetComponentInChildren<TouchFreeRingCursor>();
        }

        if (window.clickThroughEnabled)
        {
            cursor.overriding = true;
            cursor.OverridePosition(screenMiddle);
            window.SetPosition(cursor.GetWindowPos());
        }
        else
        {
            cursor.overriding = false;
        }
    }
}
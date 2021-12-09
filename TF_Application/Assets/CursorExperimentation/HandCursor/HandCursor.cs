using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ultraleap.TouchFree.Tooling;
using Ultraleap.TouchFree.Tooling.Connection;
using Ultraleap.TouchFree.Tooling.Cursors;

public class HandCursor : TouchlessCursor
{
    bool cursorCanMove = true;

    void Start()
    {
        ConnectionManager.HandsLost += RecenterCursor;
        ConnectionManager.HandFound += FreeCursor;
    }

    void OnDestroy()
    {
        ConnectionManager.HandsLost -= RecenterCursor;
        ConnectionManager.HandFound -= FreeCursor;
    }

    protected override void HandleInputAction(InputAction _inputData)
    {
        if (cursorCanMove)
        {
            targetPos = _inputData.CursorPosition;
            cursorTransform.anchoredPosition = targetPos;
        }
    }

    public void FreeCursor()
    {
        cursorCanMove = true;
    }

    public void RecenterCursor()
    {
        cursorCanMove = false;

        targetPos = new Vector2(Screen.width / 2, Screen.height / 3);
        cursorTransform.anchoredPosition = targetPos;
    }
}
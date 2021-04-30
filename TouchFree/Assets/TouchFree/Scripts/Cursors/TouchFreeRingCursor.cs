using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ultraleap.ScreenControl.Client.Cursors;

public class TouchFreeRingCursor : DotCursor
{
    public bool overriding = false;
    protected Vector2 overridePosition;
    Vector2 windowPos;

    public override void UpdateCursor(Vector2 _screenPos, float _progressToClick)
    {
        base.UpdateCursor(_screenPos, _progressToClick);
        windowPos = targetPos;

        if (overriding)
        {
            targetPos = overridePosition;
        }
    }

    public virtual void OverridePosition(Vector2 _position)
    {
        overridePosition = _position;
    }

    public virtual Vector2 GetWindowPos()
    {
        return windowPos;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchFreeCursorManager : CursorManager
{
    bool active = true;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.V))
        {
            active = !active;
            SetCursorVisibility(active);
        }
    }
}
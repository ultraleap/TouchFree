using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ultraleap.TouchFree.Tooling;
using Ultraleap.TouchFree.Tooling.Connection;
using Ultraleap.TouchFree.Tooling.Cursors;
using System;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance;

    public static event Action<CursorType> CursorChanged;

    public InteractionCursor[] interactionCursors;
    public TouchlessCursor defaultCursor;

    [HideInInspector] public TouchlessCursor currentCursor;
    InteractionType currentInteractionType;
    bool setOnce = false;

    protected bool handsActive = false;

    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    protected virtual void OnEnable()
    {
        InputActionManager.TransmitInputAction += HandleInputAction;
        ConnectionManager.HandFound += HandFound;
        ConnectionManager.HandsLost += HandLost;
    }

    protected virtual void OnDisable()
    {
        InputActionManager.TransmitInputAction -= HandleInputAction;
        ConnectionManager.HandFound -= HandFound;
        ConnectionManager.HandsLost -= HandLost;
    }

    void HandFound()
    {
        handsActive = true;
    }

    void HandLost()
    {
        handsActive = false;
    }

    void HandleInputAction(InputAction _inputAction)
    {
        if(_inputAction.InteractionType != currentInteractionType || !setOnce)
        {
            setOnce = true;
            currentInteractionType = _inputAction.InteractionType;
            ChangeCursorForInteraction(_inputAction.InteractionType);
        }
    }

    void ChangeCursorForInteraction(InteractionType _interaction)
    {
        bool cursorSet = false;

        TouchlessCursor enabledCursor = null;

        foreach(var interactionCursor in interactionCursors)
        {
            if (interactionCursor.interaction != _interaction &&
                enabledCursor != interactionCursor.cursor)
            {
                interactionCursor.cursor.gameObject.SetActive(false);
            }
            else
            {
                enabledCursor = interactionCursor.cursor;
                currentCursor = enabledCursor;
                cursorSet = true;

                if(handsActive)
                {
                    currentCursor.ShowCursor();
                }
                else
                {
                    currentCursor.HideCursor();
                }

                CursorChanged?.Invoke(interactionCursor.cursorType);
            }
        }

        if(!cursorSet)
        {
            currentCursor = defaultCursor;
        }

        SetCursorVisibility(Ultraleap.TouchFree.ServiceShared.TFAppConfig.Config.cursorEnabled);
    }

    public void SetCursorVisibility(bool _setTo)
    {
        if (currentCursor != null)
        {
            currentCursor.gameObject.SetActive(_setTo);
        }
    }

    [System.Serializable]
    public struct InteractionCursor
    {
        public InteractionType interaction;
        public CursorType cursorType;
        public TouchlessCursor cursor;
    }

    public enum CursorType
    {
        FILL,
        RING,
    }
}
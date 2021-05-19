using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ultraleap.ScreenControl.Client;
using Ultraleap.ScreenControl.Client.Cursors;
using System;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance;

    public static event Action<CursorType> CursorChanged;

    public InteractionCursor[] interactionCursors;
    public GameObject defaultCursor;

    [HideInInspector] public GameObject currentCursor;
    InteractionType currentInteractionType = InteractionType.GRAB;
    bool setOnce = false;

    private void Start()
    {
        currentCursor = defaultCursor;

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void OnEnable()
    {
        InputActionManager.TransmitInputAction += HandleInputAction;
    }

    private void OnDisable()
    {
        InputActionManager.TransmitInputAction -= HandleInputAction;
    }

    void HandleInputAction(ClientInputAction _inputAction)
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

        GameObject enabledCursor = null;

        foreach(var interactionCursor in interactionCursors)
        {
            if (interactionCursor.interaction != _interaction &&
                enabledCursor != interactionCursor.cursor)
            {
                interactionCursor.cursor.SetActive(false);
            }
            else
            {
                interactionCursor.cursor.SetActive(true);
                enabledCursor = interactionCursor.cursor;
                currentCursor = enabledCursor;
                currentCursor.GetComponent<TouchlessCursor>().ShowCursor();
                cursorSet = true;

                CursorChanged?.Invoke(interactionCursor.cursorType);
            }
        }

        if(!cursorSet)
        {
            defaultCursor.SetActive(true);
            currentCursor = defaultCursor;
            currentCursor.GetComponent<TouchlessCursor>().ShowCursor();
        }
    }

    public void SetCursorVisibility(bool _setTo)
    {
        currentCursor.SetActive(_setTo);
    }

    [System.Serializable]
    public struct InteractionCursor
    {
        public InteractionType interaction;
        public CursorType cursorType;
        public GameObject cursor;
    }

    public enum CursorType
    {
        FILL,
        RING,
    }
}
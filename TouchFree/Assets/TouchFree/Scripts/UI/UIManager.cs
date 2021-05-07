using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[DefaultExecutionOrder(-1)]
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public event Action UIActivated;
    public event Action UIDeactivated;

    public bool isActive;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }

        Application.focusChanged += Application_focusChanged;
    }

    private void OnDestroy()
    {
        Application.focusChanged -= Application_focusChanged;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            SetUIActive(!isActive);
        }
    }

    private void Application_focusChanged(bool _focussed)
    {
        if (_focussed)
        {
            SetUIActive(true);
        }
    }

    private void SetUIActive(bool _setTo = true)
    {
        isActive = _setTo;

        if(isActive)
        {
            UIActivated?.Invoke();
        }
        else
        {
            UIDeactivated?.Invoke();
        }
    }
}
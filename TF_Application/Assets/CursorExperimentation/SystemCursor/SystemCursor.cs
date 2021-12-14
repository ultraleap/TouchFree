using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ultraleap.TouchFree.Tooling;
using Ultraleap.TouchFree.Tooling.Cursors;
using Ultraleap.TouchFree.Tooling.Connection;

using UnityEngine.UI;

public class SystemCursor : TouchlessCursor
{
    public Image fillImage;

    public AnimationCurve fillCurve;

    public GameObject standardCursorGameobject;
    public GameObject dragCursorGameobject;
    public GameObject handCursorGameObject;

    bool down = false;
    bool dragging = false;

    public float cursorPunchTime;
    public AnimationCurve cursorPunchScaleCurve;

    public float hideCursorDelay = 0.5f;

    bool hidingCursor = false;
    bool hidingAfterDelay = false;

    public RectTransform cursorScaler;

    public Ultraleap.TouchFree.TransparentWindow transparentWindow;

    protected override void OnEnable()
    {
        base.OnEnable();
        ConnectionManager.HandFound += ShowCursor;
        ConnectionManager.HandsLost += HideCursor;
        HideCursor();
        SetCursorLocalScale(cursorSize);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        ConnectionManager.HandFound -= ShowCursor;
        ConnectionManager.HandsLost -= HideCursor;
    }

    protected override void HandleInputAction(InputAction _inputData)
    {
        targetPos = _inputData.CursorPosition;

        if(fillImage != null)
        {
            fillImage.fillAmount = fillCurve.Evaluate(_inputData.ProgressToClick);
        }

        switch (_inputData.InputType)
        {
            case InputType.CANCEL:
                down = false;

                if (dragging)
                {
                    dragging = false;
                    ToggleCursor(true);
                }
                break;
            case InputType.DOWN:
                down = true;
                break;
            case InputType.MOVE:
                if(down && !dragging)
                {
                    dragging = true;
                    ToggleCursor(false);
                }
                break;
            case InputType.UP:
                down = false;

                if (dragging)
                {
                    dragging = false;
                    ToggleCursor(true);
                }
                break;
        }
    }

    public override void ShowCursor()
    {
        bool wasShowing = !hidingCursor;
        hidingCursor = false;

        if (wasShowing || hidingAfterDelay)
        {
            return;
        }

        standardCursorGameobject.SetActive(true);
        dragCursorGameobject.SetActive(false);
        handCursorGameObject.SetActive(false);

        if (currentCursorPunch == null)
        {
            currentCursorPunch = StartCoroutine(PunchCursor());
        }

        SetCursorLocalScale(cursorSize);
    }

    public override void HideCursor()
    {
        bool wasHiding = hidingCursor;
        hidingCursor = true;

        if (wasHiding)
        {
            return;
        }

        StartCoroutine(HideAfterDelay(hideCursorDelay));
    }

    Coroutine currentCursorPunch;

    IEnumerator PunchCursor()
    {
        float currentPunchTime = 0;

        while(currentPunchTime < cursorPunchTime)
        {
            cursorTransform.localScale = Vector3.one * cursorPunchScaleCurve.Evaluate(currentPunchTime / cursorPunchTime);

            currentPunchTime += Time.deltaTime;
            yield return null;
        }

        currentCursorPunch = null;
    }

    void ToggleCursor(bool _toStandard)
    {
        standardCursorGameobject.SetActive(_toStandard);
        dragCursorGameobject.SetActive(!_toStandard);
    }

    IEnumerator HideAfterDelay(float _timeToWait)
    {
        hidingAfterDelay = true;
        float currentWaitTime = 0;

        while (currentWaitTime < _timeToWait)
        {
            if (!hidingCursor)
            {
                hidingAfterDelay = false;
                yield break;
            }

            currentWaitTime += Time.deltaTime;
            yield return null;
        }

        hidingAfterDelay = false;

        standardCursorGameobject.SetActive(false);
        dragCursorGameobject.SetActive(false);
        handCursorGameObject.SetActive(true);

        targetPos = new Vector2(Screen.width / 2, Screen.height / 2);
        cursorTransform.anchoredPosition = targetPos;

        transparentWindow.SetPosition(new Vector2(Display.main.systemWidth/2, Display.main.systemHeight/2));
    }


    protected virtual void SetCursorLocalScale(float _scale)
    {
        Vector3 cursorLocalScale = new Vector3(_scale, _scale, _scale);
        cursorScaler.transform.localScale = cursorLocalScale;
    }
}
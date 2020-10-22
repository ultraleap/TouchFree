using System.Collections;
using UnityEngine;
using Ultraleap.ScreenControl.Client;

public class ProgressCursor : Ultraleap.ScreenControl.Client.TouchlessCursor
{
    [Header("Graphics")]
    public Transform cursorDotTransform;
    public Transform cursorProgressTransform;

    public UnityEngine.UI.Image cursorDot;
    public UnityEngine.UI.Image cursorDotFill;
    public UnityEngine.UI.Image cursorProgressBorder;
    public UnityEngine.UI.Image cursorProgressFill;

    [Header("Ring")]
    public bool ringEnabled;
    public UnityEngine.UI.Image cursorRing;
    public AnimationCurve ringCurve;

    [Header("Pulse")]
    public AnimationCurve pulseGrowCurve;
    public AnimationCurve pulseShrinkCurve;
    [Range(0.01f, 1f)] public float pulseSeconds;
    [Range(0.01f, 2f)] public float cursorDownScale;

    protected float cursorDotSize;

    protected bool shrunk = false;

    public AnimationCurve ringFillCurve;

    public override void UpdateCursor(Vector2 screenPos, float progressToClick)
    {
        _targetPos = screenPos;

        cursorProgressFill.fillAmount = ringFillCurve.Evaluate(progressToClick);
        cursorProgressBorder.fillAmount = ringFillCurve.Evaluate(progressToClick);

        if (ringEnabled)
        {
            if (!hidingCursor)
            {
                cursorRing.enabled = true;
            }
            else
            {
                cursorRing.enabled = false;
            }
        }
    }

    protected override void HandleInputAction(Ultraleap.ScreenControl.Client.ScreenControlTypes.ClientInputAction _inputData)
    {
        Ultraleap.ScreenControl.Client.ScreenControlTypes.InputType _type = _inputData.Type;
        Vector2 _cursorPosition = _inputData.CursorPosition;

        switch (_type)
        {
            case Ultraleap.ScreenControl.Client.ScreenControlTypes.InputType.MOVE:
                UpdateCursor(_cursorPosition, _inputData.ProgressToClick);
                break;
            case Ultraleap.ScreenControl.Client.ScreenControlTypes.InputType.DOWN:
                if (!shrunk)
                {
                    if (cursorScalingRoutine != null)
                        StopCoroutine(cursorScalingRoutine);

                    cursorScalingRoutine = StartCoroutine(ShrinkCursorDot());
                }
                break;
            case Ultraleap.ScreenControl.Client.ScreenControlTypes.InputType.UP:
                if (shrunk)
                {
                    if (cursorScalingRoutine != null)
                        StopCoroutine(cursorScalingRoutine);

                    cursorScalingRoutine = StartCoroutine(GrowCursorDot());
                }
                break;
            case Ultraleap.ScreenControl.Client.ScreenControlTypes.InputType.CANCEL:
                break;
        }
    }

    protected override void InitialiseCursor()
    {
        cursorDotSize = cursorSize;
        var dotSizeIsZero = Mathf.Approximately(cursorDotSize, 0f);
        cursorDotSize = dotSizeIsZero ? 1f : cursorDotSize;

        // Scale up the dot by a factor of 2 for this interaction due to the smaller dot
        cursorDotSize *= 2f;

        cursorDot.enabled = !dotSizeIsZero;
        cursorProgressBorder.enabled = !dotSizeIsZero;
        cursorProgressFill.enabled = !dotSizeIsZero;
        cursorDotTransform.localScale = new Vector3(cursorDotSize, cursorDotSize, cursorDotSize);
        cursorProgressTransform.localScale = new Vector3(cursorDotSize, cursorDotSize, cursorDotSize);
    }

    Coroutine cursorScalingRoutine;
    public IEnumerator GrowCursorDot()
    {
        SetCursorDotLocalScale(cursorDownScale * cursorDotSize);
        shrunk = false;
        YieldInstruction yieldInstruction = new YieldInstruction();
        float elapsedTime = 0.0f;

        while (elapsedTime < pulseSeconds)
        {
            yield return yieldInstruction;
            elapsedTime += Time.deltaTime;
            float scale = Utilities.MapRangeToRange(pulseGrowCurve.Evaluate(elapsedTime / pulseSeconds), 0, 1, cursorDownScale * cursorDotSize, cursorDotSize);
            SetCursorDotLocalScale(scale);
        }

        SetCursorDotLocalScale(cursorDotSize);
        cursorScalingRoutine = null;
    }

    public IEnumerator ShrinkCursorDot()
    {
        shrunk = true;
        YieldInstruction yieldInstruction = new YieldInstruction();
        float elapsedTime = 0.0f;

        while (elapsedTime < pulseSeconds)
        {
            yield return yieldInstruction;
            elapsedTime += Time.deltaTime;
            float scale = Utilities.MapRangeToRange(pulseShrinkCurve.Evaluate(elapsedTime / pulseSeconds), 0, 1, cursorDownScale * cursorDotSize, cursorDotSize);
            SetCursorDotLocalScale(scale);
        }

        SetCursorDotLocalScale(cursorDownScale * cursorDotSize);
        cursorScalingRoutine = null;
    }

    private void SetCursorDotLocalScale(float scale)
    {
        cursorDotTransform.localScale = new Vector3(scale, scale, scale);
    }

    public override void ShowCursor()
    {
        base.ShowCursor();
        cursorDot.enabled = true;
        cursorDotFill.enabled = true;
        cursorProgressBorder.enabled = true;
        cursorProgressFill.enabled = true;

        if (ringEnabled)
        {
            cursorRing.enabled = true;
        }
    }

    public override void HideCursor()
    {
        base.HideCursor();
        cursorDot.enabled = false;
        cursorDotFill.enabled = false;
        cursorProgressBorder.enabled = false;
        cursorProgressFill.enabled = false;

        if (ringEnabled)
        {
            cursorRing.enabled = false;
        }
    }
}
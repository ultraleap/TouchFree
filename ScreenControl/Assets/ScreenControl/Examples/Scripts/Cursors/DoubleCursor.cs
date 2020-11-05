using System.Collections;
using UnityEngine;
using Ultraleap.ScreenControl.Client;

public class DoubleCursor : TouchlessCursor
{
    [Header("Controls")]
    public Ultraleap.ScreenControl.Client.ScreenControlTypes.InteractionType moveInteraction = Ultraleap.ScreenControl.Client.ScreenControlTypes.InteractionType.PUSH;
    public Ultraleap.ScreenControl.Client.ScreenControlTypes.InteractionType clickInteraction = Ultraleap.ScreenControl.Client.ScreenControlTypes.InteractionType.GRAB;

    [Header("Graphics")]
    public UnityEngine.UI.Image cursorDot;
    public UnityEngine.UI.Image cursorDotFill;

    [Header("Ring")]
    public bool ringEnabled;
    public UnityEngine.UI.Image cursorRing;
    public AnimationCurve ringCurve;
    public float cursorMaxRingSize = 2;

    [Header("Pulse")]
    public AnimationCurve pulseGrowCurve;
    public AnimationCurve pulseShrinkCurve;
    [Range(0.01f, 1f)] public float pulseSeconds;
    [Range(0.01f, 2f)] public float cursorDownScale;

    protected float cursorDotSize;
    protected float maxRingScale;
    public float screenDistanceAtMaxScaleMeters;

    protected Color ringColor;

    protected bool dotShrunk = false;

    public override void UpdateCursor(Vector2 screenPos, float progressToClick)
    {
        _targetPos = screenPos;

        var dist = progressToClick;
        dist = Mathf.InverseLerp(0, screenDistanceAtMaxScaleMeters, dist);
        dist = Mathf.Lerp(0, 1, dist);

        cursorRing.color = new Color(cursorRing.color.r, cursorRing.color.g, cursorRing.color.b, Mathf.Lerp(ringColor.a, 0f, dist));

        if (ringEnabled && !hidingCursor)
        {
            cursorRing.enabled = true;
            var ringScale = Mathf.Lerp(1f, maxRingScale, ringCurve.Evaluate(dist));
            cursorRing.transform.localScale = Vector3.one * ringScale;
        }
        else
        {
            cursorRing.enabled = false;
        }
    }

    protected override void HandleInputAction(Ultraleap.ScreenControl.Client.ScreenControlTypes.ClientInputAction _inputData)
    {
        if (_inputData.InputType == Ultraleap.ScreenControl.Client.ScreenControlTypes.InputType.MOVE)
        {
            if (_inputData.InteractionType == moveInteraction)
            {
                UpdateCursor(_inputData.CursorPosition, _inputData.ProgressToClick);
            }
        }

        if (_inputData.InteractionType == clickInteraction)
        {
            switch (_inputData.InputType)
            {
                case Ultraleap.ScreenControl.Client.ScreenControlTypes.InputType.DOWN:
                    if (!dotShrunk)
                    {
                        if (cursorScalingRoutine != null)
                            StopCoroutine(cursorScalingRoutine);

                        cursorScalingRoutine = StartCoroutine(ShrinkCursorDot());
                    }
                    break;
                case Ultraleap.ScreenControl.Client.ScreenControlTypes.InputType.UP:
                    if (dotShrunk)
                    {
                        if (cursorScalingRoutine != null)
                            StopCoroutine(cursorScalingRoutine);

                        cursorScalingRoutine = StartCoroutine(GrowCursorDot());
                    }
                    break;
                case Ultraleap.ScreenControl.Client.ScreenControlTypes.InputType.MOVE:
                case Ultraleap.ScreenControl.Client.ScreenControlTypes.InputType.CANCEL:
                    break;
            }
        }

    }

    protected override void InitialiseCursor()
    {
        if (ringEnabled)
        {
            ringColor = cursorRing.color;
        }

        cursorDotSize = cursorSize;
        var dotSizeIsZero = Mathf.Approximately(cursorDotSize, 0f);
        cursorDotSize = dotSizeIsZero ? 1f : cursorDotSize;
        cursorDot.enabled = !dotSizeIsZero;
        cursorDot.transform.localScale = new Vector3(cursorDotSize, cursorDotSize, cursorDotSize);

        maxRingScale = (1f / cursorDotSize) * cursorMaxRingSize;
    }

    Coroutine cursorScalingRoutine;
    public IEnumerator GrowCursorDot()
    {
        SetCursorDotLocalScale(cursorDownScale * cursorDotSize);
        dotShrunk = false;
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
        dotShrunk = true;
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
        cursorDot.transform.localScale = new Vector3(scale, scale, scale);
    }

    public override void ShowCursor()
    {
        base.ShowCursor();
        cursorDot.enabled = true;
        cursorDotFill.enabled = true;
        cursorRing.enabled = true;
    }

    public override void HideCursor()
    {
        base.HideCursor();
        cursorDot.enabled = false;
        cursorDotFill.enabled = false;
        cursorRing.enabled = false;
    }
}

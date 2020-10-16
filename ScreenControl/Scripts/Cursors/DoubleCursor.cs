using System.Collections;
using UnityEngine;

public class DoubleCursor : Cursor
{
    [Header("Controls")]
    public InteractionType moveInteraction = InteractionType.Push;
    public InteractionType clickInteraction = InteractionType.Grab;

    [Header("Graphics")]
    public UnityEngine.UI.Image cursorDot;
    public UnityEngine.UI.Image cursorDotFill;

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
    protected float maxRingScale;
    protected float screenDistanceAtMaxScaleMeters;

    protected Color dotFillColor;
    protected Color dotBorderColor;
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

    protected override void OnHandleInputAction(InputActionData _inputData)
    {
        if (_inputData.Type == InputType.MOVE)
        {
            if (_inputData.Source == moveInteraction)
            {
                UpdateCursor(_inputData.CursorPosition, _inputData.ProgressToClick);
            }
        }

        if (_inputData.Source == clickInteraction)
        {
            switch (_inputData.Type)
            {
                case InputType.DOWN:
                    if (!dotShrunk)
                    {
                        if (cursorScalingRoutine != null)
                            StopCoroutine(cursorScalingRoutine);

                        cursorScalingRoutine = StartCoroutine(ShrinkCursorDot());
                    }
                    break;
                case InputType.UP:
                case InputType.HOVER:
                    if (dotShrunk)
                    {
                        if (cursorScalingRoutine != null)
                            StopCoroutine(cursorScalingRoutine);

                        cursorScalingRoutine = StartCoroutine(GrowCursorDot());
                    }
                    break;
                case InputType.MOVE:
                case InputType.CANCEL:
                    break;
            }
        }
        
    }

    protected override void OnConfigUpdated()
    {
        dotFillColor = ScreenControlUtility.ParseColor(SettingsConfig.Config.CursorDotFillColor, SettingsConfig.Config.CursorDotFillOpacity);
        dotBorderColor = ScreenControlUtility.ParseColor(SettingsConfig.Config.CursorDotBorderColor, SettingsConfig.Config.CursorDotBorderOpacity);
        ringColor = ScreenControlUtility.ParseColor(SettingsConfig.Config.CursorRingColor, SettingsConfig.Config.CursorRingOpacity);

        cursorDot.color = dotBorderColor;
        cursorDotFill.color = dotFillColor;

        if (ringEnabled)
        {
            cursorRing.color = ringColor;
        }

        screenDistanceAtMaxScaleMeters = SettingsConfig.Config.CursorMaxRingScaleAtDistanceM;
        
        cursorDotSize = (GlobalSettings.ScreenHeight / PhysicalConfigurable.Config.ScreenHeightM) * SettingsConfig.Config.CursorDotSizeM / 100f;
        var dotSizeIsZero = Mathf.Approximately(cursorDotSize, 0f);
        cursorDotSize = dotSizeIsZero ? 1f : cursorDotSize;
        cursorDot.enabled = !dotSizeIsZero;
        cursorDot.transform.localScale = new Vector3(cursorDotSize, cursorDotSize, cursorDotSize);

        maxRingScale = (1f / cursorDotSize) * SettingsConfig.Config.CursorRingMaxScale;
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
            float scale = ScreenControlUtility.MapRangeToRange(pulseGrowCurve.Evaluate(elapsedTime / pulseSeconds), 0, 1, cursorDownScale * cursorDotSize, cursorDotSize);
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
            float scale = ScreenControlUtility.MapRangeToRange(pulseShrinkCurve.Evaluate(elapsedTime / pulseSeconds), 0, 1, cursorDownScale * cursorDotSize, cursorDotSize);
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
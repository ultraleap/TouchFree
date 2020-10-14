using System.Collections;
using UnityEngine;

public class ProgressCursor : Cursor
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
    protected float maxRingScale;
    protected float screenDistanceAtMaxScaleMeters;

    protected Color dotFillColor;
    protected Color dotBorderColor;
    protected Color ringColor;

    protected bool shrunk = false;

    private ProgressTimer progressTimer;

    public AnimationCurve ringFillCurve;

    void Start()
    {
        progressTimer = FindObjectOfType<ProgressTimer>();
    }

    public override void UpdateCursor(Vector2 screenPos, float distanceFromScreen)
    {
        _targetPos = screenPos;

        float progress = progressTimer.Progress;

        cursorProgressFill.fillAmount = ringFillCurve.Evaluate(progress);
        cursorProgressBorder.fillAmount = ringFillCurve.Evaluate(progress);

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

    protected override void OnHandleInputAction(InputActionData _inputData)
    {
        InputType _type = _inputData.Type;
        Vector2 _cursorPosition = _inputData.CursorPosition;
        Vector2 _clickPosition = _inputData.ClickPosition;
        float _distanceFromScreen = _inputData.ProgressToClick;
        
        switch (_type)
        {
            case InputType.MOVE:
                UpdateCursor(_cursorPosition, _distanceFromScreen);
                break;
            case InputType.DOWN:
                if (!shrunk)
                {
                    if (cursorScalingRoutine != null)
                        StopCoroutine(cursorScalingRoutine);

                    cursorScalingRoutine = StartCoroutine(ShrinkCursorDot());
                }
                break;
            case InputType.UP:
            case InputType.HOVER:
                if (shrunk)
                {
                    if (cursorScalingRoutine != null)
                        StopCoroutine(cursorScalingRoutine);

                    cursorScalingRoutine = StartCoroutine(GrowCursorDot());
                }
                break;
            case InputType.CANCEL:
                break;
        }
    }

    protected override void OnConfigUpdated()
    {
        dotFillColor = ScreenControlUtility.ParseColor(SettingsConfig.Config.CursorDotFillColor, SettingsConfig.Config.CursorDotFillOpacity);
        dotBorderColor = ScreenControlUtility.ParseColor(SettingsConfig.Config.CursorDotBorderColor, SettingsConfig.Config.CursorDotBorderOpacity);
        ringColor = ScreenControlUtility.ParseColor(SettingsConfig.Config.CursorRingColor, SettingsConfig.Config.CursorRingOpacity);

        cursorDot.color = dotBorderColor;
        cursorDotFill.color = dotFillColor;
        cursorProgressBorder.color = dotBorderColor;
        cursorProgressFill.color = ringColor;

        if (ringEnabled)
        {
            cursorRing.color = ringColor;
        }

        screenDistanceAtMaxScaleMeters = SettingsConfig.Config.CursorMaxRingScaleAtDistanceM;

        cursorDotSize = (GlobalSettings.ScreenHeight / PhysicalConfigurable.Config.ScreenHeightM) * SettingsConfig.Config.CursorDotSizeM / 100f;
        var dotSizeIsZero = Mathf.Approximately(cursorDotSize, 0f);
        cursorDotSize = dotSizeIsZero ? 1f : cursorDotSize;

        // Scale up the dot by a factor of 2 for this interaction due to the smaller dot
        cursorDotSize *= 2f;

        cursorDot.enabled = !dotSizeIsZero;
        cursorProgressBorder.enabled = !dotSizeIsZero;
        cursorProgressFill.enabled = !dotSizeIsZero;
        cursorDotTransform.localScale = new Vector3(cursorDotSize, cursorDotSize, cursorDotSize);
        cursorProgressTransform.localScale = new Vector3(cursorDotSize, cursorDotSize, cursorDotSize);

        maxRingScale = (1f / cursorDotSize) * SettingsConfig.Config.CursorRingMaxScale;
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
            float scale = ScreenControlUtility.MapRangeToRange(pulseGrowCurve.Evaluate(elapsedTime / pulseSeconds), 0, 1, cursorDownScale * cursorDotSize, cursorDotSize);
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
            float scale = ScreenControlUtility.MapRangeToRange(pulseShrinkCurve.Evaluate(elapsedTime / pulseSeconds), 0, 1, cursorDownScale * cursorDotSize, cursorDotSize);
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
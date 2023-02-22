using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ultraleap.TouchFree.Tooling.Cursors;
using Ultraleap.TouchFree.Tooling;
using Ultraleap.TouchFree.Tooling.Connection;

public class FillCursor : TouchlessCursor
{
    // Variable: fadeDuration
    // The number of frames over which the cursor should fade when appearing/disappearing
    [Range(0f, 60f)] public float fadeDuration = 30;

    // Variable: cursorBorder
    // The image of the border around the dot, this is the parent image in the prefab and is
    //  used to do all of the scaling of the images that make up this cursor.
    [Header("Graphics")]
    public Image cursorBorder;

    // Variable: cursorFill
    // The main background image of the Cursor, used for fading the image out.
    public Image cursorFill;

    public Image fillRingImage;

    // Variable: pulseShrinkCurve
    // When a "click" is recognised, an animation plays where the dot "pulses" (briefly
    // shrinking and expanding). This AnimationCurve governs the shrinking that follows a
    // "DOWN" input.
    [Header("Pulse")]
    public AnimationCurve pulseShrinkCurve;

    // Variable: pulseGrowCurve
    // When a "click" is recognised, an animation plays where the dot "pulses" (briefly
    // shrinking and expanding). This AnimationCurve governs the growing back to full size
    // that follows a "UP" input.
    public AnimationCurve pulseGrowCurve;

    // Variable: pulseSeconds
    // The amount of time each part of the pulse should last (a pulse will last twice this long)
    [Range(0.01f, 1f)] public float pulseSeconds;

    // Variable: cursorDownScale
    // The scale of the cursor's dot at its smallest size (between the shrink and the grow )
    [Range(0.01f, 2f)] public float cursorDownScale;

    Coroutine cursorScalingRoutine;

    protected bool hidingCursor = true;
    protected bool growQueued = false;
    protected Vector3 cursorLocalScale = Vector3.one;

    // Function: OnDisable
    // This override of Unity's OnDisable feature of MonoBehaviour does the teardown of this
    // Cursor when it is disabled.
    protected override void OnDisable()
    {
        base.OnDisable();

        ConnectionManager.HandFound -= ShowCursor;
        ConnectionManager.HandsLost -= HideCursor;
    }

    // Function: InitialiseCursor
    // This override ensures that the DotCursor is properly set up with relative scales and
    // sorting orders for the ring sprites.
    protected override void InitialiseCursor()
    {
        ConnectionManager.HandFound += ShowCursor;
        ConnectionManager.HandsLost += HideCursor;

        SetColors(primaryColor, secondaryColor, tertiaryColor);

        bool dotSizeIsZero = Mathf.Approximately(cursorSize, 0f);
        cursorSize = dotSizeIsZero ? 1f : cursorSize;
        cursorBorder.transform.localScale = new Vector3(cursorSize, cursorSize, cursorSize);
        SetCursorLocalScale(cursorSize);
    }

    protected override void HandleInputAction(InputAction _inputData)
    {
        base.HandleInputAction(_inputData);

        fillRingImage.fillAmount = _inputData.ProgressToClick;

        switch (_inputData.InputType)
        {
            case InputType.DOWN:
                growQueued = false;
                if (cursorScalingRoutine != null)
                {
                    StopCoroutine(cursorScalingRoutine);
                }
                cursorScalingRoutine = StartCoroutine(ShrinkCursorDot());
                break;

            case InputType.UP:
                if (cursorScalingRoutine != null)
                {
                    growQueued = true;
                }
                else
                {
                    growQueued = false;
                    cursorScalingRoutine = StartCoroutine(GrowCursorDot());
                }
                break;

            case InputType.CANCEL:
                HideCursor();
                break;
        }

        if (hidingCursor && _inputData.InputType != InputType.CANCEL)
        {
            ShowCursor();
        }
    }

    // Function: SetColors
    // This override ensures the correct cursor UI elemets are coloured correctly when new
    // colours are set.
    public override void SetColors(Color _primary, Color _secondary, Color _tertiary)
    {
        _primaryColor = _primary;
        _secondaryColor = _secondary;
        _tertiaryColor = _tertiary;

        cursorFill.color = _primaryColor;
        fillRingImage.color = _secondaryColor;
        cursorBorder.color = _tertiaryColor;
    }

    // Function: ShowCursor
    // This override replaces the basic functionality of the <TouchlessCursor> to ensure that the cursor is faded smoothly
    // when being set to show.
    public override void ShowCursor()
    {
        bool wasShowing = !hidingCursor;
        hidingCursor = false;

        if (wasShowing)
        {
            return;
        }

        ResetCursor();
        StartCoroutine(FadeCursor(0, 1, fadeDuration));
        cursorBorder.enabled = true;
        cursorFill.enabled = true;
        fillRingImage.enabled = true;
    }

    // Function: HideCursor
    // This override replaces the basic functionality of the <TouchlessCursor> to ensure that the cursor is faded smoothly
    // when being set to hide.
    public override void HideCursor()
    {
        bool wasHiding = hidingCursor;
        hidingCursor = true;

        if (wasHiding)
        {
            return;
        }

        ResetCursor();
        StartCoroutine(FadeCursor(1, 0, fadeDuration, true));
    }

    // Group: Coroutine Functions

    // Function: GrowCursorDot
    // This coroutine smoothly expands the cursor dots size.
    public virtual IEnumerator GrowCursorDot()
    {
        SetCursorLocalScale(cursorDownScale * cursorSize);

        YieldInstruction yieldInstruction = new YieldInstruction();
        float elapsedTime = 0.0f;

        while (elapsedTime < pulseSeconds)
        {
            yield return yieldInstruction;
            elapsedTime += Time.deltaTime;

            float scale = Utilities.MapRangeToRange(pulseGrowCurve.Evaluate(elapsedTime / pulseSeconds), 0, 1, cursorDownScale * cursorSize, cursorSize);
            SetCursorLocalScale(scale);
        }

        SetCursorLocalScale(cursorSize);
        cursorScalingRoutine = null;
    }

    // Function: ShrinkCursorDot
    // This coroutine smoothly contracts the cursor dots size.
    public virtual IEnumerator ShrinkCursorDot()
    {
        YieldInstruction yieldInstruction = new YieldInstruction();
        float elapsedTime = 0.0f;

        while (elapsedTime < pulseSeconds)
        {
            yield return yieldInstruction;
            elapsedTime += Time.deltaTime;

            float scale = Utilities.MapRangeToRange(pulseShrinkCurve.Evaluate(elapsedTime / pulseSeconds), 0, 1, cursorDownScale * cursorSize, cursorSize);
            SetCursorLocalScale(scale);
        }

        SetCursorLocalScale(cursorDownScale * cursorSize);
        cursorScalingRoutine = null;

        if (growQueued)
        {
            growQueued = false;
            cursorScalingRoutine = StartCoroutine(GrowCursorDot());
        }
    }

    // Function: FadeCursor
    // This coroutine smoothly fades the cursors colours.
    protected virtual IEnumerator FadeCursor(float _from, float _to, float _duration, bool _disableOnEnd = false)
    {
        for (int i = 0; i < _duration; i++)
        {
            yield return null;
            float a = Mathf.Lerp(_from, _to, i / _duration);

            cursorBorder.color = new Color()
            {
                r = cursorBorder.color.r,
                g = cursorBorder.color.g,
                b = cursorBorder.color.b,
                a = a * cursorBorder.color.a
            };

            cursorFill.color = new Color()
            {
                r = cursorFill.color.r,
                g = cursorFill.color.g,
                b = cursorFill.color.b,
                a = a * cursorFill.color.a
            };

            fillRingImage.color = new Color()
            {
                r = fillRingImage.color.r,
                g = fillRingImage.color.g,
                b = fillRingImage.color.b,
                a = a * fillRingImage.color.a
            };
        };

        if (_disableOnEnd)
        {
            SetCursorLocalScale(cursorSize);

            cursorBorder.enabled = false;
            cursorFill.enabled = false;
            fillRingImage.enabled = false;
        }
    }

    // Function: SetCursorLocalScale
    // This function resizes the cursor and its border.
    protected virtual void SetCursorLocalScale(float _scale)
    {
        cursorLocalScale = new Vector3(_scale, _scale, _scale);
        cursorBorder.transform.localScale = cursorLocalScale;
    }


    // Function: SetRingThickness
    // Used to set the <cursorRingThickness> value. Overridden to clamp between
    // <minRingThickness> and <maxRingThickness>.
    public override void SetRingThickness(float _thickness)
    {
        cursorRingThickness = Utilities.MapRangeToRange(_thickness, 0, 1, minRingThickness, maxRingThickness);
        fillRingImage.transform.localScale = Vector3.one * cursorRingThickness;
    }

    // Function: ResetCursor
    // This function stops all scaling coroutines and clears their related variables.
    public virtual void ResetCursor()
    {
        StopAllCoroutines();
        cursorScalingRoutine = null;
        growQueued = false;

        SetCursorLocalScale(cursorSize);
    }
}
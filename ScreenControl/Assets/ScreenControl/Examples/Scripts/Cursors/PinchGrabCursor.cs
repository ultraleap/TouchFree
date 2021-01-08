using UnityEngine;
using Ultraleap.ScreenControl.Client.Cursors;

public class PinchGrabCursor : TouchlessCursor
{
    [Header("Graphics")]
    public UnityEngine.UI.Image cursorDot;
    public UnityEngine.UI.Image cursorDotFill;

    protected float cursorDotSize;
    protected bool hidingCursor;

    private Ultraleap.ScreenControl.Client.ScreenControlTypes.HandChirality cursorChirality = Ultraleap.ScreenControl.Client.ScreenControlTypes.HandChirality.RIGHT;

    [Header("HandGraphics")]
    public UnityEngine.UI.Image openHandImage;
    public UnityEngine.UI.Image closedHandImage;

    protected override void HandleInputAction(Ultraleap.ScreenControl.Client.ScreenControlTypes.ClientInputAction _inputData)
    {
        Ultraleap.ScreenControl.Client.ScreenControlTypes.InputType type = _inputData.InputType;
        Vector2 cursorPosition = _inputData.CursorPosition;
        float distanceFromScreen = _inputData.ProgressToClick;

        if (_inputData.Chirality != cursorChirality)
        {
            cursorDot.transform.Rotate(0f, 180f, 0f);
            cursorChirality = _inputData.Chirality;
        }

        switch (type)
        {
            case Ultraleap.ScreenControl.Client.ScreenControlTypes.InputType.MOVE:
                _targetPos = cursorPosition;
                break;
            case Ultraleap.ScreenControl.Client.ScreenControlTypes.InputType.DOWN:
                openHandImage.enabled = false;
                closedHandImage.enabled = true;
                break;
            case Ultraleap.ScreenControl.Client.ScreenControlTypes.InputType.UP:
            case Ultraleap.ScreenControl.Client.ScreenControlTypes.InputType.CANCEL:
                openHandImage.enabled = true;
                closedHandImage.enabled = false;
                break;
        }
    }

    protected override void InitialiseCursor()
    {
        cursorDotSize = cursorSize;
        var dotSizeIsZero = Mathf.Approximately(cursorDotSize, 0f);
        cursorDotSize = dotSizeIsZero ? 1f : cursorDotSize;
        cursorDot.enabled = !dotSizeIsZero;

        if (!hidingCursor)
        {
            cursorDot.transform.localScale = new Vector3(cursorDotSize, cursorDotSize, cursorDotSize);
        }
    }

    public override void ShowCursor()
    {
        hidingCursor = false;
        cursorDot.enabled = true;
        cursorDotFill.enabled = true;
        openHandImage.enabled = true;
        closedHandImage.enabled = true;
    }

    public override void HideCursor()
    {
        hidingCursor = true;
        cursorDot.enabled = false;
        cursorDotFill.enabled = false;
        openHandImage.enabled = false;
        closedHandImage.enabled = false;
    }
}
using UnityEngine;
using Ultraleap.ScreenControl.Client;
using Ultraleap.ScreenControl.Client.Cursors;

public class PinchGrabCursor : TouchlessCursor
{
    [Header("Graphics")]
    public UnityEngine.UI.Image cursorDot;
    public UnityEngine.UI.Image cursorDotFill;

    public float cursorDotSize = 0.25f;
    protected bool hidingCursor;

    private HandChirality cursorChirality = HandChirality.RIGHT;

    [Header("HandGraphics")]
    public UnityEngine.UI.Image openHandImage;
    public UnityEngine.UI.Image closedHandImage;

    protected override void HandleInputAction(ClientInputAction _inputData)
    {
        InputType type = _inputData.InputType;
        Vector2 cursorPosition = _inputData.CursorPosition;
        float distanceFromScreen = _inputData.ProgressToClick;

        if (_inputData.Chirality != cursorChirality)
        {
            cursorDot.transform.Rotate(0f, 180f, 0f);
            cursorChirality = _inputData.Chirality;
        }

        switch (type)
        {
            case InputType.MOVE:
                _targetPos = cursorPosition;
                break;
            case InputType.DOWN:
                openHandImage.enabled = false;
                closedHandImage.enabled = true;
                break;
            case InputType.UP:
            case InputType.CANCEL:
                openHandImage.enabled = true;
                closedHandImage.enabled = false;
                break;
        }
    }

    protected override void InitialiseCursor()
    {
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
using UnityEngine;
using Ultraleap.ScreenControl.Client;

public class PinchGrabCursor : Ultraleap.ScreenControl.Client.Cursor
{
    [Header("Graphics")]
    public UnityEngine.UI.Image cursorDot;
    public UnityEngine.UI.Image cursorDotFill;

    protected float cursorDotSize;
    protected Color dotFillColor;
    protected Color dotBorderColor;

    private Ultraleap.ScreenControl.Client.ScreenControlTypes.HandChirality cursorChirality = Ultraleap.ScreenControl.Client.ScreenControlTypes.HandChirality.RIGHT;

    [Header("HandGraphics")]
    public UnityEngine.UI.Image openHandImage;
    public UnityEngine.UI.Image closedHandImage;

    protected override void OnHandleInputAction(Ultraleap.ScreenControl.Client.ScreenControlTypes.ClientInputAction _inputData)
    {
        Ultraleap.ScreenControl.Client.ScreenControlTypes.InputType type = _inputData.Type;
        Vector2 cursorPosition = _inputData.CursorPosition;
        float distanceFromScreen = _inputData.ProgressToClick;

        if (_inputData.Chirality != cursorChirality && _inputData.Chirality != Ultraleap.ScreenControl.Client.ScreenControlTypes.HandChirality.UNKNOWN)
        {
            cursorDot.transform.Rotate(0f, 180f, 0f);
            cursorChirality = _inputData.Chirality;
        }

        switch (type)
        {
            case Ultraleap.ScreenControl.Client.ScreenControlTypes.InputType.MOVE:
                UpdateCursor(cursorPosition, distanceFromScreen);
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

    protected override void OnConfigUpdated()
    {
        dotFillColor = Utilities.ParseColor(ClientSettings.clientConstants.CursorDotFillColor, ClientSettings.clientConstants.CursorDotFillOpacity);
        dotBorderColor = Utilities.ParseColor(ClientSettings.clientConstants.CursorDotBorderColor, ClientSettings.clientConstants.CursorDotBorderOpacity);

        openHandImage.color = Utilities.ParseColor(ClientSettings.clientConstants.CursorRingColor, ClientSettings.clientConstants.CursorRingOpacity);
        closedHandImage.color = Utilities.ParseColor(ClientSettings.clientConstants.CursorRingColor, ClientSettings.clientConstants.CursorRingOpacity);

        cursorDot.color = dotBorderColor;
        cursorDotFill.color = dotFillColor;

        cursorDotSize = ClientSettings.clientConstants.CursorDotSizePixels;
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
        base.ShowCursor();
        cursorDot.enabled = true;
        cursorDotFill.enabled = true;
        openHandImage.enabled = true;
        closedHandImage.enabled = true;
    }

    public override void HideCursor()
    {
        base.HideCursor();
        cursorDot.enabled = false;
        cursorDotFill.enabled = false;
        openHandImage.enabled = false;
        closedHandImage.enabled = false;
    }
}
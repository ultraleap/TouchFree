using UnityEngine;

public class PinchGrabCursor : Ultraleap.ScreenControl.Client.Cursors.Cursor
{
    [Header("Graphics")]
    public UnityEngine.UI.Image cursorDot;
    public UnityEngine.UI.Image cursorDotFill;

    protected float cursorDotSize;
    protected Color dotFillColor;
    protected Color dotBorderColor;

    private HandChirality cursorChirality = HandChirality.RIGHT;

    [Header("HandGraphics")]
    public UnityEngine.UI.Image openHandImage;
    public UnityEngine.UI.Image closedHandImage;

    protected override void OnHandleInputAction(InputActionData _inputData)
    {
        InputType _type = _inputData.Type;
        Vector2 _cursorPosition = _inputData.CursorPosition;
        Vector2 _clickPosition = _inputData.ClickPosition;
        float _distanceFromScreen = _inputData.ProgressToClick;

        if(_inputData.Chirality != cursorChirality && _inputData.Chirality != HandChirality.UNKNOWN)
        {
            cursorDot.transform.Rotate(0f, 180f, 0f);
            cursorChirality = _inputData.Chirality;
        }

        switch (_type)
        {
            case InputType.MOVE:
                UpdateCursor(_cursorPosition, _distanceFromScreen);
                break;
            case InputType.DOWN:
            case InputType.HOLD:
                openHandImage.enabled = false;
                closedHandImage.enabled = true;
                break;

            case InputType.DRAG:
                openHandImage.enabled = false;
                closedHandImage.enabled = true;
                break;

            case InputType.UP:
            case InputType.HOVER:
            case InputType.CANCEL:
                openHandImage.enabled = true;
                closedHandImage.enabled = false;
                break;
        }
    }

    protected override void OnConfigUpdated()
    {
        dotFillColor = ScreenControlUtility.ParseColor(SettingsConfig.Config.CursorDotFillColor, SettingsConfig.Config.CursorDotFillOpacity);
        dotBorderColor = ScreenControlUtility.ParseColor(SettingsConfig.Config.CursorDotBorderColor, SettingsConfig.Config.CursorDotBorderOpacity);

        openHandImage.color = ScreenControlUtility.ParseColor(SettingsConfig.Config.CursorRingColor, SettingsConfig.Config.CursorRingOpacity);
        closedHandImage.color = ScreenControlUtility.ParseColor(SettingsConfig.Config.CursorRingColor, SettingsConfig.Config.CursorRingOpacity);

        cursorDot.color = dotBorderColor;
        cursorDotFill.color = dotFillColor;

        cursorDotSize = (GlobalSettings.ScreenHeight / PhysicalConfigurable.Config.ScreenHeightM) * SettingsConfig.Config.CursorDotSizeM / 100f;
        var dotSizeIsZero = Mathf.Approximately(cursorDotSize, 0f);
        cursorDotSize = dotSizeIsZero ? 1f : cursorDotSize;
        cursorDot.enabled = !dotSizeIsZero;

        if(!hidingCursor)
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
using UnityEngine;

namespace Ultraleap.ScreenControl.Client
{
    public class PinchGrabCursor : Cursor
    {
        [Header("Graphics")]
        public UnityEngine.UI.Image cursorDot;
        public UnityEngine.UI.Image cursorDotFill;

        protected float cursorDotSize;
        protected Color dotFillColor;
        protected Color dotBorderColor;

        private ScreenControlTypes.HandChirality cursorChirality = ScreenControlTypes.HandChirality.RIGHT;

        [Header("HandGraphics")]
        public UnityEngine.UI.Image openHandImage;
        public UnityEngine.UI.Image closedHandImage;

        protected override void OnHandleInputAction(ScreenControlTypes.ClientInputAction _inputData)
        {
            ScreenControlTypes.InputType type = _inputData.Type;
            Vector2 cursorPosition = _inputData.CursorPosition;
            float distanceFromScreen = _inputData.ProgressToClick;

            if (_inputData.Chirality != cursorChirality && _inputData.Chirality != ScreenControlTypes.HandChirality.UNKNOWN)
            {
                cursorDot.transform.Rotate(0f, 180f, 0f);
                cursorChirality = _inputData.Chirality;
            }

            switch (type)
            {
                case ScreenControlTypes.InputType.MOVE:
                    UpdateCursor(cursorPosition, distanceFromScreen);
                    break;
                case ScreenControlTypes.InputType.DOWN:
                    openHandImage.enabled = false;
                    closedHandImage.enabled = true;
                    break;
                case ScreenControlTypes.InputType.UP:
                case ScreenControlTypes.InputType.CANCEL:
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

            cursorDotSize = (ClientSettings.ScreenHeight_px / ConnectionManager.coreConnection.physicalConfig.ScreenHeightM) * ClientSettings.clientConstants.CursorDotSizeM / 100f;
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
}
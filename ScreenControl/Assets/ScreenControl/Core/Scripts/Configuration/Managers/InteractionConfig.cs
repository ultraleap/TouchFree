using System;

namespace Ultraleap.ScreenControl.Core
{
    public struct GenericInteractionSettings
    {
        public bool ShowSetupScreenOnStartup;
        public bool UseScrollingOrDragging;
        public float DeadzoneRadius;
        // Todo: Rename this to be more accurate
        public float CursorVerticalOffset;
        // Todo: Find a better name for this because distance from screen is calibrated based on it as well
        public float TouchPlaneDistanceFromScreenM;
    }

    public struct HoverAndHoldInteractionSettings
    {
        public float HoverCursorStartTimeS;
        public float HoverCursorCompleteTimeS;
    }

    public struct TouchPlaneInteractionSettings
    {
        public float CursorMaxRingScaleAtDistanceM;
        public float PushDeadzoneStartDistance;
    }

    public class InteractionConfig : BaseSettings
    {
        public GenericInteractionSettings Generic;
        // Interaction-specific settings
        public HoverAndHoldInteractionSettings HoverAndHold;
        public TouchPlaneInteractionSettings TouchPlane;

        public InteractionConfig()
        {
            SetAllValuesToDefault();
        }

        public override void SetAllValuesToDefault()
        {
            Generic.ShowSetupScreenOnStartup = true;
            Generic.UseScrollingOrDragging = false;
            Generic.DeadzoneRadius = 0.003f;
            Generic.CursorVerticalOffset = 0f;
            Generic.TouchPlaneDistanceFromScreenM = 0.05f;
            HoverAndHold.HoverCursorStartTimeS = 0.5f;
            HoverAndHold.HoverCursorCompleteTimeS = 0.6f;
            TouchPlane.CursorMaxRingScaleAtDistanceM = 2.0f;
            TouchPlane.PushDeadzoneStartDistance = 0.1f;
        }

        public void SaveConfig()
        {
            InteractionConfigFile.SaveConfig(this);
        }
    }
}

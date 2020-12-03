using UnityEngine;

namespace Ultraleap.ScreenControl.Core
{
    namespace ScreenControlTypes
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

        public class InteractionConfig
        {
            public GenericInteractionSettings Generic;
            // Interaction-specific settings
            public HoverAndHoldInteractionSettings HoverAndHold;
            public TouchPlaneInteractionSettings TouchPlane;

            public InteractionConfig()
            {
                SetAllValuesToDefault();
            }

            public void SetAllValuesToDefault()
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
        }

        public class PhysicalConfig
        {
            public float ScreenHeightM = 0.33f;
            public Vector3 LeapPositionRelativeToScreenBottomM = new Vector3(0f, -0.12f, -0.25f);
            public Vector3 LeapRotationD = Vector3.zero;
            public float ScreenRotationD = 0f;

            public void SetAllValuesToDefault()
            {
                var defaults = new PhysicalConfig();

                ScreenHeightM = defaults.ScreenHeightM;
                LeapPositionRelativeToScreenBottomM = defaults.LeapPositionRelativeToScreenBottomM;
                LeapRotationD = defaults.LeapRotationD;
                ScreenRotationD = defaults.ScreenRotationD;
            }
        }

        public class GlobalSettings
        {
            public int CursorWindowSize = 256;
            public int ScreenWidth;
            public int ScreenHeight;

            // Store in M, display in CM
            public readonly float ConfigToDisplayMeasurementMultiplier = 100;

            public void SetAllValuesToDefault()
            {
                var defaults = new GlobalSettings();

                CursorWindowSize = defaults.CursorWindowSize;
            }
        }
    }
}
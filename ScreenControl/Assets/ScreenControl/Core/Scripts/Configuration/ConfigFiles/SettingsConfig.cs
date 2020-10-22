namespace Ultraleap.ScreenControl.Core
{
    public class SettingsData
    {
        public string CursorRingColor = "#000000";
        public float CursorRingOpacity = 1;
        public string CursorDotFillColor = "#000000";
        public float CursorDotFillOpacity = 1;
        public string CursorDotBorderColor = "#FFFFFF";
        public float CursorDotBorderOpacity = 0.8f;
        public CursorColourPreset CursorColorPreset = CursorColourPreset.black_Contrast;

        public float CursorDotSizeM = 0.008f;
        public float CursorRingMaxScale = 2.0f;
        public float CursorMaxRingScaleAtDistanceM = 0.1f;
        public bool UseScrollingOrDragging = false;
        public bool ShowSetupScreenOnStartup = true;
        public float DeadzoneRadius = 0.003f;
        public float CursorVerticalOffset = 0f;
        public bool SendHoverEvents = false;

        public float HoverCursorStartTimeS = 0.5f;
        public float HoverCursorCompleteTimeS = 0.6f;

        public float TouchPlaneDistanceFromScreenM = 0.05f;
    }

    public class SettingsConfig : ConfigFile<SettingsData, SettingsConfig>
    {
        public const float CursorDotSize_Min = 0.001f;
        public const float CursorDotSize_Max = 0.01f;
        public const float CursorRingMaxScale_Min = 1f;
        public const float CursorRingMaxScale_Max = 3f;
        public const float CursorMaxRingScaleAtDistance_Min = 0f;
        public const float CursorMaxRingScaleAtDistance_Max = 0.2f;

        public const int minResolution = 200;

        public const float CursorDeadzone_Min = 0f;
        public const float CursorDeadzone_Max = 0.015f;

        public const float CursorVerticalOffset_Min = -0.1f;
        public const float CursorVerticalOffset_Max = 0.1f;

        public const float HoverCursorStartTime_Min = 0.1f;
        public const float HoverCursorStartTime_Max = 2f;
        public const float HoverCursorCompleteTime_Min = 0.1f;
        public const float HoverCursorCompleteTime_Max = 2f;

        public const string CursorColourPresetLight = "#F4F4F4";
        public const string CursorColourPresetDark = "#2B2B2B";

        public const string CursorColourPresetWhite = "#FFFFFF";
        public const string CursorColourPresetBlack = "#000000";

        public const float CursorRingOpacityPreset = 1;
        public const float CursorDotFillOpacityPreset = 1;
        public const float CursorDotBorderOpacityPreset = 0.8f;

        public const float PushDeadzoneStartDistance = 0.1f;

        public override string ConfigFileName => "Settings.json";

        protected override void ApplyParameterLimits(ref SettingsData config)
        {
            ClampValue(ref config.CursorDotSizeM, CursorDotSize_Min, CursorDotSize_Max);
            ClampValue(ref config.CursorRingMaxScale, CursorRingMaxScale_Min, CursorRingMaxScale_Max);
            ClampValue(ref config.CursorMaxRingScaleAtDistanceM, CursorMaxRingScaleAtDistance_Min, CursorMaxRingScaleAtDistance_Max);

            ClampValue(ref config.DeadzoneRadius, CursorDeadzone_Min, CursorDeadzone_Max);
        }
    }

    public enum CursorColourPreset
    {
        light,
        dark,
        white_Contrast,
        black_Contrast,
        custom
    }
}
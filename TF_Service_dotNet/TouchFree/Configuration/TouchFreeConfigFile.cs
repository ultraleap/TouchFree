using System;
using System.Drawing;
using System.IO;

namespace Ultraleap.TouchFree.Library.Configuration
{
    public class TouchFreeConfigFile : ConfigFile<TouchFreeConfig, TouchFreeConfigFile>
    {
        protected override string _ConfigFileName => "TouchFreeConfig.json";
    }

    public enum CtiHideTrigger
    {
        PRESENCE,
        INTERACTION
    }

    public enum CursorColorPreset
    {
        LIGHT,
        DARK,
        CUSTOM,
        LIGHT_OUTLINE,
        DARK_OUTLINE,
    }

    [Serializable]
    public record class TouchFreeConfig
    {
        // Cursor Settings
        public bool cursorEnabled = true;
        public float cursorSizeCm = 0.25f;
        public float cursorRingThickness = 0.15f;
        public CursorColorPreset activeCursorPreset = CursorColorPreset.LIGHT;
        public Color primaryCustomColor = Color.White;
        public Color secondaryCustomColor = Color.White;
        public Color tertiaryCustomColor = Color.Black;

        // CTI Settings
        public bool ctiEnabled = false;
        public string ctiFilePath = Path.Combine(AppContext.BaseDirectory, "../SettingsUI/TouchFreeSettingsUI_Data/StreamingAssets", "CallToInteract", "AirPush_Portrait.mp4");
        public CtiHideTrigger ctiHideTrigger = CtiHideTrigger.INTERACTION;
        public float ctiShowAfterTimer = 10.0f;

        // Misc Settings
        public bool StartupUIShown = false;

        public void DefaultCTI()
        {
            ctiFilePath = Path.Combine(AppContext.BaseDirectory, "../SettingsUI/TouchFreeSettingsUI_Data/StreamingAssets", "CallToInteract", "AirPush_Portrait.mp4");
        }
    }
}
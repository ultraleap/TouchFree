using System;
using System.IO;
using UnityEngine;

namespace Ultraleap.TouchFree.ServiceShared
{
    public enum CtiHideTrigger
    {
        PRESENCE,
        INTERACTION
    }

    public enum CursorColorPreset
    {
        LIGHT,
        DARK,
        CUSTOM
    }

    public static class TFAppConfig
    {
        public static ConfigurationState Config
        {
            get
            {
                if (_config == null)
                {
                    _config = TouchFreeAppConfigFile.LoadConfig();
                }

                return _config;
            }
            set
            {
                _config = value;
            }
        }

        private static ConfigurationState _config = null;
    }

    public class ConfigurationState
    {
        // Cursor Settings
        public bool cursorEnabled = true;
        public float cursorSizeCm = 0.25f;
        public float cursorRingThickness = 0.15f;
        public CursorColorPreset activeCursorPreset = CursorColorPreset.LIGHT;
        public Color primaryCustomColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        public Color secondaryCustomColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        public Color tertiaryCustomColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);

        // CTI Settings
        public bool ctiEnabled = false;
        public string ctiFilePath = Path.Combine(Application.streamingAssetsPath, "CallToInteract/AirPush_Portrait.mp4");
        public CtiHideTrigger ctiHideTrigger = CtiHideTrigger.INTERACTION;
        public float ctiShowAfterTimer = 10.0f;

        // Misc Settings
        public bool StartupUIShown = false;

        public event Action OnConfigUpdated;

        public void ConfigWasUpdated()
        {
            OnConfigUpdated?.Invoke();
        }

        public void GetCurrentColors(ref Color Primary, ref Color Secondary, ref Color Tertiary) {
            switch (activeCursorPreset)
            {
                case CursorColorPreset.LIGHT:
                    Primary = Color.white;
                    Secondary = Color.white;
                    Tertiary = Color.black;
                    break;
                case CursorColorPreset.DARK:
                    Primary = Color.black;
                    Secondary = Color.black;
                    Tertiary = Color.white;
                    break;
                case CursorColorPreset.CUSTOM:
                    Primary = primaryCustomColor;
                    Secondary = secondaryCustomColor;
                    Tertiary = tertiaryCustomColor;
                    break;
            }
        }

        public void SetAllValuesToDefault()
        {
            var defaults = new ConfigurationState();

            cursorEnabled = defaults.cursorEnabled;
            cursorSizeCm = defaults.cursorSizeCm;
            cursorRingThickness = defaults.cursorRingThickness;
            activeCursorPreset = defaults.activeCursorPreset;
            primaryCustomColor = defaults.primaryCustomColor;
            secondaryCustomColor = defaults.secondaryCustomColor;
            tertiaryCustomColor = defaults.tertiaryCustomColor;

            ctiEnabled = defaults.ctiEnabled;
            ctiFilePath = defaults.ctiFilePath;
            ctiHideTrigger = defaults.ctiHideTrigger;
            ctiShowAfterTimer = defaults.ctiShowAfterTimer;
        }

        public void SaveConfig()
        {
            TouchFreeAppConfigFile.SaveConfig(this);
        }
    }
}
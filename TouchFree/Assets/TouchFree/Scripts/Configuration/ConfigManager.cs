using System;
using System.IO;
using UnityEngine;

namespace Ultraleap.TouchFree
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

    public static class ConfigManager
    {
        public static ConfigurationState Config
        {
            get
            {
                if (_config == null)
                {
                    _config = TouchFreeConfigFile.LoadConfig();
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
        public float cursorSizeCm = 0.8f;
        public CursorColorPreset activeCursorPreset = CursorColorPreset.LIGHT;
        public Color primaryCustomColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        public Color secondaryCustomColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        public Color tertiaryCustomColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);

        // CTI Settings
        public bool ctiEnabled = true;
        public string ctiFilePath = Path.Combine(Application.streamingAssetsPath, "CallTointeract/1 Push in mid-air to start.mp4");
        public CtiHideTrigger ctiHideTrigger = CtiHideTrigger.INTERACTION;
        public float ctiShowAfterTimer = 10.0f;

        // Misc Settings
        public bool interactionZoneEnabled = false;
        public float interactionMinDistanceCm = 0.0f;
        public float interactionMaxDistanceCm = 25.0f;
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
            activeCursorPreset = defaults.activeCursorPreset;
            primaryCustomColor = defaults.primaryCustomColor;
            secondaryCustomColor = defaults.secondaryCustomColor;
            tertiaryCustomColor = defaults.tertiaryCustomColor;

            ctiEnabled = defaults.ctiEnabled;
            ctiFilePath = defaults.ctiFilePath;
            ctiHideTrigger = defaults.ctiHideTrigger;
            ctiShowAfterTimer = defaults.ctiShowAfterTimer;

            interactionZoneEnabled = defaults.interactionZoneEnabled;
            interactionMinDistanceCm = defaults.interactionMinDistanceCm;
            interactionMaxDistanceCm = defaults.interactionMaxDistanceCm;
        }

        public void SaveConfig()
        {
            TouchFreeConfigFile.SaveConfig(this);
        }
    }
}
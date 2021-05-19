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

    public static class ConfigManager
    {
        public static ConfigurationState Config
        {
            get { return new ConfigurationState(); }
        }
    }

    public class ConfigurationState
    {
        // Cursor Settings
        public bool cursorEnabled = true;
        public float cursorSizeCm = 0.8f;
        public Color primaryCursorColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        public Color secondaryCursorColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        public Color tertiaryCursorColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);

        // CTI Settings
        public bool ctiEnabled = true;
        public string ctiFilePath = Path.Combine(Application.streamingAssetsPath, "CallTointeract", "1 Push in mid-air to start.mp4");
        public CtiHideTrigger ctiHideTrigger = CtiHideTrigger.PRESENCE;
        public float ctiShowAfterTimer = 2.0f;

        // Misc Settings
        public bool interactionZoneEnabled = false;
        public float interactionMinDistanceCm = 0.0f;
        public float interactionMaxDistanceCm = 25.0f;

        public event Action OnConfigUpdated;

        public void ConfigWasUpdated()
        {
            OnConfigUpdated?.Invoke();
        }

        public void SetAllValuesToDefault()
        {
            var defaults = new ConfigurationState();

            cursorEnabled = defaults.cursorEnabled;
            cursorSizeCm = defaults.cursorSizeCm;
            primaryCursorColor = defaults.primaryCursorColor;
            secondaryCursorColor = defaults.secondaryCursorColor;

            ctiEnabled = defaults.ctiEnabled;
            ctiFilePath = defaults.ctiFilePath;
            ctiHideTrigger = defaults.ctiHideTrigger;
            ctiShowAfterTimer = defaults.ctiShowAfterTimer;

            interactionZoneEnabled = defaults.interactionZoneEnabled;
            interactionMinDistanceCm = defaults.interactionMinDistanceCm;
            interactionMaxDistanceCm = defaults.interactionMaxDistanceCm;
        }
    }
}
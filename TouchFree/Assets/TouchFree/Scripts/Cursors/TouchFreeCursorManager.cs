using UnityEngine;
using Ultraleap.ScreenControl.Client.Cursors;

namespace Ultraleap.TouchFree
{
    public class TouchFreeCursorManager : CursorManager
    {
        protected Color Primary;
        protected Color Secondary;
        protected Color Tertiary;

        protected override void OnEnable()
        {
            ConfigManager.Config.OnConfigUpdated += ConfigUpdated;
            base.OnEnable();
            ConfigUpdated();
        }

        protected override void OnDisable()
        {
            ConfigManager.Config.OnConfigUpdated -= ConfigUpdated;
            base.OnDisable();
        }

        void Start()
        {
            Primary = defaultCursor.primaryColor;
            Secondary = defaultCursor.secondaryColor;
            Tertiary = defaultCursor.tertiaryColor;
        }

        void ConfigUpdated()
        {
            // Update size, colors, & visibility based on current Config
            SetCursorVisibility(ConfigManager.Config.cursorEnabled);

            var cursorSize = ConfigManager.Config.cursorSizeCm;
            ConfigManager.Config.GetCurrentColors(ref Primary, ref Secondary, ref Tertiary);

            foreach (InteractionCursor cursor in interactionCursors)
            {
                cursor.cursor.cursorSize = cursorSize;
                cursor.cursor.SetColors(Primary, Secondary, Tertiary);
            }
        }
    }
}
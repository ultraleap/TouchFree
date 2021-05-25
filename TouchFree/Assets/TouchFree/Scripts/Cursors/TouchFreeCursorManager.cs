using UnityEngine;
using Ultraleap.ScreenControl.Client.Cursors;

namespace Ultraleap.TouchFree
{
    public class TouchFreeCursorManager : CursorManager
    {
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
            Color Primary = defaultCursor.primaryColor;
            Color Secondary = defaultCursor.secondaryColor;
            Color Tertiary = defaultCursor.tertiaryColor;

            ConfigManager.Config.GetCurrentColors(ref Primary, ref Secondary, ref Tertiary);

            foreach (InteractionCursor cursor in interactionCursors)
            {
                cursor.cursor.SetColors(Primary, Secondary, Tertiary);
            }
        }

        void ConfigUpdated()
        {
            SetCursorVisibility(ConfigManager.Config.cursorEnabled);
        }
    }
}
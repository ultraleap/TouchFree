using UnityEngine;

namespace Ultraleap.TouchFree
{
    public class TouchFreeCursorManager : CursorManager
    {
        protected Color Primary;
        protected Color Secondary;
        protected Color Tertiary;

        protected override void OnEnable()
        {
            ServiceShared.TFAppConfig.Config.OnConfigUpdated += ConfigUpdated;
            base.OnEnable();
            ConfigUpdated();
        }

        protected override void OnDisable()
        {
            ServiceShared.TFAppConfig.Config.OnConfigUpdated -= ConfigUpdated;
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
            SetCursorVisibility(ServiceShared.TFAppConfig.Config.cursorEnabled);

            var cursorSize = ServiceShared.TFAppConfig.Config.cursorSizeCm;
            ServiceShared.TFAppConfig.Config.GetCurrentColors(ref Primary, ref Secondary, ref Tertiary);

            foreach (InteractionCursor cursor in interactionCursors)
            {
                cursor.cursor.SetRingThickness(ServiceShared.TFAppConfig.Config.cursorRingThickness);
                cursor.cursor.cursorSize = cursorSize;
                cursor.cursor.SetColors(Primary, Secondary, Tertiary);
            }
        }
    }
}
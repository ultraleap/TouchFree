using System;

namespace Ultraleap.ScreenControl.Core
{
    [Serializable]
    public class HoverAndHoldInteractionSettings
    {
        public float HoverCursorStartTimeS = 0.5f;
        public float HoverCursorCompleteTimeS = 0.6f;
    }

    [Serializable]
    public class InteractionConfig : BaseSettings
    {
        public bool UseScrollingOrDragging = false;
        public float DeadzoneRadius = 0.003f;

        // Interaction-specific settings
        public HoverAndHoldInteractionSettings HoverAndHold;

        public override void SetAllValuesToDefault()
        {
            var defaults = new InteractionConfig();

            UseScrollingOrDragging = defaults.UseScrollingOrDragging;
            DeadzoneRadius = defaults.DeadzoneRadius;
            HoverAndHold.HoverCursorStartTimeS = defaults.HoverAndHold.HoverCursorStartTimeS;
            HoverAndHold.HoverCursorCompleteTimeS = defaults.HoverAndHold.HoverCursorCompleteTimeS;
        }

        public void SaveConfig()
        {
            InteractionConfigFile.SaveConfig(this);
        }
    }
}

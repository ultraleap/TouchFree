using System;

namespace Ultraleap.ScreenControl.Core
{
    [Serializable]
    public class HoverAndHoldInteractionSettings
    {
        public float HoverStartTimeS = 0.5f;
        public float HoverCompleteTimeS = 0.6f;
    }

    [Serializable]
    public class InteractionConfig : BaseSettings
    {
        public bool UseScrollingOrDragging = false;
        public float DeadzoneRadius = 0.003f;

        // TODO: Store the currently selected interaction here

        // Interaction-specific settings
        public HoverAndHoldInteractionSettings HoverAndHold;

        public override void SetAllValuesToDefault()
        {
            var defaults = new InteractionConfig();

            UseScrollingOrDragging = defaults.UseScrollingOrDragging;
            DeadzoneRadius = defaults.DeadzoneRadius;
            HoverAndHold.HoverStartTimeS = defaults.HoverAndHold.HoverStartTimeS;
            HoverAndHold.HoverCompleteTimeS = defaults.HoverAndHold.HoverCompleteTimeS;
        }

        public void SaveConfig()
        {
            InteractionConfigFile.SaveConfig(this);
        }
    }
}

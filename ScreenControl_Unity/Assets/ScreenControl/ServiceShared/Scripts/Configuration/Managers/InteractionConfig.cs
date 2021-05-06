using System;
using Ultraleap.ScreenControl.Core.ScreenControlTypes;

namespace Ultraleap.ScreenControl.Core
{
    [Serializable]
    public class HoverAndHoldInteractionSettings
    {
        public float HoverStartTimeS = 0.5f;
        public float HoverCompleteTimeS = 0.6f;
    }

    [Serializable]
    public class TouchPlaneInteractionSettings
    {
        public float TouchPlaneDistanceCM = 5f;
        public float TouchPlaneStartDistanceCM = 5f;
    }

    [Serializable]
    public class InteractionConfig : BaseSettings
    {
        public bool UseScrollingOrDragging = false;
        public float DeadzoneRadius = 0.003f;

        public InteractionType InteractionType = InteractionType.PUSH;

        // Interaction-specific settings
        public HoverAndHoldInteractionSettings HoverAndHold = new HoverAndHoldInteractionSettings();
        public TouchPlaneInteractionSettings TouchPlane = new TouchPlaneInteractionSettings();

        public override void SetAllValuesToDefault()
        {
            var defaults = new InteractionConfig();

            UseScrollingOrDragging = defaults.UseScrollingOrDragging;
            DeadzoneRadius = defaults.DeadzoneRadius;
            HoverAndHold.HoverStartTimeS = defaults.HoverAndHold.HoverStartTimeS;
            HoverAndHold.HoverCompleteTimeS = defaults.HoverAndHold.HoverCompleteTimeS;
            TouchPlane.TouchPlaneDistanceCM = defaults.TouchPlane.TouchPlaneDistanceCM;
            TouchPlane.TouchPlaneStartDistanceCM = defaults.TouchPlane.TouchPlaneStartDistanceCM;
            InteractionType = defaults.InteractionType;
        }

        public void SaveConfig()
        {
            InteractionConfigFile.SaveConfig(this);
        }
    }
}

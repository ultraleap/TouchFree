using System;

namespace Ultraleap.TouchFree.Library.Configuration
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
        public float TouchPlaneActivationDistanceMm = 50f;
        public TrackedPosition TouchPlaneTrackedPosition = TrackedPosition.NEAREST;
    }

    [Serializable]
    public class InteractionConfig
    {
        public bool UseScrollingOrDragging = false;
        public float DeadzoneRadius = 0.003f;

        public bool InteractionZoneEnabled = false;
        public float InteractionMinDistanceMm = 0.0f;
        public float InteractionMaxDistanceMm = 250.0f;

        public InteractionType InteractionType = InteractionType.PUSH;

        // Interaction-specific settings
        public HoverAndHoldInteractionSettings HoverAndHold = new HoverAndHoldInteractionSettings();
        public TouchPlaneInteractionSettings TouchPlane = new TouchPlaneInteractionSettings();

        public InteractionConfig()
        {
            this.UseScrollingOrDragging = false;
            this.DeadzoneRadius = 0.003f;

            this.InteractionZoneEnabled = false;
            this.InteractionMinDistanceMm = 0.0f;
            this.InteractionMaxDistanceMm = 250.0f;

            this.InteractionType = InteractionType.PUSH;

            this.HoverAndHold = new HoverAndHoldInteractionSettings();
            this.TouchPlane = new TouchPlaneInteractionSettings();
        }

        public InteractionConfig(InteractionConfigForFile fromFile)
        {
            this.InteractionMinDistanceMm = fromFile.InteractionMaxDistanceCm * 10f;
            this.InteractionMaxDistanceMm = fromFile.InteractionMaxDistanceCm * 10f;

            this.UseScrollingOrDragging = fromFile.UseScrollingOrDragging;
            this.DeadzoneRadius = fromFile.DeadzoneRadius;

            this.InteractionZoneEnabled = fromFile.InteractionZoneEnabled;

            this.InteractionType = fromFile.InteractionType;

            HoverAndHoldInteractionSettings intermedHH = new HoverAndHoldInteractionSettings();
            intermedHH.HoverStartTimeS = fromFile.HoverAndHold.HoverStartTimeS;
            intermedHH.HoverCompleteTimeS = fromFile.HoverAndHold.HoverCompleteTimeS;

            TouchPlaneInteractionSettings intermedTP = new TouchPlaneInteractionSettings();
            intermedTP.TouchPlaneActivationDistanceMm = fromFile.TouchPlane.TouchPlaneActivationDistanceCm * 10f;
            intermedTP.TouchPlaneTrackedPosition = fromFile.TouchPlane.TouchPlaneTrackedPosition;

            this.HoverAndHold = intermedHH;
            this.TouchPlane = intermedTP;
        }
    }
}
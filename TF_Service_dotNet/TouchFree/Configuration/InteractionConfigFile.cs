using System;

namespace Ultraleap.TouchFree.Library.Configuration
{
    public class InteractionConfigFile : ConfigFile<InteractionConfig, InteractionConfigFile>
    {
        protected override string _ConfigFileName => "InteractionConfig.json";
    }

    [Serializable]
    public class HoverAndHoldInteractionSettings
    {
        public float HoverStartTimeS = 0.5f;
        public float HoverCompleteTimeS = 0.6f;
    }

    [Serializable]
    public class TouchPlaneInteractionSettings
    {
        public float TouchPlaneActivationDistanceCm = 5f;
        public TrackedPosition TouchPlaneTrackedPosition = TrackedPosition.NEAREST;
    }

    [Serializable]
    public class InteractionConfig
    {
        public bool UseScrollingOrDragging = false;
        public float DeadzoneRadius = 0.003f;

        public bool InteractionZoneEnabled = false;
        public float InteractionMinDistanceCm = 0.0f;
        public float InteractionMaxDistanceCm = 25.0f;

        public InteractionType InteractionType = InteractionType.PUSH;

        // Interaction-specific settings
        public HoverAndHoldInteractionSettings HoverAndHold = new HoverAndHoldInteractionSettings();
        public TouchPlaneInteractionSettings TouchPlane = new TouchPlaneInteractionSettings();

        public InteractionConfig()
        {
            this.UseScrollingOrDragging = false;
            this.DeadzoneRadius = 0.003f;

            this.InteractionZoneEnabled = false;
            this.InteractionMinDistanceCm = 0.0f;
            this.InteractionMaxDistanceCm = 25.0f;

            this.InteractionType = InteractionType.PUSH;

            // Interaction-specific settings
            this.HoverAndHold = new HoverAndHoldInteractionSettings();
            this.TouchPlane = new TouchPlaneInteractionSettings();
        }

        public InteractionConfig(InteractionConfigInternal _internal)
        {
            this.InteractionMinDistanceCm = _internal.InteractionMinDistanceMm / 10f;
            this.InteractionMaxDistanceCm = _internal.InteractionMaxDistanceMm / 10f;

            this.DeadzoneRadius = _internal.DeadzoneRadiusMm / 1000f;

            this.UseScrollingOrDragging = _internal.UseScrollingOrDragging;
            this.InteractionZoneEnabled = _internal.InteractionZoneEnabled;

            this.InteractionType = _internal.InteractionType;

            HoverAndHoldInteractionSettings intermedHH = new HoverAndHoldInteractionSettings();
            intermedHH.HoverStartTimeS = _internal.HoverAndHold.HoverStartTimeS;
            intermedHH.HoverCompleteTimeS = _internal.HoverAndHold.HoverCompleteTimeS;

            TouchPlaneInteractionSettings intermedTP = new TouchPlaneInteractionSettings();
            intermedTP.TouchPlaneActivationDistanceCm = _internal.TouchPlane.TouchPlaneActivationDistanceMm / 10f;
            intermedTP.TouchPlaneTrackedPosition = _internal.TouchPlane.TouchPlaneTrackedPosition;

            this.HoverAndHold = intermedHH;
            this.TouchPlane = intermedTP;
        }
    }
}
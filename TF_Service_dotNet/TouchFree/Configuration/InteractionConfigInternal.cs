using System;

namespace Ultraleap.TouchFree.Library.Configuration
{
    [Serializable]
    public class HoverAndHoldInteractionSettingsInternal
    {
        public float HoverStartTimeS = 0.5f;
        public float HoverCompleteTimeS = 0.6f;
    }

    [Serializable]
    public class TouchPlaneInteractionSettingsInternal
    {
        public float TouchPlaneActivationDistanceMm = 50f;
        public TrackedPosition TouchPlaneTrackedPosition = TrackedPosition.NEAREST;
    }

    [Serializable]
    public class InteractionConfigInternal
    {
        public bool UseScrollingOrDragging = true;
        public float DeadzoneRadiusMm = 3f;

        public bool InteractionZoneEnabled = false;
        public float InteractionMinDistanceMm = 0.0f;
        public float InteractionMaxDistanceMm = 250.0f;

        public InteractionType InteractionType = InteractionType.PUSH;

        // Interaction-specific settings
        public HoverAndHoldInteractionSettingsInternal HoverAndHold = new HoverAndHoldInteractionSettingsInternal();
        public TouchPlaneInteractionSettingsInternal TouchPlane = new TouchPlaneInteractionSettingsInternal();

        public InteractionConfig ForApi()
        {
            return new InteractionConfig()
            {
                DeadzoneRadius = DeadzoneRadiusMm / 1000f,
                HoverAndHold = new HoverAndHoldInteractionSettings()
                {
                    HoverCompleteTimeS = HoverAndHold.HoverCompleteTimeS,
                    HoverStartTimeS = HoverAndHold.HoverStartTimeS,
                },
                InteractionMaxDistanceCm = InteractionMaxDistanceMm / 10f,
                InteractionMinDistanceCm = InteractionMinDistanceMm / 10f,
                InteractionType = InteractionType,
                InteractionZoneEnabled = InteractionZoneEnabled,
                UseScrollingOrDragging = UseScrollingOrDragging,
                TouchPlane = new TouchPlaneInteractionSettings()
                {
                    TouchPlaneActivationDistanceCm = TouchPlane.TouchPlaneActivationDistanceMm / 10f,
                    TouchPlaneTrackedPosition = TouchPlane.TouchPlaneTrackedPosition
                }
            };
        }

        public InteractionConfigInternal()
        {
            this.UseScrollingOrDragging = true;
            this.DeadzoneRadiusMm = 3f;

            this.InteractionZoneEnabled = false;
            this.InteractionMinDistanceMm = 0.0f;
            this.InteractionMaxDistanceMm = 250.0f;

            this.InteractionType = InteractionType.PUSH;

            this.HoverAndHold = new HoverAndHoldInteractionSettingsInternal();
            this.TouchPlane = new TouchPlaneInteractionSettingsInternal();
    }

        public InteractionConfigInternal(InteractionConfig fromFile)
        {
            this.InteractionMinDistanceMm = fromFile.InteractionMinDistanceCm * 10f;
            this.InteractionMaxDistanceMm = fromFile.InteractionMaxDistanceCm * 10f;

            this.DeadzoneRadiusMm = fromFile.DeadzoneRadius * 1000;

            this.UseScrollingOrDragging = fromFile.UseScrollingOrDragging;
            this.InteractionZoneEnabled = fromFile.InteractionZoneEnabled;

            this.InteractionType = fromFile.InteractionType;

            HoverAndHoldInteractionSettingsInternal intermedHH = new HoverAndHoldInteractionSettingsInternal();
            intermedHH.HoverStartTimeS = fromFile.HoverAndHold.HoverStartTimeS;
            intermedHH.HoverCompleteTimeS = fromFile.HoverAndHold.HoverCompleteTimeS;

            TouchPlaneInteractionSettingsInternal intermedTP = new TouchPlaneInteractionSettingsInternal();
            intermedTP.TouchPlaneActivationDistanceMm = fromFile.TouchPlane.TouchPlaneActivationDistanceCm * 10f;
            intermedTP.TouchPlaneTrackedPosition = fromFile.TouchPlane.TouchPlaneTrackedPosition;

            this.HoverAndHold = intermedHH;
            this.TouchPlane = intermedTP;
        }
    }
}
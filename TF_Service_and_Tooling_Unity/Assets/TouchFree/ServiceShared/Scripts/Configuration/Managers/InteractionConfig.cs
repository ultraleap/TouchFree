using System;

namespace Ultraleap.TouchFree.ServiceShared
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
        public float TouchPlaneActivationDistanceCM = 5f;
        public TrackedPosition TouchPlaneTrackedPosition = TrackedPosition.NEAREST;
    }

    [Serializable]
    public class AirPushInteractionSettings
    {
        // distance to drag start
        public float AirPushTriggerDistanceAtMaxSpeedM = 0.005f;
        public float AirPushTriggerDistanceAtMinSpeedM = 0.015f;

        // Angles before click cancel
        public float AirPushApproachAngleDeg = 40f;
        // thetaOne
        public float AirPushExitAngleDeg = 135f;
        // thetaTwo

        // If a hand moves an angle less than ApproachAngle, this is "towards" the screen
        // If a hand moves an angle greater than ExitAngle, this is "backwards" from the screen
        // If a hand moves between the two angles, this is "horizontal" to the screen

        public float AirPushReleaseThreshold = 0.9f;
        // unclickThreshold
        public float AirPushDraggingReleaseThreshold = 0.8f;

        public float AirPushDragDistanceThresholdM = 0.013f;
        public float AirPushDragDeadzoneShrinkRate = 0.9f;
    }

    [Serializable]
    public class InteractionConfig : BaseSettings
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
        public AirPushInteractionSettings AirPush = new AirPushInteractionSettings();

        public override void SetAllValuesToDefault()
        {
            var defaults = new InteractionConfig();

            UseScrollingOrDragging = defaults.UseScrollingOrDragging;
            DeadzoneRadius = defaults.DeadzoneRadius;

            InteractionZoneEnabled = defaults.InteractionZoneEnabled;
            InteractionMinDistanceCm = defaults.InteractionMinDistanceCm;
            InteractionMaxDistanceCm = defaults.InteractionMaxDistanceCm;

            InteractionType = defaults.InteractionType;

            HoverAndHold.HoverStartTimeS = defaults.HoverAndHold.HoverStartTimeS;
            HoverAndHold.HoverCompleteTimeS = defaults.HoverAndHold.HoverCompleteTimeS;
            TouchPlane.TouchPlaneActivationDistanceCM = defaults.TouchPlane.TouchPlaneActivationDistanceCM;
            TouchPlane.TouchPlaneTrackedPosition = defaults.TouchPlane.TouchPlaneTrackedPosition;
            AirPush.AirPushTriggerDistanceAtMaxSpeedM = defaults.AirPush.AirPushTriggerDistanceAtMaxSpeedM;
            AirPush.AirPushTriggerDistanceAtMinSpeedM = defaults.AirPush.AirPushTriggerDistanceAtMinSpeedM;
            AirPush.AirPushApproachAngleDeg = defaults.AirPush.AirPushApproachAngleDeg;
            AirPush.AirPushExitAngleDeg = defaults.AirPush.AirPushExitAngleDeg;
            AirPush.AirPushReleaseThreshold = defaults.AirPush.AirPushReleaseThreshold;
            AirPush.AirPushDraggingReleaseThreshold = defaults.AirPush.AirPushDraggingReleaseThreshold;
            AirPush.AirPushDragDistanceThresholdM = defaults.AirPush.AirPushDragDistanceThresholdM;
            AirPush.AirPushDragDeadzoneShrinkRate = defaults.AirPush.AirPushDragDeadzoneShrinkRate;
        }

        public void SaveConfig()
        {
            InteractionConfigFile.SaveConfig(this);
        }
    }
}

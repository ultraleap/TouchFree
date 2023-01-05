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
    public class AirPushSettings
    {
        public float SpeedMin = 150f;
        public float SpeedMax = 500f;
        public float DistAtSpeedMinMm = 42f;
        public float DistAtSpeedMaxMm = 8f;
        public float HorizontalDecayDistMm = 50f;

        public float ThetaOne = 65f;
        public float ThetaTwo = 135f;

        public float UnclickThreshold = 0.97f;
        public float UnclickThresholdDrag = 0.97f;
        public bool DecayForceOnClick = true;
        public float ForceDecayTime = 0.1f;

        public bool UseTouchPlaneForce = true;
        public float DistPastTouchPlaneMm = 20f;

        public float DragStartDistanceThresholdMm = 30f;
        public float DragDeadzoneShrinkRate = 0.9f;
        public float DragDeadzoneShrinkDistanceThresholdMm = 10f;

        public float DeadzoneMaxSizeIncreaseMm = 20f;
        public float DeadzoneShrinkRate = 0.8f;
    }

    [Serializable]
    public class VelocitySwipeSettings
    {
        public float MinScrollVelocity_mmps = 625f;
        public float UpwardsMinVelocityDecrease_mmps = 50f;
        public float DownwardsMinVelocityIncrease_mmps = 50f;
        public float MaxReleaseVelocity_mmps = 200f;

        public float MaxLateralVelocity_mmps = 300f;
        public float MaxOpposingVelocity_mmps = 65f;

        public double ScrollDelayMs = 450;

        public float MinSwipeLength = 10f;
        public float MaxSwipeWidth = 10f;
        public float SwipeWidthScaling = 0.2f;

        public bool AllowBidirectionalScroll = false;
        public bool AllowHorizontalScroll = true;
        public bool AllowVerticalScroll = true;
    }

    [Serializable]
    public class InteractionConfig : BaseSettings
    {
        public bool UseScrollingOrDragging = true;
        public bool UseSwipeInteraction = false;
        public float DeadzoneRadius = 0.003f;

        public bool InteractionZoneEnabled = false;
        public float InteractionMinDistanceCm = 0.0f;
        public float InteractionMaxDistanceCm = 25.0f;

        public InteractionType InteractionType = InteractionType.PUSH;

        // Interaction-specific settings
        public AirPushSettings AirPush = new AirPushSettings();
        public HoverAndHoldInteractionSettings HoverAndHold = new HoverAndHoldInteractionSettings();
        public TouchPlaneInteractionSettings TouchPlane = new TouchPlaneInteractionSettings();
        public VelocitySwipeSettings VelocitySwipe = new VelocitySwipeSettings();

        public override void SetAllValuesToDefault()
        {
            var defaults = new InteractionConfig();

            UseScrollingOrDragging = defaults.UseScrollingOrDragging;
            UseSwipeInteraction = defaults.UseSwipeInteraction;
            DeadzoneRadius = defaults.DeadzoneRadius;

            InteractionZoneEnabled = defaults.InteractionZoneEnabled;
            InteractionMinDistanceCm = defaults.InteractionMinDistanceCm;
            InteractionMaxDistanceCm = defaults.InteractionMaxDistanceCm;

            InteractionType = defaults.InteractionType;

            foreach(var entry in AirPush)
            {
                entry.Value = defaults.AirPush[entry.Key];
            }

            HoverAndHold.HoverStartTimeS = defaults.HoverAndHold.HoverStartTimeS;
            HoverAndHold.HoverCompleteTimeS = defaults.HoverAndHold.HoverCompleteTimeS;
            TouchPlane.TouchPlaneActivationDistanceCM = defaults.TouchPlane.TouchPlaneActivationDistanceCM;
            TouchPlane.TouchPlaneTrackedPosition = defaults.TouchPlane.TouchPlaneTrackedPosition;

            VelocitySwipe.DownwardsMinVelocityIncrease_mmps = defaults.VelocitySwipe.DownwardsMinVelocityIncrease_mmps;
            VelocitySwipe.MaxLateralVelocity_mmps = defaults.VelocitySwipe.MaxLateralVelocity_mmps;
            VelocitySwipe.MaxOpposingVelocity_mmps = defaults.VelocitySwipe.MaxOpposingVelocity_mmps;
            VelocitySwipe.MaxReleaseVelocity_mmps = defaults.VelocitySwipe.MaxReleaseVelocity_mmps;
            VelocitySwipe.MaxSwipeWidth = defaults.VelocitySwipe.MaxSwipeWidth;
            VelocitySwipe.MinScrollVelocity_mmps = defaults.VelocitySwipe.MinScrollVelocity_mmps;
            VelocitySwipe.MinSwipeLength = defaults.VelocitySwipe.MinSwipeLength;
            VelocitySwipe.ScrollDelayMs = defaults.VelocitySwipe.ScrollDelayMs;
            VelocitySwipe.SwipeWidthScaling = defaults.VelocitySwipe.SwipeWidthScaling;
            VelocitySwipe.UpwardsMinVelocityDecrease_mmps = defaults.VelocitySwipe.UpwardsMinVelocityDecrease_mmps;
            VelocitySwipe.AllowBidirectionalScroll = defaults.VelocitySwipe.AllowBidirectionalScroll;
            VelocitySwipe.AllowHorizontalScroll = defaults.VelocitySwipe.AllowHorizontalScroll;
            VelocitySwipe.AllowVerticalScroll = defaults.VelocitySwipe.AllowVerticalScroll;
        }

        public void SaveConfig()
        {
            InteractionConfigFile.SaveConfig(this);
        }
    }
}

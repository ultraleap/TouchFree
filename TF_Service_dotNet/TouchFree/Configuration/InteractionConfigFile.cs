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
    public class InteractionConfig
    {
        public bool UseScrollingOrDragging = true;
        public bool UseSwipeInteraction = false;
        public float DeadzoneRadius = 0.003f;

        public bool InteractionZoneEnabled = false;
        public float InteractionMinDistanceCm = 0.0f;
        public float InteractionMaxDistanceCm = 25.0f;

        public InteractionType InteractionType = InteractionType.PUSH;

        // Interaction-specific settings
        public HoverAndHoldInteractionSettings HoverAndHold = new HoverAndHoldInteractionSettings();
        public TouchPlaneInteractionSettings TouchPlane = new TouchPlaneInteractionSettings();
        public VelocitySwipeSettings VelocitySwipe = new VelocitySwipeSettings();

        public InteractionConfig()
        {
            this.UseScrollingOrDragging = true;
            this.UseSwipeInteraction = false;
            this.DeadzoneRadius = 0.003f;

            this.InteractionZoneEnabled = false;
            this.InteractionMinDistanceCm = 0.0f;
            this.InteractionMaxDistanceCm = 25.0f;

            this.InteractionType = InteractionType.PUSH;

            // Interaction-specific settings
            this.HoverAndHold = new HoverAndHoldInteractionSettings();
            this.TouchPlane = new TouchPlaneInteractionSettings();
            this.VelocitySwipe = new VelocitySwipeSettings();
        }

        public InteractionConfig(InteractionConfigInternal _internal)
        {
            this.InteractionMinDistanceCm = _internal.InteractionMinDistanceMm / 10f;
            this.InteractionMaxDistanceCm = _internal.InteractionMaxDistanceMm / 10f;

            this.DeadzoneRadius = _internal.DeadzoneRadiusMm / 1000f;

            this.UseScrollingOrDragging = _internal.UseScrollingOrDragging;
            this.UseSwipeInteraction = _internal.UseSwipeInteraction;
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

            this.VelocitySwipe = new VelocitySwipeSettings()
            {
                DownwardsMinVelocityIncrease_mmps = _internal.VelocitySwipe.DownwardsMinVelocityIncrease_mmps,
                MaxLateralVelocity_mmps = _internal.VelocitySwipe.MaxLateralVelocity_mmps,
                MaxOpposingVelocity_mmps= _internal.VelocitySwipe.MaxOpposingVelocity_mmps,
                MaxReleaseVelocity_mmps= _internal.VelocitySwipe.MaxReleaseVelocity_mmps,
                MaxSwipeWidth= _internal.VelocitySwipe.MaxSwipeWidth,
                MinScrollVelocity_mmps= _internal.VelocitySwipe.MinScrollVelocity_mmps,
                MinSwipeLength= _internal.VelocitySwipe.MinSwipeLength,
                ScrollDelayMs= _internal.VelocitySwipe.ScrollDelayMs,
                SwipeWidthScaling= _internal.VelocitySwipe.SwipeWidthScaling,
                UpwardsMinVelocityDecrease_mmps= _internal.VelocitySwipe.UpwardsMinVelocityDecrease_mmps,
                AllowBidirectionalScroll= _internal.VelocitySwipe.AllowBidirectionalScroll,
                AllowHorizontalScroll= _internal.VelocitySwipe.AllowHorizontalScroll,
                AllowVerticalScroll= _internal.VelocitySwipe.AllowVerticalScroll,
            };
        }
    }
}
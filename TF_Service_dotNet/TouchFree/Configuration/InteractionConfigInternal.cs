using System;

namespace Ultraleap.TouchFree.Library.Configuration
{
    [Serializable]
    public record class HoverAndHoldInteractionSettingsInternal
    {
        public float HoverStartTimeS = 0.5f;
        public float HoverCompleteTimeS = 0.6f;
    }

    [Serializable]
    public record class TouchPlaneInteractionSettingsInternal
    {
        public float TouchPlaneActivationDistanceMm = 50f;
        public TrackedPosition TouchPlaneTrackedPosition = TrackedPosition.NEAREST;
    }

    [Serializable]
    public record class VelocitySwipeSettingsInternal
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
    public record class InteractionConfigInternal
    {
        public bool UseScrollingOrDragging = true;
        public bool UseSwipeInteraction = false;
        public float DeadzoneRadiusMm = 3f;

        public bool InteractionZoneEnabled = false;
        public float InteractionMinDistanceMm = 0.0f;
        public float InteractionMaxDistanceMm = 250.0f;

        public InteractionType InteractionType = InteractionType.PUSH;

        // Interaction-specific settings
        public HoverAndHoldInteractionSettingsInternal HoverAndHold = new HoverAndHoldInteractionSettingsInternal();
        public TouchPlaneInteractionSettingsInternal TouchPlane = new TouchPlaneInteractionSettingsInternal();
        public VelocitySwipeSettingsInternal VelocitySwipe = new VelocitySwipeSettingsInternal();

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
                UseSwipeInteraction = UseSwipeInteraction,
                TouchPlane = new TouchPlaneInteractionSettings()
                {
                    TouchPlaneActivationDistanceCm = TouchPlane.TouchPlaneActivationDistanceMm / 10f,
                    TouchPlaneTrackedPosition = TouchPlane.TouchPlaneTrackedPosition
                },
                VelocitySwipe = new VelocitySwipeSettings()
                {
                    MaxOpposingVelocity_mmps = VelocitySwipe.MaxOpposingVelocity_mmps,
                    MaxLateralVelocity_mmps = VelocitySwipe.MaxLateralVelocity_mmps,
                    DownwardsMinVelocityIncrease_mmps = VelocitySwipe.DownwardsMinVelocityIncrease_mmps,
                    MaxReleaseVelocity_mmps = VelocitySwipe.MaxReleaseVelocity_mmps,
                    MaxSwipeWidth = VelocitySwipe.MaxSwipeWidth,
                    MinScrollVelocity_mmps = VelocitySwipe.MinScrollVelocity_mmps,
                    MinSwipeLength = VelocitySwipe.MinSwipeLength,
                    ScrollDelayMs = VelocitySwipe.ScrollDelayMs,
                    SwipeWidthScaling = VelocitySwipe.SwipeWidthScaling,
                    UpwardsMinVelocityDecrease_mmps = VelocitySwipe.UpwardsMinVelocityDecrease_mmps,
                    AllowBidirectionalScroll = VelocitySwipe.AllowBidirectionalScroll,
                    AllowHorizontalScroll = VelocitySwipe.AllowHorizontalScroll,
                    AllowVerticalScroll = VelocitySwipe.AllowVerticalScroll,
                }
            };
        }

        public InteractionConfigInternal()
        {
            this.UseScrollingOrDragging = true;
            this.UseSwipeInteraction = false;
            this.DeadzoneRadiusMm = 3f;

            this.InteractionZoneEnabled = false;
            this.InteractionMinDistanceMm = 0.0f;
            this.InteractionMaxDistanceMm = 250.0f;

            this.InteractionType = InteractionType.PUSH;

            this.HoverAndHold = new HoverAndHoldInteractionSettingsInternal();
            this.TouchPlane = new TouchPlaneInteractionSettingsInternal();
            this.VelocitySwipe = new VelocitySwipeSettingsInternal();
        }

        public InteractionConfigInternal(InteractionConfig fromFile)
        {
            this.InteractionMinDistanceMm = fromFile.InteractionMinDistanceCm * 10f;
            this.InteractionMaxDistanceMm = fromFile.InteractionMaxDistanceCm * 10f;

            this.DeadzoneRadiusMm = fromFile.DeadzoneRadius * 1000;

            this.UseScrollingOrDragging = fromFile.UseScrollingOrDragging;
            this.UseSwipeInteraction = fromFile.UseSwipeInteraction;
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

            this.VelocitySwipe = new VelocitySwipeSettingsInternal()
            {
                DownwardsMinVelocityIncrease_mmps = fromFile.VelocitySwipe.DownwardsMinVelocityIncrease_mmps,
                MaxLateralVelocity_mmps = fromFile.VelocitySwipe.MaxLateralVelocity_mmps,
                MaxOpposingVelocity_mmps = fromFile.VelocitySwipe.MaxOpposingVelocity_mmps,
                MaxReleaseVelocity_mmps = fromFile.VelocitySwipe.MaxReleaseVelocity_mmps,
                MaxSwipeWidth = fromFile.VelocitySwipe.MaxSwipeWidth,
                MinScrollVelocity_mmps = fromFile.VelocitySwipe.MinScrollVelocity_mmps,
                MinSwipeLength = fromFile.VelocitySwipe.MinSwipeLength,
                ScrollDelayMs = fromFile.VelocitySwipe.ScrollDelayMs,
                SwipeWidthScaling = fromFile.VelocitySwipe.SwipeWidthScaling,
                UpwardsMinVelocityDecrease_mmps = fromFile.VelocitySwipe.UpwardsMinVelocityDecrease_mmps,
                AllowBidirectionalScroll = fromFile.VelocitySwipe.AllowBidirectionalScroll,
                AllowHorizontalScroll = fromFile.VelocitySwipe.AllowHorizontalScroll,
                AllowVerticalScroll = fromFile.VelocitySwipe.AllowVerticalScroll,
            };
        }
    }
}
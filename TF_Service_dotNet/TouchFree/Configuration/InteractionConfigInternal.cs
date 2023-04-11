using System;

namespace Ultraleap.TouchFree.Library.Configuration;

[Serializable]
public record HoverAndHoldInteractionSettingsInternal
{
    public float HoverStartTimeS = 0.5f;
    public float HoverCompleteTimeS = 0.6f;
}

[Serializable]
public record TouchPlaneInteractionSettingsInternal
{
    public float TouchPlaneActivationDistanceMm = 50f;
    public TrackedPosition TouchPlaneTrackedPosition = TrackedPosition.NEAREST;
}

[Serializable]
public record AirPushSettingsInternal
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
public record VelocitySwipeSettingsInternal
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
public record InteractionConfigInternal
{
    public bool UseScrollingOrDragging = true;
    public bool UseSwipeInteraction = false;
    public float DeadzoneRadiusMm = 3f;

    public bool InteractionZoneEnabled = false;
    public float InteractionMinDistanceMm = 0.0f;
    public float InteractionMaxDistanceMm = 250.0f;

    public InteractionType InteractionType = InteractionType.PUSH;

    // Interaction-specific settings
    public AirPushSettingsInternal AirPush = new();
    public HoverAndHoldInteractionSettingsInternal HoverAndHold = new();
    public TouchPlaneInteractionSettingsInternal TouchPlane = new();
    public VelocitySwipeSettingsInternal VelocitySwipe = new();

    public InteractionConfig ForApi() =>
        new()
        {
            DeadzoneRadius = DeadzoneRadiusMm / 1000f,
            HoverAndHold = new HoverAndHoldInteractionSettings
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
            TouchPlane = new TouchPlaneInteractionSettings
            {
                TouchPlaneActivationDistanceCm = TouchPlane.TouchPlaneActivationDistanceMm / 10f,
                TouchPlaneTrackedPosition = TouchPlane.TouchPlaneTrackedPosition
            },
            VelocitySwipe = new VelocitySwipeSettings
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

    public InteractionConfigInternal() { /* Defaults set in field initializers */ }

    public InteractionConfigInternal(InteractionConfig cfg)
    {
        InteractionMinDistanceMm = cfg.InteractionMinDistanceCm * 10f;
        InteractionMaxDistanceMm = cfg.InteractionMaxDistanceCm * 10f;

        DeadzoneRadiusMm = cfg.DeadzoneRadius * 1000;

        UseScrollingOrDragging = cfg.UseScrollingOrDragging;
        UseSwipeInteraction = cfg.UseSwipeInteraction;
        InteractionZoneEnabled = cfg.InteractionZoneEnabled;

        InteractionType = cfg.InteractionType;

        AirPush = new AirPushSettingsInternal
        {
            SpeedMin = cfg.AirPush.SpeedMin,
            SpeedMax = cfg.AirPush.SpeedMax,
            DistAtSpeedMinMm = cfg.AirPush.DistAtSpeedMinMm,
            DistAtSpeedMaxMm = cfg.AirPush.DistAtSpeedMaxMm,
            HorizontalDecayDistMm = cfg.AirPush.HorizontalDecayDistMm,

            ThetaOne = cfg.AirPush.ThetaOne,
            ThetaTwo = cfg.AirPush.ThetaTwo,
                
            UnclickThreshold = cfg.AirPush.UnclickThreshold,
            UnclickThresholdDrag = cfg.AirPush.UnclickThresholdDrag,
            DecayForceOnClick = cfg.AirPush.DecayForceOnClick,
            ForceDecayTime = cfg.AirPush.ForceDecayTime,

            UseTouchPlaneForce = cfg.AirPush.UseTouchPlaneForce,
            DistPastTouchPlaneMm = cfg.AirPush.DistPastTouchPlaneMm,

            DragStartDistanceThresholdMm = cfg.AirPush.DragStartDistanceThresholdMm,
            DragDeadzoneShrinkRate = cfg.AirPush.DragDeadzoneShrinkRate,
            DragDeadzoneShrinkDistanceThresholdMm = cfg.AirPush.DragDeadzoneShrinkDistanceThresholdMm,

            DeadzoneMaxSizeIncreaseMm = cfg.AirPush.DeadzoneMaxSizeIncreaseMm,
            DeadzoneShrinkRate = cfg.AirPush.DeadzoneShrinkRate,
        };
        HoverAndHold = new HoverAndHoldInteractionSettingsInternal
        {
            HoverStartTimeS = cfg.HoverAndHold.HoverStartTimeS,
            HoverCompleteTimeS = cfg.HoverAndHold.HoverCompleteTimeS
        };
        TouchPlane = new TouchPlaneInteractionSettingsInternal
        {
            TouchPlaneActivationDistanceMm = cfg.TouchPlane.TouchPlaneActivationDistanceCm * 10f,
            TouchPlaneTrackedPosition = cfg.TouchPlane.TouchPlaneTrackedPosition
        };
        VelocitySwipe = new VelocitySwipeSettingsInternal
        {
            DownwardsMinVelocityIncrease_mmps = cfg.VelocitySwipe.DownwardsMinVelocityIncrease_mmps,
            MaxLateralVelocity_mmps = cfg.VelocitySwipe.MaxLateralVelocity_mmps,
            MaxOpposingVelocity_mmps = cfg.VelocitySwipe.MaxOpposingVelocity_mmps,
            MaxReleaseVelocity_mmps = cfg.VelocitySwipe.MaxReleaseVelocity_mmps,
            MaxSwipeWidth = cfg.VelocitySwipe.MaxSwipeWidth,
            MinScrollVelocity_mmps = cfg.VelocitySwipe.MinScrollVelocity_mmps,
            MinSwipeLength = cfg.VelocitySwipe.MinSwipeLength,
            ScrollDelayMs = cfg.VelocitySwipe.ScrollDelayMs,
            SwipeWidthScaling = cfg.VelocitySwipe.SwipeWidthScaling,
            UpwardsMinVelocityDecrease_mmps = cfg.VelocitySwipe.UpwardsMinVelocityDecrease_mmps,
            AllowBidirectionalScroll = cfg.VelocitySwipe.AllowBidirectionalScroll,
            AllowHorizontalScroll = cfg.VelocitySwipe.AllowHorizontalScroll,
            AllowVerticalScroll = cfg.VelocitySwipe.AllowVerticalScroll,
        };
    }
}
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.Numerics;
using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Library.Connections;

namespace Ultraleap.TouchFree.Library.Interactions.InteractionModules;

public class AirPushInteraction : InteractionModule
{
    public override InteractionType InteractionType => InteractionType.PUSH;

    private readonly double _millisecondsCooldownOnEntry = 300.0;
    private readonly double _clickHoldTimerMs = 1500.0;

    private readonly Stopwatch _clickHoldStopwatch = new();
    private readonly TimestampStopwatch _handAppearedStopwatch = new();

    // Speed in millimeters per second
    private float _speedMin = 150f;
    private float _speedMax = 500f;
    private float _distAtSpeedMinMm = 42f;
    private float _distAtSpeedMaxMm = 8f;
    private float _horizontalDecayDistMm = 50f;

    private float _thetaOne = 65f;
    private float _thetaTwo = 135f;
    // If a hand moves an angle less than thetaOne, this is "towards" the screen
    // If a hand moves an angle greater than thetaTwo, this is "backwards" from the screen
    // If a hand moves between the two angles, this is "horizontal" to the screen

    private float _unclickThreshold = 0.97f;
    private float _unclickThresholdDrag = 0.97f;
    private bool _decayForceOnClick = true;
    private float _forceDecayTime = 0.1f;
    private bool _decayingForce;

    private bool _useTouchPlaneForce = true;
    private float _distPastTouchPlaneMm = 20f;

    private float _dragStartDistanceThresholdMm = 30f;
    private float _dragDeadzoneShrinkRate = 0.9f;
    private float _dragDeadzoneShrinkDistanceThresholdMm = 10f;

    private float _deadzoneMaxSizeIncreaseMm = 20f;
    private float _deadzoneShrinkRate = 0.8f;

    private Vector2 _cursorPressPosition;

    private long _previousTime = 0;
    private float _previousScreenDistanceMm = float.PositiveInfinity;
    private Vector2 _previousScreenPos = Vector2.Zero;

    private float _appliedForce = 0f;
    private bool _pressing = false;

    private bool _isDragging = false;

    private readonly ExtrapolationPositionModifier _extrapolation;
    private readonly PositionFilter _filter;

    public AirPushInteraction(
        IHandManager handManager,
        IVirtualScreen virtualScreen,
        IConfigManager configManager,
        IClientConnectionManager connectionManager,
        IOptions<InteractionTuning> interactionTuning,
        IPositioningModule positioningModule,
        IPositionStabiliser positionStabiliser) : base(handManager, virtualScreen, configManager, connectionManager, positioningModule, positionStabiliser)
    {
        var config = configManager.InteractionConfig?.AirPush;
        if (config != null)
        {
            _speedMin = config.SpeedMin;
            _speedMax = config.SpeedMax;
            _distAtSpeedMinMm = config.DistAtSpeedMinMm;
            _distAtSpeedMaxMm = config.DistAtSpeedMaxMm;
            _horizontalDecayDistMm = config.HorizontalDecayDistMm;
            _thetaOne = config.ThetaOne;
            _thetaTwo = config.ThetaTwo;
            _unclickThreshold = config.UnclickThreshold;
            _unclickThresholdDrag = config.UnclickThresholdDrag;
            _decayForceOnClick = config.DecayForceOnClick;
            _forceDecayTime = config.ForceDecayTime;

            _useTouchPlaneForce = config.UseTouchPlaneForce;
            _distPastTouchPlaneMm = config.DistPastTouchPlaneMm;

            _dragStartDistanceThresholdMm = config.DragStartDistanceThresholdMm;
            _dragDeadzoneShrinkRate = config.DragDeadzoneShrinkRate;
            _dragDeadzoneShrinkDistanceThresholdMm = config.DragDeadzoneShrinkDistanceThresholdMm;

            _deadzoneMaxSizeIncreaseMm = config.DeadzoneMaxSizeIncreaseMm;
            _deadzoneShrinkRate = config.DeadzoneShrinkRate;
        }
        _extrapolation = new ExtrapolationPositionModifier(interactionTuning);
        _filter = new PositionFilter(interactionTuning);

        PositionConfiguration = new[]
        {
            new PositionTrackerConfiguration(TrackedPosition.INDEX_STABLE, 1)
        };
    }

    protected override void OnInteractionSettingsUpdated(InteractionConfigInternal interactionConfig)
    {
        base.OnInteractionSettingsUpdated(interactionConfig);

        var config = interactionConfig.AirPush;

        _speedMin = config.SpeedMin;
        _speedMax = config.SpeedMax;
        _distAtSpeedMinMm = config.DistAtSpeedMinMm;
        _distAtSpeedMaxMm = config.DistAtSpeedMaxMm;
        _horizontalDecayDistMm = config.HorizontalDecayDistMm;
        _thetaOne = IgnoreDragging || IgnoreSwiping ? 15f : config.ThetaOne;
        _thetaTwo = config.ThetaTwo;
        _unclickThreshold = config.UnclickThreshold;
        _unclickThresholdDrag = config.UnclickThresholdDrag;
        _decayForceOnClick = config.DecayForceOnClick;
        _forceDecayTime = config.ForceDecayTime;

        _useTouchPlaneForce = config.UseTouchPlaneForce;
        _distPastTouchPlaneMm = config.DistPastTouchPlaneMm;

        _dragStartDistanceThresholdMm = config.DragStartDistanceThresholdMm;
        _dragDeadzoneShrinkRate = config.DragDeadzoneShrinkRate;
        _dragDeadzoneShrinkDistanceThresholdMm = config.DragDeadzoneShrinkDistanceThresholdMm;

        _deadzoneMaxSizeIncreaseMm = config.DeadzoneMaxSizeIncreaseMm;
        _deadzoneShrinkRate = config.DeadzoneShrinkRate;
    }

    protected override Positions ApplyAdditionalPositionModifiers(Positions pos) =>
        base.ApplyAdditionalPositionModifiers(pos)
            .ApplyModifier(_extrapolation)
            .ApplyModifier(_filter);

    protected override InputActionResult UpdateData(Leap.Hand hand, float confidence)
    {
        if (hand == null)
        {
            _appliedForce = 0f;
            _pressing = false;
            _isDragging = false;
            // Restarts the hand timer every frame that we have no active hand
            _handAppearedStopwatch.Restart(LatestTimestamp);

            if (HadHandLastFrame)
            {
                // We lost the hand so cancel anything we may have been doing
                return CreateInputActionResult(InputType.CANCEL, positions, _appliedForce);
            }

            return new InputActionResult();
        }

        return HandleInteractionsAirPush(confidence);
    }

    private InputActionResult HandleInteractionsAirPush(float confidence)
    {
        InputActionResult inputActionResult;
        long currentTimestamp = LatestTimestamp;

        if (_handAppearedStopwatch.HasBeenRunningForThreshold(currentTimestamp, _millisecondsCooldownOnEntry))
        {
            _handAppearedStopwatch.Stop();
        }

        // If not ignoring clicks...
        if ((_previousTime != 0f) && !_handAppearedStopwatch.IsRunning)
        {
            // Calculate important variables needed in determining the key events
            long dtMicroseconds = (currentTimestamp - _previousTime);
            float dt = dtMicroseconds / (1000f * 1000f);     // Seconds
            float dz = (-1f) * (DistanceFromScreenMm - _previousScreenDistanceMm);   // Millimetres +ve = towards screen
            float currentVelocity = dz / dt;    // mm/s

            Vector2 dPerpPx = positions.CursorPosition - _previousScreenPos;
            Vector2 dPerp = VirtualScreen.PixelsToMillimeters(dPerpPx);

            // Multiply by confidence to make it harder to use when disused
            float forceChange = GetAppliedForceChange(currentVelocity, dt, dPerp, DistanceFromScreenMm) * confidence;
            // Update AppliedForce, which is the crux of the AirPush algorithm
            _appliedForce += forceChange;
            _appliedForce = Math.Clamp(_appliedForce, 0f, 1f);

            // Update the deadzone size
            if (!_pressing)
            {
                AdjustDeadzoneSize(forceChange);
            }

            // Determine whether to send any other events
            if (_pressing)
            {
                if ((!_isDragging && _appliedForce < _unclickThreshold) ||
                    (_isDragging && _appliedForce < _unclickThresholdDrag) ||
                    IgnoreDragging ||
                    (_clickHoldStopwatch.IsRunning && _clickHoldStopwatch.ElapsedMilliseconds >= _clickHoldTimerMs))
                {
                    _pressing = false;
                    _isDragging = false;
                    _cursorPressPosition = Vector2.Zero;
                    inputActionResult = CreateInputActionResult(InputType.UP, positions, _appliedForce);
                    _clickHoldStopwatch.Stop();

                    _decayingForce = true;
                }
                else
                {
                    if (_isDragging)
                    {
                        inputActionResult = CreateInputActionResult(InputType.MOVE, positions, _appliedForce);
                        PositionStabiliser.ReduceDeadzoneOffset();
                    }
                    else if (CheckForStartDrag(_cursorPressPosition, positions.CursorPosition))
                    {
                        _isDragging = true;
                        inputActionResult = CreateInputActionResult(InputType.MOVE, positions, _appliedForce);
                        PositionStabiliser.StartShrinkingDeadzone(_dragDeadzoneShrinkRate);
                        _clickHoldStopwatch.Stop();
                    }
                    else
                    {
                        // NONE causes the client to react to data without using Input.
                        inputActionResult = CreateInputActionResult(InputType.NONE, positions, _appliedForce);
                    }
                }
            }
            else if (!_decayingForce && _appliedForce >= 1f)
            {
                // Need the !decayingForce check here to eliminate the risk of double-clicks

                _pressing = true;
                inputActionResult = CreateInputActionResult(InputType.DOWN, positions, _appliedForce);
                _cursorPressPosition = positions.CursorPosition;

                if (!IgnoreDragging)
                {
                    _clickHoldStopwatch.Restart();
                }

                PositionStabiliser.SetDeadzoneOffset();
                PositionStabiliser.CurrentDeadzoneRadius = _dragStartDistanceThresholdMm;
            }
            else if (positions.CursorPosition != _previousScreenPos || DistanceFromScreenMm != _previousScreenDistanceMm)
            {
                // Send the move event
                inputActionResult = CreateInputActionResult(InputType.MOVE, positions, _appliedForce);
                PositionStabiliser.ReduceDeadzoneOffset();
            }
            else
            {
                // Send the move event
                inputActionResult = CreateInputActionResult(InputType.MOVE, positions, _appliedForce);
            }

            if (_decayingForce && (_appliedForce <= _unclickThreshold - 0.1f))
            {
                _decayingForce = false;
            }
        }
        else
        {
            // show them they have been seen but send no major events as we have only just discovered the hand
            inputActionResult = CreateInputActionResult(InputType.MOVE, positions, _appliedForce);
        }

        // Update stored variables
        _previousTime = currentTimestamp;
        _previousScreenDistanceMm = DistanceFromScreenMm;
        _previousScreenPos = positions.CursorPosition;

        return inputActionResult;
    }

    private void AdjustDeadzoneSize(float df)
    {
        // _df is change in Force in the last frame
        if (df < -1f * float.Epsilon)
        {
            // Start decreasing deadzone size
            PositionStabiliser.StartShrinkingDeadzone(_deadzoneShrinkRate);
        }
        else
        {
            PositionStabiliser.StopShrinkingDeadzone();

            float deadzoneSizeIncrease = _deadzoneMaxSizeIncreaseMm * df;

            float deadzoneMinSize = PositionStabiliser.DefaultDeadzoneRadius;
            float deadzoneMaxSize = deadzoneMinSize + _deadzoneMaxSizeIncreaseMm;

            float newDeadzoneSize = PositionStabiliser.CurrentDeadzoneRadius + deadzoneSizeIncrease;
            newDeadzoneSize = Math.Clamp(newDeadzoneSize, deadzoneMinSize, deadzoneMaxSize);
            PositionStabiliser.CurrentDeadzoneRadius = newDeadzoneSize;
        }
    }

    private float GetAppliedForceChange(float currentVelocity, float dt, Vector2 dPerp, float distanceFromTouchPlane)
    {
        // currentVelocity = current z-component of velocity in mm/s
        // dt = current change in time in seconds
        // dPerp = horizontal change in position
        // distanceFromTouchPlane = z-distance from a virtual plane where clicks are always triggered

        float forceChange = 0f;

        if (dt < float.Epsilon)
        {
            // Not long enough between the two frames
            // Also triggered if recognising a new hand (dt is negative)

            // No change in force
            forceChange = 0f;
        }
        else if (_useTouchPlaneForce && distanceFromTouchPlane < 0f)
        {
            // Use a fixed stiffness beyond the touch-plane
            float stiffness = 1f / _distPastTouchPlaneMm;

            // Do not reduce force on backwards motion
            float forwardVelocity = Math.Max(0f, currentVelocity);
            forceChange = stiffness * forwardVelocity * dt;

            // Do not decay force when beyond the touch plane.
            // This is to ensure the user cannot edge closer and closer to the screen.
        }
        else
        {
            float angleFromScreen = (float)Math.Atan2(
                dPerp.Length(),
                currentVelocity * dt) * Utilities.RADTODEG;

            if (angleFromScreen < _thetaOne || angleFromScreen > _thetaTwo)
            {
                // Towards screen: Increase force
                // Moving towards or away from screen.
                // Adjust force based on spring stiffness

                // Perform a calculation:
                float vClamped = Math.Clamp(Math.Abs(currentVelocity), _speedMin, _speedMax);

                float stiffnessRatio = (vClamped - _speedMin) / (_speedMax - _speedMin);

                float stiffnessMin = 1f / _distAtSpeedMinMm;
                float stiffnessMax = 1f / _distAtSpeedMaxMm;

                float k = stiffnessMin + (stiffnessRatio * stiffnessRatio) * (stiffnessMax - stiffnessMin);

                forceChange = k * currentVelocity * dt;
            }
            else
            {
                // Approximately horizontal movement.
                if (_pressing)
                {
                    // If pressing, do not change
                    forceChange = 0f;
                }
                else
                {
                    // Change force based on horizontal velocity and a horizontal decay distance
                    float vPerp = dPerp.Length() / dt;

                    float stiffness = 1f / _horizontalDecayDistMm;
                    forceChange = -1f * stiffness * vPerp * dt;
                }
            }

            // If the forceChange is increasing, do not apply the decay
            if (_decayingForce)
            {
                if (forceChange <= 0f)
                {
                    forceChange -= (1f - (_unclickThreshold - 0.1f)) * (dt / _forceDecayTime);
                }
                else
                {
                    // Do not increase the force if it's supposed to be decreasing
                    forceChange = 0f;
                }
            }
        }
        return forceChange;
    }
}
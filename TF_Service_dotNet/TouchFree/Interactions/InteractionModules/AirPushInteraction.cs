using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Microsoft.Extensions.Options;
using Ultraleap.TouchFree.Library;
using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Library.Interactions.InteractionModules;

namespace Ultraleap.TouchFree.Library.Interactions
{
    public class AirPushInteraction : InteractionModule
    {
        public override InteractionType InteractionType { get; } = InteractionType.PUSH;

        public double millisecondsCooldownOnEntry = 300.0;
        public double clickHoldTimerMs = 1500.0;

        Stopwatch handAppearedCooldown = new Stopwatch();
        Stopwatch clickHoldStopwatch = new Stopwatch();

        // Speed in millimeters per second
        public float speedMin = 150f;
        public float speedMax = 500f;
        public float distAtSpeedMinMm = 42f;
        public float distAtSpeedMaxMm = 8f;
        public float horizontalDecayDistMm = 50f;

        public float thetaOne = 22f;
        public float thetaTwo = 135f;
        // If a hand moves an angle less than thetaOne, this is "towards" the screen
        // If a hand moves an angle greater than thetaTwo, this is "backwards" from the screen
        // If a hand moves between the two angles, this is "horizontal" to the screen

        public float unclickThreshold = 0.97f;
        public float unclickThresholdDrag = 0.97f;
        public bool decayForceOnClick = true;
        public float forceDecayTime = 0.1f;
        bool decayingForce;

        public bool useTouchPlaneForce = true;
        public float distPastTouchPlaneMm = 20f;

        public float dragStartDistanceThresholdMm = 30f;
        public float dragDeadzoneShrinkRate = 0.9f;
        public float dragDeadzoneShrinkDistanceThresholdMm = 10f;

        public float deadzoneMaxSizeIncreaseMm = 20f;
        public float deadzoneShrinkRate = 0.8f;

        private Vector2 cursorPressPosition;

        private long previousTime = 0;
        private float previousScreenDistanceMm = float.PositiveInfinity;
        private Vector2 previousScreenPos = Vector2.Zero;

        private float appliedForce = 0f;
        private bool pressing = false;

        private bool isDragging = false;

        private readonly ExtrapolationPositionModifier extrapolation;
        private readonly PositionFilter filter;

        public AirPushInteraction(
            HandManager _handManager,
            IVirtualScreen _virtualScreen,
            IConfigManager _configManager,
            IOptions<InteractionTuning> _interactionTuning,
            IPositioningModule _positioningModule,
            IPositionStabiliser _positionStabiliser) : base(_handManager, _virtualScreen, _configManager, _positioningModule, _positionStabiliser)
        {
            if (_interactionTuning?.Value?.AirPushSettings != null)
            {
                speedMin = _interactionTuning.Value.AirPushSettings.SpeedMin;
                speedMax = _interactionTuning.Value.AirPushSettings.SpeedMax;
                distAtSpeedMinMm = _interactionTuning.Value.AirPushSettings.DistAtSpeedMinMm;
                distAtSpeedMaxMm = _interactionTuning.Value.AirPushSettings.DistAtSpeedMaxMm;
                horizontalDecayDistMm = _interactionTuning.Value.AirPushSettings.HorizontalDecayDistMm;
                thetaOne = _interactionTuning.Value.AirPushSettings.ThetaOne;
                thetaTwo = _interactionTuning.Value.AirPushSettings.ThetaTwo;
                unclickThreshold = _interactionTuning.Value.AirPushSettings.UnclickThreshold;
                unclickThresholdDrag = _interactionTuning.Value.AirPushSettings.UnclickThresholdDrag;
            }
            extrapolation = new ExtrapolationPositionModifier(_interactionTuning);
            filter = new PositionFilter(_interactionTuning);

            positionConfiguration = new[]
            {
                new PositionTrackerConfiguration(TrackedPosition.INDEX_STABLE, 1)
            };
        }

        protected override Positions ApplyAdditionalPositionModifiers(Positions positions)
        {
            var returnPositions = base.ApplyAdditionalPositionModifiers(positions);
            returnPositions.CursorPosition = extrapolation.ApplyModification(returnPositions.CursorPosition);
            returnPositions.CursorPosition = filter.ApplyModification(returnPositions.CursorPosition);
            return returnPositions;
        }

        protected override InputActionResult UpdateData(Leap.Hand hand, float confidence)
        {
            if (hand == null)
            {
                appliedForce = 0f;
                pressing = false;
                isDragging = false;
                // Restarts the hand timer every frame that we have no active hand
                handAppearedCooldown.Restart();

                if(hadHandLastFrame)
                {
                    // We lost the hand so cancel anything we may have been doing
                    return CreateInputActionResult(InputType.CANCEL, positions, appliedForce);
                }

                return new InputActionResult();
            }

            return HandleInteractionsAirPush(confidence);
        }

        private InputActionResult HandleInteractionsAirPush(float confidence)
        {
            var inputActionResult =  new InputActionResult();
            long currentTimestamp = latestTimestamp;

            if (handAppearedCooldown.IsRunning && handAppearedCooldown.ElapsedMilliseconds >= millisecondsCooldownOnEntry)
            {
                handAppearedCooldown.Stop();
            }

            // If not ignoring clicks...
            if ((previousTime != 0f) && !handAppearedCooldown.IsRunning)
            {
                // Calculate important variables needed in determining the key events
                long dtMicroseconds = (currentTimestamp - previousTime);
                float dt = dtMicroseconds / (1000f * 1000f);     // Seconds
                float dz = (-1f) * (distanceFromScreenMm - previousScreenDistanceMm);   // Millimetres +ve = towards screen
                float currentVelocity = dz / dt;    // mm/s

                Vector2 dPerpPx = positions.CursorPosition - previousScreenPos;
                Vector2 dPerp = virtualScreen.PixelsToMillimeters(dPerpPx);

                // Update AppliedForce, which is the crux of the AirPush algorithm
                float forceChange = GetAppliedForceChange(currentVelocity, dt, dPerp, distanceFromScreenMm) * confidence;  // Multiply by confidence to make it harder to use when disused
                appliedForce += forceChange;
                appliedForce = Math.Clamp(appliedForce, 0f, 1f);

                // Update the deadzone size
                if (!pressing)
                {
                    AdjustDeadzoneSize(forceChange);
                }

                // Determine whether to send any other events
                if (pressing)
                {
                    if ((!isDragging && appliedForce < unclickThreshold) || 
                        (isDragging && appliedForce < unclickThresholdDrag) || 
                        ignoreDragging ||
                        (clickHoldStopwatch.IsRunning && clickHoldStopwatch.ElapsedMilliseconds >= clickHoldTimerMs))
                    {
                        pressing = false;
                        isDragging = false;
                        cursorPressPosition = Vector2.Zero;
                        inputActionResult = CreateInputActionResult(InputType.UP, positions, appliedForce);
                        clickHoldStopwatch.Stop();

                        decayingForce = true;
                    }
                    else
                    {
                        if (isDragging)
                        {
                            inputActionResult = CreateInputActionResult(InputType.MOVE, positions, appliedForce);
                            positionStabiliser.ReduceDeadzoneOffset();
                        }
                        else if (CheckForStartDrag(cursorPressPosition, positions.CursorPosition))
                        {
                            isDragging = true;
                            inputActionResult = CreateInputActionResult(InputType.MOVE, positions, appliedForce);
                            positionStabiliser.StartShrinkingDeadzone(dragDeadzoneShrinkRate);
                            clickHoldStopwatch.Stop();
                        }
                        else
                        {
                            // NONE causes the client to react to data without using Input.
                            inputActionResult = CreateInputActionResult(InputType.NONE, positions, appliedForce);
                        }
                    }
                }
                else if (!decayingForce && appliedForce >= 1f)
                {
                    // Need the !decayingForce check here to eliminate the risk of double-clicks

                    pressing = true;
                    inputActionResult = CreateInputActionResult(InputType.DOWN, positions, appliedForce);
                    cursorPressPosition = positions.CursorPosition;

                    if(!ignoreDragging)
                    {
                        clickHoldStopwatch.Restart();
                    }

                    positionStabiliser.SetDeadzoneOffset();
                    positionStabiliser.currentDeadzoneRadius = dragStartDistanceThresholdMm;
                }
                else if (positions.CursorPosition != previousScreenPos || distanceFromScreenMm != previousScreenDistanceMm)
                {
                    // Send the move event
                    inputActionResult = CreateInputActionResult(InputType.MOVE, positions, appliedForce);
                    positionStabiliser.ReduceDeadzoneOffset();
                }

                if (decayingForce && (appliedForce <= unclickThreshold - 0.1f))
                {
                    decayingForce = false;
                }
            }
            else
            {
                // show them they have been seen but send no major events as we have only just discovered the hand
                inputActionResult = CreateInputActionResult(InputType.MOVE, positions, appliedForce);
            }

            // Update stored variables
            previousTime = currentTimestamp;
            previousScreenDistanceMm = distanceFromScreenMm;
            previousScreenPos = positions.CursorPosition;

            return inputActionResult;
        }


        /// <summary>
        /// Check if any movement has happened, if it has, we have left the deadzone
        /// </summary>
        private static bool CheckForStartDrag(Vector2 _startPos, Vector2 _currentPos)
        {
            return _startPos != _currentPos;
        }

        private void AdjustDeadzoneSize(float _df)
        {
            // _df is change in Force in the last frame
            if (_df < -1f * float.Epsilon)
            {
                // Start decreasing deadzone size
                positionStabiliser.StartShrinkingDeadzone(deadzoneShrinkRate);
            }
            else
            {
                positionStabiliser.StopShrinkingDeadzone();

                float deadzoneSizeIncrease = deadzoneMaxSizeIncreaseMm * _df;

                float deadzoneMinSize = positionStabiliser.defaultDeadzoneRadius;
                float deadzoneMaxSize = deadzoneMinSize + deadzoneMaxSizeIncreaseMm;

                float newDeadzoneSize = positionStabiliser.currentDeadzoneRadius + deadzoneSizeIncrease;
                newDeadzoneSize = Math.Clamp(newDeadzoneSize, deadzoneMinSize, deadzoneMaxSize);
                positionStabiliser.currentDeadzoneRadius = newDeadzoneSize;
            }
        }

        private float GetAppliedForceChange(float _currentVelocity, float _dt, Vector2 _dPerp, float _distanceFromTouchPlane)
        {
            // currentVelocity = current z-component of velocity in mm/s
            // dt = current change in time in seconds
            // dPerp = horizontal change in position
            // distanceFromTouchPlane = z-distance from a virtual plane where clicks are always triggered

            float forceChange = 0f;

            if (_dt < float.Epsilon)
            {
                // Not long enough between the two frames
                // Also triggered if recognising a new hand (dt is negative)

                // No change in force
                forceChange = 0f;
            }
            else if (useTouchPlaneForce && _distanceFromTouchPlane < 0f)
            {
                // Use a fixed stiffness beyond the touch-plane
                float stiffness = 1f / distPastTouchPlaneMm;

                // Do not reduce force on backwards motion
                float forwardVelocity = Math.Max(0f, _currentVelocity);
                forceChange = stiffness * forwardVelocity * _dt;

                // Do not decay force when beyond the touch plane.
                // This is to ensure the user cannot edge closer and closer to the screen.
            }
            else
            {
                float angleFromScreen = (float) Math.Atan2(
                    _dPerp.Length(),
                    _currentVelocity * _dt) * Utilities.RADTODEG;

                if (angleFromScreen < thetaOne || angleFromScreen > thetaTwo)
                {
                    // Towards screen: Increase force
                    // Moving towards or away from screen.
                    // Adjust force based on spring stiffness

                    // Perform a calculation:
                    float vClamped = Math.Clamp(Math.Abs(_currentVelocity), speedMin, speedMax);

                    float stiffnessRatio = (vClamped - speedMin) / (speedMax - speedMin);

                    float stiffnessMin = 1f / distAtSpeedMinMm;
                    float stiffnessMax = 1f / distAtSpeedMaxMm;

                    float k = stiffnessMin + (stiffnessRatio * stiffnessRatio) * (stiffnessMax - stiffnessMin);

                    forceChange = k * _currentVelocity * _dt;
                }
                else
                {
                    // Approximately horizontal movement.
                    if (pressing)
                    {
                        // If pressing, do not change
                        forceChange = 0f;
                    }
                    else
                    {
                        // Change force based on horizontal velocity and a horizontal decay distance
                        float vPerp = _dPerp.Length() / _dt;

                        float stiffness = 1f / horizontalDecayDistMm;
                        forceChange = -1f * stiffness * vPerp * _dt;
                    }
                }

                // If the forceChange is increasing, do not apply the decay
                if (decayingForce)
                {
                    if (forceChange <= 0f)
                    {
                        forceChange -= (1f - (unclickThreshold - 0.1f)) * (_dt / forceDecayTime);
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
}
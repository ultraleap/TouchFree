using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;
using System.Collections.Generic;
using Ultraleap.ScreenControl.Core.ScreenControlTypes;

namespace Ultraleap.ScreenControl.Core
{
    public class AirPushInteraction : InteractionModule
    {
        public override InteractionType InteractionType { get; } = InteractionType.Push;

        public bool InteractionEnabled { get; set; } = true;

        [Header("Hand Entry")]
        public double millisecondsCooldownOnEntry;
        bool handLastSeen = false;
        Stopwatch handAppearedCooldown = new Stopwatch();

        [Header("AirPush Detection")]
        public float speedMin;
        public float speedMax;
        public float distAtSpeedMin;
        public float distAtSpeedMax;
        public AnimationCurve stiffnessCurve;
        public float horizontalDecayDist;

        [Header("AirPush Angles")]
        [Range(0, 180)]
        public float thetaOne;
        [Range(0, 180)]
        public float thetaTwo;
        // If a hand moves an angle less than thetaOne, this is "towards" the screen
        // If a hand moves an angle greater than thetaTwo, this is "backwards" from the screen
        // If a hand moves between the two angles, this is "horizontal" to the screen 

        [Header("AirPush Click")]
        public bool clampOnPress;
        private Vector2 cursorPressPosition;
        private Vector2 clickPressPosition;

        [Header("AirPush Unclick")]

        [Range(0, 0.999f)]
        public float unclickThreshold = 0.999f;
        public bool decayForceOnClick;
        [Range(0, 0.999f)]
        public float decayThreshold;
        public float forceDecayTime;
        bool decayingForce;

        [Header("TouchPlane Params")]
        public bool useTouchPlaneForce;
        public float distPastTouchPlane;

        private long previousTime = 0;
        private float previousScreenDistance = Mathf.Infinity;
        private Vector2 previousScreenPos = Vector2.zero;

        private float appliedForce = 0f;
        private bool pressing = false;

        // Instant-unclick params
        private int REQUIRED_HOLD_FRAMES = 1;
        private int heldFrames = 0;
        private bool requireHold = false;

        [Header("Dragging")]
        public float dragStartDistanceThresholdM = 0.04f;
        public float dragStartTimeDelaySecs = 0.1f;
        public float dragDeadzoneShrinkRate = 0.5f;
        public float dragDeadzoneShrinkDistanceThresholdM = 0.001f;

        [Header("Deadzone")]
        public float deadzoneSizeIncreaseAtMaxForce;
        public float deadzoneMaxSizeIncrease;
        public float deadzoneShrinkRate;

        private bool dragDeadzoneShrinkTriggered = false;
        private Stopwatch dragStartTimer = new Stopwatch();
        private Vector2 downPos;
        private bool isDragging = false;

        [Header("Debug")]
        public bool logData = false;
        public DataLogger dataLogger;

        protected override void UpdateData(Leap.Hand hand)
        {
            if (hand == null)
            {
                handLastSeen = false;
                appliedForce = 0f;
                pressing = false;
                isDragging = false;
                handAppearedCooldown.Stop();
                return;
            }

            if (!InteractionEnabled)
            {
                return;
            }

            positions = positioningModule.CalculatePositions(hand);

            HandleInteractionsAirPush(hand);
        }

        private void HandleInteractionsAirPush(Leap.Hand hand)
        {
            long currentTimestamp = latestTimestamp;

            Vector2 cursorPosition = positions.CursorPosition;
            float distanceFromScreen = positions.DistanceFromScreen;

            /**
                Update the hand-appeared cooldown timer if needed.

                Do not allow clicks until a certain amount of time has elapsed after a hand first appears.
            */
            if (!handLastSeen)
            {
                // Start a hand appeared cooldown timer
                handAppearedCooldown.Restart();
            }
            else
            {
                if (handAppearedCooldown.ElapsedMilliseconds >= millisecondsCooldownOnEntry | !handAppearedCooldown.IsRunning)
                {
                    handAppearedCooldown.Stop();
                }
            }
            handLastSeen = true;

            // If not ignoring clicks...
            if ((previousTime != 0f) && !handAppearedCooldown.IsRunning)
            {
                // Calculate important variables needed in determining the key events
                long dtMicroseconds = (currentTimestamp - previousTime);
                float dt = dtMicroseconds / (1000f * 1000f);     // Seconds
                float dz = (-1f) * (distanceFromScreen - previousScreenDistance);   // Metres +ve = towards screen
                float currentVelocity = dz / dt;    // m/s

                Vector2 dPerpPx = cursorPosition - previousScreenPos;
                Vector2 dPerp = GlobalSettings.virtualScreen.PixelsToMeters(dPerpPx);

                // Update AppliedForce, which is the crux of the AirPush algorithm
                float forceChange = GetAppliedForceChange(currentVelocity, dt, dPerp, distanceFromScreen);
                appliedForce += forceChange;
                appliedForce = Mathf.Clamp01(appliedForce);

                // Optionally log data to a logger, for post-processing
                if (logData)
                {
                    dataLogger.LogData(
                        new List<float>()
                        {
                        currentTimestamp,
                        distanceFromScreen,
                        dt,
                        dz,
                        dPerp.magnitude,
                        appliedForce
                        }
                    );
                }

                // Update the deadzone size
                if (!pressing)
                {
                    AdjustDeadzoneSize(forceChange);
                }

                // Calculate the positions to use for events
                Positions eventPositions = new Positions(Vector2.zero, distanceFromScreen);
                if (pressing && ignoreDragging && clampOnPress)
                {
                    eventPositions.CursorPosition = cursorPressPosition;
                }
                else
                {
                    eventPositions.CursorPosition = cursorPosition;
                }

                // Send the move event
                SendInputAction(InputType.MOVE, eventPositions, appliedForce);

                // Determine whether to send any other events
                if (pressing)
                {
                    if (requireHold)
                    {
                        requireHold = false;
                    }
                    else if (appliedForce < unclickThreshold || ignoreDragging)
                    {
                        pressing = false;
                        isDragging = false;
                        cursorPressPosition = Vector2.zero;
                        clickPressPosition = Vector2.zero;
                        SendInputAction(InputType.UP, eventPositions, appliedForce);
                    }
                    else
                    {
                        if (isDragging)
                        {
                            if (!dragDeadzoneShrinkTriggered && CheckForStartDragDeadzoneShrink(cursorPressPosition, cursorPosition))
                            {
                                positioningModule.Stabiliser.StartShrinkingDeadzone(ShrinkType.MOTION_BASED, dragDeadzoneShrinkRate);
                                dragDeadzoneShrinkTriggered = true;
                            }
                        }
                        else
                        {
                            if (!ignoreDragging && CheckForStartDrag(cursorPressPosition, cursorPosition))
                            {
                                isDragging = true;
                                dragDeadzoneShrinkTriggered = false;
                            }
                        }

                    }
                }
                else if (!decayingForce && appliedForce >= 1f)
                {
                    // Need the !decayingForce check here to eliminate the risk of double-clicks
                    // when ignoring dragging and moving past the touch-plane.

                    pressing = true;
                    SendInputAction(InputType.DOWN, eventPositions, appliedForce);
                    cursorPressPosition = cursorPosition;

                    // If dragging is off, we want to decay the force after a click back to the unclick threshold
                    if (decayForceOnClick && ignoreDragging)
                    {
                        decayingForce = true;
                    }

                    // This might be only true if "ignoreDragging" is on, but I think because Pokes can be done quite quickly,
                    // there's a chance that no frame is triggered 
                    requireHold = true;
                }

                if (decayingForce && (appliedForce <= decayThreshold))
                {
                    decayingForce = false;
                }
            }
            else
            {
                // If ignoring clicks, just move horizontally
                SendInputAction(InputType.MOVE, positions, 0f);
            }

            // Update stored variables
            previousTime = currentTimestamp;
            previousScreenDistance = distanceFromScreen;
            previousScreenPos = cursorPosition;
        }

        private bool CheckForStartDrag(Vector2 _startPos, Vector2 _currentPos)
        {
            Vector2 startPosM = GlobalSettings.virtualScreen.PixelsToMeters(_startPos);
            Vector2 currentPosM = GlobalSettings.virtualScreen.PixelsToMeters(_currentPos);
            float distFromStartPos = (startPosM - currentPosM).magnitude;
            if (distFromStartPos > dragStartDistanceThresholdM)
            {
                return true;
            }

            if (dragStartTimer.ElapsedMilliseconds >= dragStartTimeDelaySecs * 1000f)
            {
                //Debug.Log("Drag started: time");
                dragStartTimer.Stop();
                return true;
            }

            return false;
        }

        private bool CheckForStartDragDeadzoneShrink(Vector2 _startPos, Vector2 _currentPos)
        {
            Vector2 startPosM = GlobalSettings.virtualScreen.PixelsToMeters(_startPos);
            Vector2 currentPosM = GlobalSettings.virtualScreen.PixelsToMeters(_currentPos);
            float distFromStartPos = (startPosM - currentPosM).magnitude;
            return (distFromStartPos > dragDeadzoneShrinkDistanceThresholdM);
        }

        private void AdjustDeadzoneSize(float _df)
        {
            // _df is change in Force in the last frame
            if (_df < -1f * Mathf.Epsilon)
            {
                // Start decreasing deadzone size
                positioningModule.Stabiliser.StartShrinkingDeadzone(ShrinkType.MOTION_BASED, deadzoneShrinkRate);
            }
            else
            {
                positioningModule.Stabiliser.StopShrinkingDeadzone();

                float deadzoneSizeIncrease = deadzoneMaxSizeIncrease * _df;

                float deadzoneMinSize = positioningModule.Stabiliser.defaultDeadzoneRadius;
                float deadzoneMaxSize = deadzoneMinSize + deadzoneMaxSizeIncrease;

                float newDeadzoneSize = positioningModule.Stabiliser.GetCurrentDeadzoneRadius() + deadzoneSizeIncrease;
                newDeadzoneSize = Mathf.Clamp(newDeadzoneSize, deadzoneMinSize, deadzoneMaxSize);
                positioningModule.Stabiliser.SetCurrentDeadzoneRadius(newDeadzoneSize);
            }
        }

        private float GetAppliedForceChange(float _currentVelocity, float _dt, Vector2 _dPerp, float _distanceFromTouchPlane)
        {
            // currentVelocity = current z-component of velocity in m/s
            // dt = current change in time in seconds
            // dPerp = horizontal change in position
            // distanceFromTouchPlane = z-distance from a virtual plane where clicks are always triggered

            float forceChange = 0f;

            if (_dt < Mathf.Epsilon)
            {
                // Not long enough between the two frames
                // Also triggered if recognising a new hand (dt is negative)

                // No change in force
                forceChange = 0f;
            }
            else if (useTouchPlaneForce && _distanceFromTouchPlane < 0f)
            {
                // Use a fixed stiffness beyond the touch-plane
                float stiffness = 1.0f / distPastTouchPlane;

                // Do not reduce force on backwards motion
                float forwardVelocity = Mathf.Max(0f, _currentVelocity);
                forceChange = stiffness * forwardVelocity * _dt;

                // Do not decay force when beyond the touch plane.
                // This is to ensure the user cannot edge closer and closer to the screen.
            }
            else
            {
                float angleFromScreen = Mathf.Atan2(_dPerp.magnitude, _currentVelocity * _dt) * Mathf.Rad2Deg;

                if (angleFromScreen < thetaOne || angleFromScreen > thetaTwo)
                {
                    // Towards screen: Increase force
                    // Moving towards or away from screen.
                    // Adjust force based on spring stiffness

                    // Perform a calculation:
                    float vClamped = Mathf.Clamp(Mathf.Abs(_currentVelocity), speedMin, speedMax);

                    float ratio = (vClamped - speedMin) / (speedMax - speedMin);

                    float stiffnessMin = 1.0f / distAtSpeedMin;
                    float stiffnessMax = 1.0f / distAtSpeedMax;

                    float k = stiffnessMin + stiffnessCurve.Evaluate(ratio) * (stiffnessMax - stiffnessMin);

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
                        float vPerp = _dPerp.magnitude / _dt;

                        float stiffness = 1f / horizontalDecayDist;
                        forceChange = -1f * stiffness * vPerp * _dt;
                    }
                }

                // If the forceChange is increasing, do not apply the decay
                if (decayingForce)
                {
                    if (forceChange <= 0f)
                    {
                        forceChange -= (1f - decayThreshold) * (_dt / forceDecayTime);
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
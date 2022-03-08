﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

using Ultraleap.TouchFree.Library;
using Ultraleap.TouchFree.Library.Configuration;

namespace Ultraleap.TouchFree.Library.Interactions
{
    public class AirPushInteraction : InteractionModule
    {
        public override InteractionType InteractionType { get; } = InteractionType.PUSH;

        public double millisecondsCooldownOnEntry = 300.0;
        Stopwatch handAppearedCooldown = new Stopwatch();

        public float speedMin = 0.15f;
        public float speedMax = 0.5f;
        public float distAtSpeedMin = 0.03f;
        public float distAtSpeedMax = 0.005f;
        public float horizontalDecayDist = 0.05f;

        public float thetaOne = 15f;
        public float thetaTwo = 135f;
        // If a hand moves an angle less than thetaOne, this is "towards" the screen
        // If a hand moves an angle greater than thetaTwo, this is "backwards" from the screen
        // If a hand moves between the two angles, this is "horizontal" to the screen

        public float unclickThreshold = 0.85f;
        public float unclickThresholdDrag = 0.85f;
        public bool decayForceOnClick = true;
        public float forceDecayTime = 0.1f;
        bool decayingForce;

        public bool useTouchPlaneForce = true;
        public float distPastTouchPlane = 0.02f;

        public float dragStartDistanceThresholdM = 0.01f;
        public float dragDeadzoneShrinkRate = 0.5f;
        public float dragDeadzoneShrinkDistanceThresholdM = 0.01f;

        public float deadzoneMaxSizeIncrease = 0.02f;
        public float deadzoneShrinkRate = 0.8f;

        private Vector2 cursorPressPosition;

        private long previousTime = 0;
        private float previousScreenDistance = float.PositiveInfinity;
        private Vector2 previousScreenPos = Vector2.Zero;

        private float appliedForce = 0f;
        private bool pressing = false;

        private bool isDragging = false;

        public AirPushInteraction(
            HandManager _handManager,
            IVirtualScreen _virtualScreen,
            IConfigManager _configManager,
            IPositioningModule _positioningModule) : base(_handManager, _virtualScreen, _configManager, _positioningModule, TrackedPosition.INDEX_STABLE)
        {
        }

        protected override void UpdateData(Leap.Hand hand)
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
                    SendInputAction(InputType.CANCEL, positions, appliedForce);
                }

                return;
            }

            HandleInteractionsAirPush();
        }

        private void HandleInteractionsAirPush()
        {
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
                float dz = (-1f) * (positions.DistanceFromScreen - previousScreenDistance);   // Metres +ve = towards screen
                float currentVelocity = dz / dt;    // m/s

                Vector2 dPerpPx = positions.CursorPosition - previousScreenPos;
                Vector2 dPerp = virtualScreen.PixelsToMillimeters(dPerpPx);

                // Update AppliedForce, which is the crux of the AirPush algorithm
                float forceChange = GetAppliedForceChange(currentVelocity, dt, dPerp, positions.DistanceFromScreen);
                appliedForce += forceChange;
                appliedForce = Utilities.Clamp(appliedForce, 0f, 1f);

                // Update the deadzone size
                if (!pressing)
                {
                    AdjustDeadzoneSize(forceChange);
                }

                // Determine whether to send any other events
                if (pressing)
                {
                    if ((!isDragging && appliedForce < unclickThreshold) || (isDragging && appliedForce < unclickThresholdDrag) || ignoreDragging)
                    {
                        pressing = false;
                        isDragging = false;
                        cursorPressPosition = Vector2.Zero;
                        SendInputAction(InputType.UP, positions, appliedForce);
                    }
                    else
                    {
                        if (isDragging)
                        {
                            SendInputAction(InputType.MOVE, positions, appliedForce);
                            positioningModule.Stabiliser.ReduceDeadzoneOffset();
                        }
                        else if (CheckForStartDrag(cursorPressPosition, positions.CursorPosition))
                        {
                            isDragging = true;
                            SendInputAction(InputType.MOVE, positions, appliedForce);
                            positioningModule.Stabiliser.StartShrinkingDeadzone(dragDeadzoneShrinkRate);
                        }
                        else
                        {
                            // NONE causes the client to react to data without using Input.
                            SendInputAction(InputType.NONE, positions, appliedForce);
                        }
                    }
                }
                else if (!decayingForce && appliedForce >= 1f)
                {
                    // Need the !decayingForce check here to eliminate the risk of double-clicks
                    // when ignoring dragging and moving past the touch-plane.

                    pressing = true;
                    SendInputAction(InputType.DOWN, positions, appliedForce);
                    cursorPressPosition = positions.CursorPosition;

                    // If dragging is off, we want to decay the force after a click back to the unclick threshold
                    if (decayForceOnClick && ignoreDragging)
                    {
                        decayingForce = true;
                    }

                    positioningModule.Stabiliser.SetDeadzoneOffset();
                    positioningModule.Stabiliser.currentDeadzoneRadius = dragStartDistanceThresholdM;
                }
                else if (positions.CursorPosition != previousScreenPos || positions.DistanceFromScreen != previousScreenDistance)
                {
                    // Send the move event
                    SendInputAction(InputType.MOVE, positions, appliedForce);
                    positioningModule.Stabiliser.ReduceDeadzoneOffset();
                }

                if (decayingForce && (appliedForce <= unclickThreshold - 0.1f))
                {
                    decayingForce = false;
                }
            }
            else
            {
                // show them they have been seen but send no major events as we have only just discovered the hand
                SendInputAction(InputType.MOVE, positions, appliedForce);
            }

            // Update stored variables
            previousTime = currentTimestamp;
            previousScreenDistance = positions.DistanceFromScreen;
            previousScreenPos = positions.CursorPosition;
        }

        private bool CheckForStartDrag(Vector2 _startPos, Vector2 _currentPos)
        {
            float distFromStartPosPx = (_startPos - _currentPos).Length();

            return distFromStartPosPx > virtualScreen.MillimetersToPixels(dragStartDistanceThresholdM * 1000);
        }

        private void AdjustDeadzoneSize(float _df)
        {
            // _df is change in Force in the last frame
            if (_df < -1f * float.Epsilon)
            {
                // Start decreasing deadzone size
                positioningStabiliser.StartShrinkingDeadzone(deadzoneShrinkRate);
            }
            else
            {
                positioningStabiliser.StopShrinkingDeadzone();

                float deadzoneSizeIncrease = deadzoneMaxSizeIncrease * _df;

                float deadzoneMinSize = positioningStabiliser.defaultDeadzoneRadius;
                float deadzoneMaxSize = deadzoneMinSize + deadzoneMaxSizeIncrease;

                float newDeadzoneSize = positioningStabiliser.currentDeadzoneRadius + deadzoneSizeIncrease;
                newDeadzoneSize = Utilities.Clamp(newDeadzoneSize, deadzoneMinSize, deadzoneMaxSize);
                positioningStabiliser.currentDeadzoneRadius = newDeadzoneSize;
            }
        }

        private float GetAppliedForceChange(float _currentVelocity, float _dt, Vector2 _dPerp, float _distanceFromTouchPlane)
        {
            // currentVelocity = current z-component of velocity in m/s
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
                float stiffness = 1.0f / distPastTouchPlane;

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
                    float vClamped = Utilities.Clamp(Math.Abs(_currentVelocity), speedMin, speedMax);

                    float stiffnessRatio = (vClamped - speedMin) / (speedMax - speedMin);

                    float stiffnessMin = 1.0f / distAtSpeedMin;
                    float stiffnessMax = 1.0f / distAtSpeedMax;

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

                        float stiffness = 1f / horizontalDecayDist;
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
﻿using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;
using System.Collections.Generic;
using Ultraleap.TouchFree.ServiceShared;
using Leap;

namespace Ultraleap.TouchFree.Service
{
    public class AirPushInteraction : InteractionModule
    {
        public override InteractionType InteractionType { get; } = InteractionType.PUSH;

        [Header("Hand Entry")]
        public double millisecondsCooldownOnEntry;
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
        private Vector2 cursorPressPosition;

        [Header("AirPush Unclick")]

        [Range(0, 0.999f)]
        public float unclickThreshold = 0.999f;
        [Range(0, 0.999f)]
        public float unclickThresholdDrag = 0.999f;
        public bool decayForceOnClick;
        [Range(0, 0.999f)]
        public float forceDecayTime;
        bool decayingForce;

        [Header("TouchPlane Params")]
        public bool useTouchPlaneForce;
        public float touchPlaneDistance;

        private float previousScreenDistance = Mathf.Infinity;
        private Vector2 previousScreenPos = Vector2.zero;

        private float appliedForce = 0f;

        [Header("Dragging")]
        public float dragStartDistanceThresholdM = 0.01f;
        public float dragDeadzoneShrinkRate = 0.5f;

        [Header("Deadzone")]
        public float deadzoneMaxSizeIncrease;
        public float deadzoneShrinkRate;

        private bool isDragging = false;

        private void HandleInteractionsAirPush()
        {
            if (handAppearedCooldown.IsRunning && handAppearedCooldown.ElapsedMilliseconds >= millisecondsCooldownOnEntry)
            {
                handAppearedCooldown.Stop();
            }

            // If not ignoring clicks...
            if (!handAppearedCooldown.IsRunning)
            {
                // Update the deadzone size
                if (!isTouching)
                {
                    AdjustDeadzoneSize(forceChange);
                }

                // Determine whether to send any other events
                if (isTouching)
                {
                    if ((!isDragging && appliedForce < unclickThreshold) || (isDragging && appliedForce < unclickThresholdDrag) || ignoreDragging)
                    {
                        isTouching = false;
                        isDragging = false;
                        cursorPressPosition = Vector2.zero;
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
                    isTouching = true;
                    SendInputAction(InputType.DOWN, positions, appliedForce);
                    cursorPressPosition = positions.CursorPosition;
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
            previousScreenDistance = positions.DistanceFromScreen;
            previousScreenPos = positions.CursorPosition;
        }

        private bool CheckForStartDrag(Vector2 _startPos, Vector2 _currentPos)
        {
            if (_currentPos != _startPos)
            {
                return true;
            }

            return false;
        }

        private void AdjustDeadzoneSize(float _changeInForce)
        {
            // _changeInForce is change in Force in the last frame
            if (_changeInForce < -1f * Mathf.Epsilon)
            {
                // Start decreasing deadzone size
                positioningModule.Stabiliser.StartShrinkingDeadzone(deadzoneShrinkRate);
            }
            else
            {
                positioningModule.Stabiliser.StopShrinkingDeadzone();
                positioningModule.Stabiliser.ScaleDeadzoneByProgress(appliedForce, deadzoneMaxSizeIncrease);
            }
        }

        private float GetAppliedForceChange()
        {
            // Calculate important variables needed in determining the key events
            float dt = Time.deltaTime; // Seconds
            float perpendicularMovement = previousScreenDistance - positions.DistanceFromScreen; // Metres towards screen
            float currentVelocity = perpendicularMovement / dt; // m/s

            Vector2 paralleMovement = positions.CursorPosition - previousScreenPos;
            paralleMovement = ConfigManager.GlobalSettings.virtualScreen.PixelsToMeters(paralleMovement);

            float forceChange = 0f;

            if (dt < Mathf.Epsilon)
            {
                // Not long enough between the two frames
                // Also triggered if recognising a new hand (dt is negative)

                // No change in force
                forceChange = 0f;
            }
            else if (useTouchPlaneForce && positions.DistanceFromScreen - touchPlaneDistance < 0f)
            {
                // Use a fixed stiffness beyond the touch-plane
                float stiffness = 1.0f / distAtSpeedMax;

                // Do not reduce force on backwards motion
                float forwardVelocity = Mathf.Max(0f, currentVelocity);
                forceChange = stiffness * forwardVelocity * dt;

                // Do not decay force when beyond the touch plane.
                // This is to ensure the user cannot edge closer and closer to the screen.
            }
            else
            {
                float angleFromScreen = Mathf.Atan2(paralleMovement.magnitude, currentVelocity * dt) * Mathf.Rad2Deg;

                if (angleFromScreen < thetaOne || angleFromScreen > thetaTwo)
                {
                    // Towards screen: Increase force
                    // Moving towards or away from screen.
                    // Adjust force based on spring stiffness

                    // Perform a calculation:
                    float vClamped = Mathf.Clamp(Mathf.Abs(currentVelocity), speedMin, speedMax);

                    float ratio = (vClamped - speedMin) / (speedMax - speedMin);

                    float stiffnessMin = 1.0f / distAtSpeedMin;
                    float stiffnessMax = 1.0f / distAtSpeedMax;

                    float k = stiffnessMin + stiffnessCurve.Evaluate(ratio) * (stiffnessMax - stiffnessMin);

                    forceChange = k * currentVelocity * dt;
                }
                else
                {
                    // Approximately horizontal movement.
                    if (isTouching)
                    {
                        forceChange = 0f;
                    }
                    else
                    {
                        // Change force based on horizontal velocity and a horizontal decay distance
                        float stiffness = 1f / horizontalDecayDist;
                        forceChange = -1f * stiffness * paralleMovement.magnitude;
                    }
                }

                // If the forceChange is increasing, do not apply the decay
                if (decayingForce)
                {
                    if (forceChange <= 0f)
                    {
                        forceChange -= (1f - (unclickThreshold - 0.1f)) * (dt / forceDecayTime);
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

        public override float CalculateProgress(Hand _hand)
        {
            if (_hand == null)
            {
                appliedForce = 0f;
                isTouching = false;
                isDragging = false;
                // Restarts the hand timer every frame that we have no active hand
                handAppearedCooldown.Restart();
                return 0;
            }

            positions = positioningModule.CalculatePositions(_hand);

            // Update AppliedForce, which is the crux of the AirPush algorithm
            forceChange = GetAppliedForceChange();
            appliedForce += forceChange;
            appliedForce = Mathf.Clamp01(appliedForce);

            return appliedForce;
        }

        float forceChange;

        public override void RunInteraction(Hand _hand, float _progress)
        {
            if (_hand == null)
            {
                appliedForce = 0f;
                isTouching = false;
                isDragging = false;
                // Restarts the hand timer every frame that we have no active hand
                handAppearedCooldown.Restart();

                if (InteractionManager.Instance.hadHandLastFrame)
                {
                    // We lost the hand so cancel anything we may have been doing
                    SendInputAction(InputType.CANCEL, positions, appliedForce);
                }

                return;
            }

            HandleInteractionsAirPush();
        }

        public override void RunPostProgressNonInteraction() 
        {
            previousScreenDistance = positions.DistanceFromScreen;
            previousScreenPos = positions.CursorPosition;

            if (decayingForce && (appliedForce <= unclickThreshold - 0.1f))
            {
                decayingForce = false;
            }
        }
    }
}
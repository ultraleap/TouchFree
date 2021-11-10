using UnityEngine;
using System;
using Ultraleap.TouchFree.ServiceShared;

namespace Ultraleap.TouchFree.Service
{
    public class GrabInteraction : InteractionModule
    {
        public override InteractionType InteractionType { get; } = InteractionType.GRAB;

        [Header("positioningModule.Stabiliser Params")]
        public float deadzoneEnlargementDistance;

        public float deadzoneShrinkSpeed;

        [Header("Other params")]
        [Tooltip("If hand is moving faster than this speed (in m/s), grabs will not be recognised")]
        public float maxHandVelocity = 0.15f;

        public GeneralisedGrabDetector grabDetector;

        [Header("Drag Params")]
        public float dragStartDistanceThresholdM;

        private bool pressing = false;

        private bool requireHold = false;
        private int REQUIRED_HOLD_FRAMES = 1;
        private int heldFrames = 0;
        private bool requireClick = false;

        // Dragging
        private Vector2 cursorDownPos;
        private bool isDragging;

        Tuple<long, Positions> previousPosition = new Tuple<long, Positions>(0, new Positions());

        protected override void UpdateData(Leap.Hand hand)
        {
            if (hand == null)
            {
                if (hadHandLastFrame)
                {
                    // We lost the hand so cancel anything we may have been doing
                    SendInputAction(InputType.CANCEL, positions, 0);
                }

                return;
            }

            positions = positioningModule.CalculatePositions(hand);

            float velocity = hand.PalmVelocity.Magnitude;
            if (previousPosition.Item1 != 0)
            {
                // Use the cursor velocity for x-y velocity
                //
                // I find that this velocity is quite similar to hand.PalmVelocity.Magnitude, but (as expected)
                // this velocity calculation gets much closer to 0 when the hand is more still.
                Vector3 previousWorldPos = ConfigManager.GlobalSettings.virtualScreen.VirtualScreenPositionToWorld(previousPosition.Item2.CursorPosition, previousPosition.Item2.DistanceFromScreen);
                Vector3 currentWorldPos = ConfigManager.GlobalSettings.virtualScreen.VirtualScreenPositionToWorld(positions.CursorPosition, positions.DistanceFromScreen);
                float changeInPos = (currentWorldPos - previousWorldPos).magnitude;
                float changeInTime = (latestTimestamp - previousPosition.Item1) / (1000f * 1000f);
                velocity = changeInPos / changeInTime;
            }
            HandleInteractions(hand, velocity);
            previousPosition = new Tuple<long, Positions>(latestTimestamp, positions);
        }

        private void HandleInteractions(Leap.Hand hand, float _velocity)
        {
            // If already pressing, continue regardless of velocity
            if (grabDetector.IsGrabbing(hand) && (pressing || _velocity < maxHandVelocity))
            {
                HandleInvoke();
            }
            else if (pressing)
            {
                HandleUnclick();
            }
            else
            {
                SendInputAction(InputType.MOVE, positions, grabDetector.GeneralisedGrabStrength);
            }
        }

        private void HandleInvoke()
        {
            // we are touching the screen
            if (!pressing)
            {
                HandlePress();
            }
            else
            {
                HandlePressHold();
            }
        }

        private void HandlePress()
        {
            SendInputAction(InputType.DOWN, positions, grabDetector.GeneralisedGrabStrength);
            pressing = true;
            if (ignoreDragging)
            {
                requireHold = true;
                heldFrames = 0;
                requireClick = false;
            }

            // Adjust deadzone
            positioningModule.Stabiliser.StopShrinkingDeadzone();
            float newDeadzoneRadius = deadzoneEnlargementDistance + positioningModule.Stabiliser.defaultDeadzoneRadius;
            positioningModule.Stabiliser.currentDeadzoneRadius = newDeadzoneRadius;
            cursorDownPos = positions.CursorPosition;
        }

        private void HandlePressHold()
        {
            if (isDragging)
            {
                SendInputAction(InputType.MOVE, positions, grabDetector.GeneralisedGrabStrength);
            }
            else
            {
                Positions downPositions = new Positions(cursorDownPos, positions.DistanceFromScreen);
                if (ignoreDragging)
                {
                    if (requireHold)
                    {
                        if (heldFrames >= REQUIRED_HOLD_FRAMES)
                        {
                            requireHold = false;
                            requireClick = true;
                            heldFrames = 0;
                        }
                        else
                        {
                            heldFrames += 1;
                        }
                        SendInputAction(InputType.MOVE, downPositions, grabDetector.GeneralisedGrabStrength);
                    }
                    else if (requireClick)
                    {
                        SendInputAction(InputType.UP, downPositions, grabDetector.GeneralisedGrabStrength);
                        positioningModule.Stabiliser.StartShrinkingDeadzone(deadzoneShrinkSpeed);
                        requireClick = false;
                    }
                    else
                    {
                        SendInputAction(InputType.MOVE, positions, grabDetector.GeneralisedGrabStrength);
                    }
                }
                else
                {
                    // Lock in to the touch down position until dragging occurs.
                    if (CheckForStartDrag(cursorDownPos, positions.CursorPosition) && !ignoreDragging)
                    {
                        isDragging = true;
                        positioningModule.Stabiliser.StartShrinkingDeadzone(deadzoneShrinkSpeed);
                    }

                    SendInputAction(InputType.MOVE, downPositions, grabDetector.GeneralisedGrabStrength);
                }
            }
        }

        private void HandleUnclick()
        {
            // Check if an unclick is needed, and perform if so
            if (!ignoreDragging)
            {
                if (!requireHold && !requireClick)
                {
                    SendInputAction(InputType.UP, positions, grabDetector.GeneralisedGrabStrength);
                }

                positioningModule.Stabiliser.StartShrinkingDeadzone(deadzoneShrinkSpeed);
            }

            pressing = false;
            isDragging = false;
        }

        bool CheckForStartDrag(Vector2 _startPos, Vector2 _currentPos)
        {
            var a = ConfigManager.GlobalSettings.virtualScreen.VirtualScreenPositionToWorld(_startPos, 0f);
            var b = ConfigManager.GlobalSettings.virtualScreen.VirtualScreenPositionToWorld(_currentPos, 0f);
            var distFromStartPos = (a - b).magnitude;

            if (distFromStartPos > dragStartDistanceThresholdM)
            {
                return true;
            }

            return false;
        }
    }
}
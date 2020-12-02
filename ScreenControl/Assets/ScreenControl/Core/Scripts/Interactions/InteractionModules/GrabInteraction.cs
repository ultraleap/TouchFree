using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;
using System;
using Ultraleap.ScreenControl.Core.ScreenControlTypes;

namespace Ultraleap.ScreenControl.Core
{
    public class GrabInteraction : InteractionModule
    {
        public InteractionType InteractionType { get; } = InteractionType.GRAB;

        [Header("positioningModule.Stabiliser Params")]
        public float deadzoneEnlargementDistance;
        public float deadzoneShrinkSpeed;

        [Header("Other params")]
        [Tooltip("If hand is moving faster than this speed (in m/s), grabs will not be recognised")]
        public float maxHandVelocity = 0.15f;
        public float verticalCursorOffset = 0.1f;

        public bool InteractionEnabled { get; set; } = true;

        public bool alwaysHover = false;

        public float inputPositionLerpSpeed;

        public bool debugDist;

        public GeneralisedGrabDetector grabDetector;

        [Header("Drag Params")]
        public float dragStartDistanceThresholdM;
        public float dragStartTimeDelaySecs;
        public float dragLerpSpeed;

        private bool pressing = false;

        public bool instantUnclick = false;
        private bool requireHold = false;
        private int REQUIRED_HOLD_FRAMES = 1;
        private int heldFrames = 0;
        private bool requireClick = false;

        // Dragging
        private Vector2 posLastFrame;
        private Vector2 cursorDownPos;
        private bool isDragging;
        private Stopwatch dragStartTimer = new Stopwatch();

        Tuple<long, Positions> previousPosition = new Tuple<long, Positions>(0, new Positions());

        protected override void UpdateData(Leap.Hand hand)
        {
            if (hand == null)
            {
                return;
            }

            if (!InteractionEnabled)
            {
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
                Vector3 previousWorldPos = GlobalSettings.virtualScreen.VirtualScreenPositionToWorld(previousPosition.Item2.CursorPosition, previousPosition.Item2.DistanceFromScreen);
                Vector3 currentWorldPos = GlobalSettings.virtualScreen.VirtualScreenPositionToWorld(positions.CursorPosition, positions.DistanceFromScreen);
                float changeInPos = (currentWorldPos - previousWorldPos).magnitude;
                float changeInTime = (latestTimestamp - previousPosition.Item1) / (1000f * 1000f);
                velocity = changeInPos / changeInTime;
            }
            HandleInteractions(hand, velocity);
            previousPosition = new Tuple<long, Positions>(latestTimestamp, positions);
        }

        private void HandleInteractions(Leap.Hand hand, float _velocity)
        {
            SendInputAction(InputType.MOVE, positions, positions.DistanceFromScreen, grabDetector.GeneralisedGrabStrength);
            // If already pressing, continue regardless of velocity
            if (grabDetector.IsGrabbing(latestTimestamp, hand, _velocity) && (pressing || _velocity < maxHandVelocity))
            {
                HandleInvoke();
            }
            else
            {
                HandlePotentialUnclick();
                isDragging = false;
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
            SendInputAction(InputType.DOWN, positions, positions.DistanceFromScreen, grabDetector.GeneralisedGrabStrength);
            dragStartTimer.Restart();
            pressing = true;
            if (instantUnclick && ignoreDragging)
            {
                requireHold = true;
                heldFrames = 0;
                requireClick = false;
            }

            // Adjust deadzone
            positioningModule.Stabiliser.StopShrinkingDeadzone();
            float newDeadzoneRadius = deadzoneEnlargementDistance + positioningModule.Stabiliser.defaultDeadzoneRadius;
            positioningModule.Stabiliser.SetCurrentDeadzoneRadius(newDeadzoneRadius);
        }

        private void HandlePressHold()
        {
            if (isDragging)
            {
            }
            else
            {
                Positions downPositions = new Positions(cursorDownPos, positions.DistanceFromScreen);
                if (instantUnclick && ignoreDragging)
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
                    }
                    else if (requireClick)
                    {
                        SendInputAction(InputType.UP, downPositions, positions.DistanceFromScreen, grabDetector.GeneralisedGrabStrength);
                        positioningModule.Stabiliser.StartShrinkingDeadzone(ShrinkType.MOTION_BASED, deadzoneShrinkSpeed);
                        requireClick = false;
                    }
                }
                else
                {
                    // Lock in to the touch down position until dragging occurs.
                    if (CheckForStartDrag(cursorDownPos, positions.CursorPosition) && !ignoreDragging)
                    {
                        isDragging = true;
                        positioningModule.Stabiliser.StartShrinkingDeadzone(ShrinkType.MOTION_BASED, deadzoneShrinkSpeed);
                    }
                }
            }
        }

        private void HandlePotentialUnclick()
        {
            // Check if an unclick is needed, and perform if so
            if (pressing)
            {
                if (!(ignoreDragging && instantUnclick))
                {
                    if (!requireHold && !requireClick)
                    {
                        SendInputAction(InputType.UP, positions, positions.DistanceFromScreen, grabDetector.GeneralisedGrabStrength);
                    }
                    positioningModule.Stabiliser.StartShrinkingDeadzone(ShrinkType.MOTION_BASED, deadzoneShrinkSpeed);
                }

                pressing = false;
            }
        }

        bool CheckForStartDrag(Vector2 _startPos, Vector2 _currentPos)
        {
            var a = GlobalSettings.virtualScreen.VirtualScreenPositionToWorld(_startPos, 0f);
            var b = GlobalSettings.virtualScreen.VirtualScreenPositionToWorld(_currentPos, 0f);
            var distFromStartPos = (a - b).magnitude;
            if (distFromStartPos > dragStartDistanceThresholdM)
            {
                return true;
            }

            if (dragStartTimer.ElapsedMilliseconds >= dragStartTimeDelaySecs * 1000f)
            {
                dragStartTimer.Stop();
                return true;
            }

            return false;
        }

        Vector3 worldPos_debug;
        Vector3 planeHit_debug;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (Application.isPlaying)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(-GlobalSettings.virtualScreen.PhysicalScreenPlane.normal * GlobalSettings.virtualScreen.PhysicalScreenPlane.distance, 0.01f);
                Gizmos.color = Color.green;
                //Gizmos.DrawWireSphere(planeHit_debug + (-GlobalSettings.virtualScreen.VirtualScreenPlane.normal * GlobalSettings.virtualScreen.VirtualScreenPlane.distance), 0.01f);
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(worldPos_debug, 0.01f);
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(planeHit_debug, 0.005f);
            }
        }
#endif
    }
}
using System;
using System.Numerics;

using Ultraleap.TouchFree.Library;
using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Library.Connections;
using Ultraleap.TouchFree.Library.Interactions;
using Ultraleap.TouchFree.Library.Interactions.InteractionModules;

namespace Ultraleap.TouchFree.Service
{
    public class GrabInteraction : InteractionModule
    {
        public override InteractionType InteractionType { get; } = InteractionType.GRAB;

        public float deadzoneEnlargementDistance = 20f;
        public float deadzoneShrinkSpeed = 0.3f;

        public float maxHandVelocity = 150f;

        public GeneralisedGrabDetector grabDetector;
        public float dragStartDistanceThresholdMm = 10f;

        private bool pressing = false;
        private bool requireHold = false;

        private int REQUIRED_HOLD_FRAMES = 1;
        private int heldFrames = 0;

        private bool requireClick = false;

        // Dragging
        private Vector2 cursorDownPos;
        private bool isDragging;

        Tuple<long, Positions> previousPosition = new Tuple<long, Positions>(0, new Positions());

        public GrabInteraction(
            IHandManager _handManager,
            IVirtualScreen _virtualScreen,
            IConfigManager _configManager,
            IClientConnectionManager _connectionManager,
            IPositioningModule _positioningModule,
            IPositionStabiliser _positionStabiliser) : base(_handManager, _virtualScreen, _configManager, _connectionManager, _positioningModule, _positionStabiliser)
        {
            grabDetector = new GeneralisedGrabDetector();
            positionConfiguration = new[]
            {
                new PositionTrackerConfiguration(TrackedPosition.INDEX_STABLE, 1)
            };
        }

        protected override InputActionResult UpdateData(Leap.Hand hand, float confidence)
        {
            if (hand == null)
            {
                if (hadHandLastFrame)
                {
                    // We lost the hand so cancel anything we may have been doing
                    return CreateInputActionResult(InputType.CANCEL, positions, 0);
                }

                return new InputActionResult();
            }

            float velocity = hand.PalmVelocity.Magnitude;
            if (previousPosition.Item1 != 0)
            {
                // Use the cursor velocity for x-y velocity
                //
                // I find that this velocity is quite similar to hand.PalmVelocity.Magnitude, but (as expected)
                // this velocity calculation gets much closer to 0 when the hand is more still.
                Vector3 previousWorldPosMm = virtualScreen.VirtualScreenPositionToWorld(previousPosition.Item2.CursorPosition, previousPosition.Item2.DistanceFromScreen * 1000);
                Vector3 currentWorldPosMm = virtualScreen.VirtualScreenPositionToWorld(positions.CursorPosition, positions.DistanceFromScreen * 1000);
                float changeInPos = (currentWorldPosMm - previousWorldPosMm).Length();
                float changeInTime = (latestTimestamp - previousPosition.Item1) / (1000f * 1000f);
                velocity = changeInPos / changeInTime;
            }
            var inputActionResult = HandleInteractions(hand, velocity);
            previousPosition = new Tuple<long, Positions>(latestTimestamp, positions);

            return inputActionResult;
        }

        private InputActionResult HandleInteractions(Leap.Hand hand, float _velocity)
        {
            // If already pressing, continue regardless of velocity
            if (grabDetector.IsGrabbing(hand) && (pressing || _velocity < (maxHandVelocity * 1000f)))
            {
                return HandleInvoke();
            }
            else if (pressing)
            {
                return HandleUnclick();
            }
            else
            {
                return CreateInputActionResult(InputType.MOVE, positions, grabDetector.GeneralisedGrabStrength);
            }
        }

        private InputActionResult HandleInvoke()
        {
            // we are touching the screen
            if (!pressing)
            {
                return HandlePress();
            }
            else
            {
                return HandlePressHold();
            }
        }

        private InputActionResult HandlePress()
        {
            var inputActionResult = CreateInputActionResult(InputType.DOWN, positions, grabDetector.GeneralisedGrabStrength);
            pressing = true;
            if (ignoreDragging)
            {
                requireHold = true;
                heldFrames = 0;
                requireClick = false;
            }

            // Adjust deadzone
            positionStabiliser.StopShrinkingDeadzone();
            float newDeadzoneRadius = deadzoneEnlargementDistance + positionStabiliser.defaultDeadzoneRadius;
            positionStabiliser.currentDeadzoneRadius = newDeadzoneRadius;
            cursorDownPos = positions.CursorPosition;

            return inputActionResult;
        }

        private InputActionResult HandlePressHold()
        {
            if (isDragging)
            {
                return CreateInputActionResult(InputType.MOVE, positions, grabDetector.GeneralisedGrabStrength);
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
                        return CreateInputActionResult(InputType.MOVE, downPositions, grabDetector.GeneralisedGrabStrength);
                    }
                    else if (requireClick)
                    {
                        positionStabiliser.StartShrinkingDeadzone(deadzoneShrinkSpeed);
                        requireClick = false;
                        return CreateInputActionResult(InputType.UP, downPositions, grabDetector.GeneralisedGrabStrength);
                    }
                    else
                    {
                        return CreateInputActionResult(InputType.MOVE, positions, grabDetector.GeneralisedGrabStrength);
                    }
                }
                else
                {
                    // Lock in to the touch down position until dragging occurs.
                    if (CheckForStartDrag(cursorDownPos, positions.CursorPosition) && !ignoreDragging)
                    {
                        isDragging = true;
                        positionStabiliser.StartShrinkingDeadzone(deadzoneShrinkSpeed);
                    }

                    return CreateInputActionResult(InputType.MOVE, downPositions, grabDetector.GeneralisedGrabStrength);
                }
            }
        }

        private InputActionResult HandleUnclick()
        {
            var inputActionResult = new InputActionResult();
            // Check if an unclick is needed, and perform if so
            if (!ignoreDragging)
            {
                if (!requireHold && !requireClick)
                {
                    inputActionResult = CreateInputActionResult(InputType.UP, positions, grabDetector.GeneralisedGrabStrength);
                }

                positionStabiliser.StartShrinkingDeadzone(deadzoneShrinkSpeed);
            }

            pressing = false;
            isDragging = false;

            return inputActionResult;
        }

        bool CheckForStartDrag(Vector2 _startPos, Vector2 _currentPos)
        {
            var a = virtualScreen.PixelsToMillimeters(_startPos);
            var b = virtualScreen.PixelsToMillimeters(_currentPos);
            var distFromStartPos = (a - b).Length();

            if (distFromStartPos > dragStartDistanceThresholdMm)
            {
                return true;
            }

            return false;
        }
    }
}
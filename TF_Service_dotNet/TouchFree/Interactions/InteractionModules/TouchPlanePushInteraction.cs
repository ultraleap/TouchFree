using System;
using System.Numerics;

using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Library.Interactions.InteractionModules;

namespace Ultraleap.TouchFree.Library.Interactions
{
    public class TouchPlanePushInteraction : InteractionModule
    {
        public override InteractionType InteractionType { get; } = InteractionType.TOUCHPLANE;

        // The distance from the touchPlane (in mm) at which the progressToClick is 0
        private const float touchPlaneZeroProgressMm = 100f;

        // The distance from screen (in mm) at which the progressToClick is 1
        private float touchPlaneDistanceMm = 50f;

        private bool pressing = false;
        bool pressComplete = false;

        private Vector2 downPos;

        // Used to ignore hands that initialise while past the touchPlane.
        // Particularly for those that are cancelled by InteractionZones
        bool handReady = false;

        public float dragStartDistanceThresholdMm = 10f;
        bool isDragging = false;

        public TouchPlanePushInteraction(
            HandManager _handManager,
            IVirtualScreen _virtualScreen,
            IConfigManager _configManager,
            IPositioningModule _positioningModule,
            IPositionStabiliser _positionStabiliser) : base(_handManager, _virtualScreen, _configManager, _positioningModule, _positionStabiliser)
        {
            positionConfiguration = new[]
            {
                new PositionTrackerConfiguration(TrackedPosition.NEAREST, 1)
            };
        }

        protected override InputActionResult UpdateData(Leap.Hand hand)
        {
            if (hand == null)
            {
                pressComplete = false;
                isDragging = false;
                pressing = false;
                handReady = false;

                if (hadHandLastFrame)
                {
                    // We lost the hand so cancel anything we may have been doing
                    return CreateInputActionResult(InputType.CANCEL, positions, 0);
                }
                return new InputActionResult();
            }

            return HandleInteractions();
        }

        private InputActionResult HandleInteractions()
        {
            Vector2 currentCursorPosition = positions.CursorPosition;

            float progressToClick = Math.Clamp(1f - Utilities.InverseLerp(touchPlaneDistanceMm, touchPlaneDistanceMm + touchPlaneZeroProgressMm, distanceFromScreenMm), 0f, 1f);

            // determine if the fingertip is across one of the surface thresholds (hover/press) and send event
            if (distanceFromScreenMm < touchPlaneDistanceMm)
            {
                if (handReady)
                {
                    // we are touching the screen
                    if (!pressing)
                    {
                        downPos = currentCursorPosition;
                        pressing = true;

                        return CreateInputActionResult(InputType.DOWN, positions, progressToClick);
                    }
                    else if (!ignoreDragging)
                    {
                        if (!isDragging && CheckForStartDrag(downPos, positions.CursorPosition))
                        {
                            isDragging = true;
                        }

                        if (isDragging)
                        {
                            return CreateInputActionResult(InputType.MOVE, positions, progressToClick);
                        }
                        else
                        {
                            // NONE causes the client to react to data without using Input.
                            return CreateInputActionResult(InputType.NONE, positions, progressToClick);
                        }
                    }
                    else if (!pressComplete)
                    {
                        pressComplete = true;

                        Positions downPositions = new Positions(downPos, positions.DistanceFromScreen);
                        return CreateInputActionResult(InputType.UP, downPositions, progressToClick);
                    }
                }
            }
            else
            {
                if (pressing && !pressComplete)
                {
                    pressComplete = false;
                    pressing = false;
                    isDragging = false;
                    handReady = true;

                    Positions downPositions = new Positions(downPos, positions.DistanceFromScreen);
                    return CreateInputActionResult(InputType.UP, downPositions, progressToClick);
                }
                else
                {
                    return CreateInputActionResult(InputType.MOVE, positions, progressToClick);
                }
            }

            return new InputActionResult();
        }

        private bool CheckForStartDrag(Vector2 _startPos, Vector2 _currentPos)
        {
            Vector2 startPosMm = virtualScreen.PixelsToMillimeters(_startPos);
            Vector2 currentPosMm = virtualScreen.PixelsToMillimeters(_currentPos);
            float distFromStartPos = (startPosMm - currentPosMm).Length();

            if (distFromStartPos > dragStartDistanceThresholdMm)
            {
                return true;
            }

            return false;
        }

        protected override void OnInteractionSettingsUpdated(InteractionConfigInternal _config)
        {
            base.OnInteractionSettingsUpdated(_config);

            touchPlaneDistanceMm = _config.TouchPlane.TouchPlaneActivationDistanceMm;
            positionConfiguration = new[]
            {
                new PositionTrackerConfiguration(_config.TouchPlane.TouchPlaneTrackedPosition, 1)
            };
        }
    }
}
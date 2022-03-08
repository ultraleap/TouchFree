﻿using System;

using Ultraleap.TouchFree.Library.Configuration;

namespace Ultraleap.TouchFree.Library.Interactions
{
    public class TouchPlanePushInteraction : InteractionModule
    {
        public override InteractionType InteractionType { get; } = InteractionType.TOUCHPLANE;

        // The distance from the touchPlane (in m) at which the progressToClick is 0
        private const float touchPlaneZeroProgress = 0.1f;

        // The distance from screen (in m) at which the progressToClick is 1
        private float touchPlaneDistance = 0.05f;

        private bool pressing = false;
        bool pressComplete = false;

        private Vector2 downPos;

        // Used to ignore hands that initialise while past the touchPlane.
        // Particularly for those that are cancelled by InteractionZones
        bool handReady = false;

        public float dragStartDistanceThresholdM = 0.01f;
        bool isDragging = false;

        public TouchPlanePushInteraction(
            HandManager _handManager,
            IVirtualScreen _virtualScreen,
            IConfigManager _configManager,
            IPositioningModule _positioningModule) : base(_handManager, _virtualScreen, _configManager, _positioningModule, TrackedPosition.NEAREST)
        {
        }

        protected override void UpdateData(Leap.Hand hand)
        {
            if (hand == null)
            {
                if (hadHandLastFrame)
                {
                    // We lost the hand so cancel anything we may have been doing
                    SendInputAction(InputType.CANCEL, positions, 0);
                }

                pressComplete = false;
                isDragging = false;
                pressing = false;
                handReady = false;
                return;
            }

            HandleInteractions();
        }

        private void HandleInteractions()
        {
            Vector2 currentCursorPosition = positions.CursorPosition;
            float distanceFromScreen = positions.DistanceFromScreen;
            float touchPlaneDistanceMm = touchPlaneDistance * 1000f;
            float touchPlaneZeroProgressMm = touchPlaneZeroProgress * 1000f;

            float progressToClick = Utilities.Clamp(1f - Utilities.InverseLerp(touchPlaneDistanceMm, touchPlaneDistanceMm + touchPlaneZeroProgressMm, distanceFromScreen), 0f, 1f);

            // determine if the fingertip is across one of the surface thresholds (hover/press) and send event
            if (distanceFromScreen < touchPlaneDistanceMm)
            {
                if (handReady)
                {
                    // we are touching the screen
                    if (!pressing)
                    {
                        SendInputAction(InputType.DOWN, positions, progressToClick);
                        downPos = currentCursorPosition;
                        pressing = true;
                    }
                    else if (!ignoreDragging)
                    {
                        if (!isDragging && CheckForStartDrag(downPos, positions.CursorPosition))
                        {
                            isDragging = true;
                        }

                        if (isDragging)
                        {
                            SendInputAction(InputType.MOVE, positions, progressToClick);
                        }
                        else
                        {
                            // NONE causes the client to react to data without using Input.
                            SendInputAction(InputType.NONE, positions, progressToClick);
                        }
                    }
                    else if (!pressComplete)
                    {
                        Positions downPositions = new Positions(downPos, distanceFromScreen);
                        SendInputAction(InputType.UP, downPositions, progressToClick);

                        pressComplete = true;
                    }
                }
            }
            else
            {
                if (pressing && !pressComplete)
                {
                    Positions downPositions = new Positions(downPos, distanceFromScreen);
                    SendInputAction(InputType.UP, downPositions, progressToClick);
                }
                else
                {
                    SendInputAction(InputType.MOVE, positions, progressToClick);
                }

                pressComplete = false;
                pressing = false;
                isDragging = false;
                handReady = true;
            }
        }

        private bool CheckForStartDrag(Vector2 _startPos, Vector2 _currentPos)
        {
            Vector2 startPosMm = virtualScreen.PixelsToMillimeters(_startPos);
            Vector2 currentPosMm = virtualScreen.PixelsToMillimeters(_currentPos);
            float distFromStartPos = (startPosMm - currentPosMm).Length();

            if (distFromStartPos > dragStartDistanceThresholdM)
            {
                return true;
            }

            return false;
        }

        protected override void OnInteractionSettingsUpdated(InteractionConfig _config)
        {
            base.OnInteractionSettingsUpdated(_config);

            // Convert from Mm to M
            touchPlaneDistance = _config.TouchPlane.TouchPlaneActivationDistanceMm / 1000;
            positioningModule.TrackedPosition = _config.TouchPlane.TouchPlaneTrackedPosition;
        }
    }
}
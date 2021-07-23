using UnityEngine;
using Ultraleap.TouchFree.ServiceShared;

namespace Ultraleap.TouchFree.Service
{
    public class TouchPlanePushInteraction : InteractionModule
    {
        public override InteractionType InteractionType { get; } = InteractionType.TOUCHPLANE;

        // The distance from the touchPlane at which the progressToClick is 0
        private const float touchPlaneZeroProgress = 0.1f;

        // The distance from screen at which the progressToClick is 1
        private float touchPlaneDistance = 0.05f;

        private bool pressing = false;
        bool pressComplete = false;

        private Vector2 downPos;

        [Header("Dragging")]
        public float dragStartDistanceThresholdM = 0.01f;
        bool isDragging = false;

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
                return;
            }

            HandleInteractions();
        }

        private void HandleInteractions()
        {
            Vector2 currentCursorPosition = positions.CursorPosition;
            float distanceFromScreen = positions.DistanceFromScreen;

            float progressToClick = Mathf.Clamp(1f - Mathf.InverseLerp(touchPlaneDistance, touchPlaneDistance + touchPlaneZeroProgress, distanceFromScreen), 0f, 1f);

            // determine if the fingertip is across one of the surface thresholds (hover/press) and send event
            if (distanceFromScreen < touchPlaneDistance)
            {
                // we are touching the screen
                if (!pressing)
                {
                    SendInputAction(InputType.DOWN, positions, progressToClick);
                    downPos = currentCursorPosition;
                    pressing = true;
                }
                else if(!ignoreDragging)
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
            }
        }

        private bool CheckForStartDrag(Vector2 _startPos, Vector2 _currentPos)
        {
            Vector2 startPosM = ConfigManager.GlobalSettings.virtualScreen.PixelsToMeters(_startPos);
            Vector2 currentPosM = ConfigManager.GlobalSettings.virtualScreen.PixelsToMeters(_currentPos);
            float distFromStartPos = (startPosM - currentPosM).magnitude;

            if (distFromStartPos > dragStartDistanceThresholdM)
            {
                return true;
            }

            return false;
        }

        protected override void OnSettingsUpdated()
        {
            base.OnSettingsUpdated();

            // Convert from CM to M
            touchPlaneDistance = ConfigManager.InteractionConfig.TouchPlane.TouchPlaneActivationDistanceCM / 100;
            positioningModule.trackedPosition = ConfigManager.InteractionConfig.TouchPlane.TouchPlaneTrackedPosition;
        }
    }
}
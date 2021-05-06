using UnityEngine;
using Ultraleap.ScreenControl.Core.ScreenControlTypes;

namespace Ultraleap.ScreenControl.Core
{
    public class TouchPlanePushInteraction : InteractionModule
    {
        public override InteractionType InteractionType { get; } = InteractionType.TOUCHPLANE;

        // The distance from the touchPlane at which the progressToClick is 0
        private float touchPlaneZeroProgress = 0.03f;

        // The distance from screen at which the progressToClick is 1
        private float touchPlaneDistance = 0.05f;

        private bool pressing = false;
        bool pressComplete = false;

        private Vector2 downPos;

        bool cancelled = false;

        [Header("Dragging")]
        public float dragStartDistanceThresholdM = 0.01f;
        public float dragDeadzoneShrinkRate = 0.5f;
        public float dragDeadzoneShrinkDistanceThresholdM = 0.01f;
        private bool dragDeadzoneShrinkTriggered = false;
        bool isDragging = false;

        protected override void UpdateData(Leap.Hand hand)
        {
            if (hand == null)
            {
                if (hadHandLastFrame)
                {
                    // We lost the hand so cancel anything we may have been doing
                    SendInputAction(InputType.CANCEL, positions, 0);
                    cancelled = true;
                }

                pressComplete = false;
                isDragging = false;
                pressing = false;
                return;
            }

            positions = positioningModule.CalculatePositions(hand);
            HandleInteractions();
        }

        private void HandleInteractions()
        {
            Vector2 currentCursorPosition = positions.CursorPosition;
            float distanceFromScreen = positions.DistanceFromScreen;

            float progressToClick = 1f - Mathf.InverseLerp(touchPlaneDistance, touchPlaneDistance + touchPlaneZeroProgress, distanceFromScreen);

            // determine if the fingertip is across one of the surface thresholds (hover/press) and send event
            if (distanceFromScreen < touchPlaneDistance)
            {
                cancelled = false;
                // we are touching the screen
                if (!pressing)
                {
                    SendInputAction(InputType.DOWN, positions, progressToClick);
                    downPos = currentCursorPosition;
                    pressing = true;
                }
                else if(!ignoreDragging)
                {
                    if (isDragging)
                    {
                        if (!dragDeadzoneShrinkTriggered && CheckForStartDragDeadzoneShrink(downPos, positions.CursorPosition))
                        {
                            positioningModule.Stabiliser.StartShrinkingDeadzone(dragDeadzoneShrinkRate);
                            dragDeadzoneShrinkTriggered = true;
                        }

                        SendInputAction(InputType.MOVE, positions, progressToClick);
                    }
                    else if (CheckForStartDrag(downPos, positions.CursorPosition))
                    {
                        isDragging = true;
                        dragDeadzoneShrinkTriggered = false;
                    }
                }
                else if (!pressComplete)
                {
                    Positions downPositions = new Positions(downPos, distanceFromScreen);
                    SendInputAction(InputType.UP, downPositions, progressToClick);

                    pressComplete = true;
                }
            }
            else if (distanceFromScreen < touchPlaneDistance + touchPlaneZeroProgress)
            {
                if (pressing && !pressComplete)
                {
                    Positions downPositions = new Positions(downPos, distanceFromScreen);
                    SendInputAction(InputType.UP, downPositions, progressToClick);
                }

                pressComplete = false;
                pressing = false;
                isDragging = false;

                SendInputAction(InputType.MOVE, positions, progressToClick);
                cancelled = false;
            }
            else if (!cancelled)
            {
                if (pressing && !pressComplete)
                {
                    Positions downPositions = new Positions(downPos, distanceFromScreen);
                    SendInputAction(InputType.UP, downPositions, progressToClick);
                }

                pressComplete = false;
                pressing = false;

                SendInputAction(InputType.CANCEL, positions, 0);
                cancelled = true;
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

        private bool CheckForStartDragDeadzoneShrink(Vector2 _startPos, Vector2 _currentPos)
        {
            Vector2 startPosM = ConfigManager.GlobalSettings.virtualScreen.PixelsToMeters(_startPos);
            Vector2 currentPosM = ConfigManager.GlobalSettings.virtualScreen.PixelsToMeters(_currentPos);
            float distFromStartPos = (startPosM - currentPosM).magnitude;
            return (distFromStartPos > dragDeadzoneShrinkDistanceThresholdM);
        }

        protected override void OnSettingsUpdated()
        {
            base.OnSettingsUpdated();

            // Convert from CM to M
            touchPlaneDistance = ConfigManager.InteractionConfig.TouchPlane.TouchPlaneDistanceCM / 100;
            touchPlaneZeroProgress = ConfigManager.InteractionConfig.TouchPlane.TouchPlaneStartDistanceCM / 100;
        }
    }
}
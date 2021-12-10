using UnityEngine;
using Ultraleap.TouchFree.ServiceShared;

using Leap.Unity;

namespace Ultraleap.TouchFree.Service
{
    public class AirTapInteraction : InteractionModule
    {
        public override InteractionType InteractionType { get; } = InteractionType.TOUCHPLANE;

        private bool pressing = false;
        bool pressComplete = false;

        private Vector2 downPos;

        [Header("Dragging")]
        public float dragStartDistanceThresholdM = 0.01f;
        bool isDragging = false;

        public AnimationCurve progressCurve;

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

            HandleInteractions(hand);
        }

        private void HandleInteractions(Leap.Hand hand)
        {
            Vector3 palmForward = (hand.PalmPosition - hand.WristPosition).ToVector3().normalized;
            Vector3 indexForward = (hand.GetIndex().bones[2].NextJoint - hand.GetIndex().bones[1].NextJoint).ToVector3().normalized;

            float dot = Vector3.Dot(palmForward, indexForward);
            float progress = progressCurve.Evaluate(dot);
            positioningModule.Stabiliser.ScaleDeadzoneByProgress(progress, 0.02f);

            if (progress >= 1)
            {
                // we are touching the screen
                if (!pressing)
                {
                    SendInputAction(InputType.DOWN, positions, progress);
                    downPos = positions.CursorPosition;
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
                        SendInputAction(InputType.MOVE, positions, progress);
                    }
                    else
                    {
                        // NONE causes the client to react to data without using Input.
                        SendInputAction(InputType.NONE, positions, progress);
                    }
                }
                else if (!pressComplete)
                {
                    Positions downPositions = new Positions(downPos, progress);
                    SendInputAction(InputType.UP, downPositions, progress);

                    pressComplete = true;
                }
            }
            else
            {
                if (pressing && !pressComplete)
                {
                    Positions downPositions = new Positions(downPos, progress);
                    SendInputAction(InputType.UP, downPositions, progress);
                }
                else
                {
                    SendInputAction(InputType.MOVE, positions, progress);
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
    }
}
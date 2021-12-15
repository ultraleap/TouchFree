using UnityEngine;
using Ultraleap.TouchFree.ServiceShared;

using Leap.Unity;
using Leap;

namespace Ultraleap.TouchFree.Service
{
    public class AirClickInteraction : InteractionModule
    {
        public override InteractionType InteractionType { get; } = InteractionType.TOUCHPLANE;

        bool touchComplete = false;

        private Vector2 downPos;

        [Header("Dragging")]
        public float dragStartDistanceThresholdM = 0.01f;
        bool isDragging = false;

        bool clickProgressing = false;
        float prevAngle;
        float startAngle;
        float endAngle;
        float maxAngleChange = 25;
        float minAngleChangePerSecond = 180;
        bool progressHit1 = false;

        public override float CalculateProgress(Hand _hand)
        {
            if(_hand == null)
            {
                touchComplete = false;
                isDragging = false;
                isTouching = false;
                return 0;
            }

            Vector3 palmForward = (_hand.GetMiddle().bones[0].NextJoint - _hand.PalmPosition).ToVector3().normalized;
            Vector3 indexForward = (_hand.GetIndex().Direction).ToVector3().normalized;

            float dot = Vector3.Dot(palmForward, indexForward);

            float angle = Mathf.Abs(dot - 1) * 90;
            float progress = 0;

            if (!isTouching)
            {
                if (angle - prevAngle > minAngleChangePerSecond * Time.deltaTime)
                {
                    // we are moving fast enough!
                    if (!clickProgressing)
                    {
                        clickProgressing = true;
                        startAngle = angle;
                    }

                    float angleChange = angle - startAngle;
                    progress = Mathf.Clamp01(ServiceUtility.MapRangeToRange(maxAngleChange - angleChange, maxAngleChange, 0, 0, 1));

                    if (progress == 1 && !progressHit1)
                    {
                        progressHit1 = true;
                        endAngle = angle;
                    }
                }
                else
                {
                    clickProgressing = false;
                }

                if (progressHit1)
                {
                    progress = 1;
                }
            }
            else
            {
                progressHit1 = false;
                clickProgressing = false;

                progress = Mathf.Clamp01(ServiceUtility.MapRangeToRange(angle, endAngle, startAngle, 1, 0));
            }

            prevAngle = angle;

            return progress;
        }

        public override void RunInteraction(Hand _hand, float _progress)
        {
            positions = positioningModule.CalculatePositions(_hand);

            if (_hand == null)
            {
                if (InteractionManager.Instance.hadHandLastFrame)
                {
                    // We lost the hand so cancel anything we may have been doing
                    SendInputAction(InputType.CANCEL, positions, 0);
                }

                touchComplete = false;
                isDragging = false;
                isTouching = false;
                return;
            }

            handChirality = _hand.IsLeft ? HandChirality.LEFT : HandChirality.RIGHT;

            if (_progress >= 1 || (_progress > 0.8f && isDragging))
            {
                // we are touching the screen
                if (!isTouching)
                {
                    SendInputAction(InputType.DOWN, positions, _progress);
                    positioningModule.Stabiliser.SetDeadzoneOffset();
                    positioningModule.Stabiliser.currentDeadzoneRadius = dragStartDistanceThresholdM;
                    downPos = positions.CursorPosition;
                    isTouching = true;
                }
                else if (!ignoreDragging)
                {
                    if (!isDragging && CheckForStartDrag(downPos, positions.CursorPosition))
                    {
                        isDragging = true;
                        positioningModule.Stabiliser.StartShrinkingDeadzone(0.9f);
                    }

                    if (isDragging)
                    {
                        SendInputAction(InputType.MOVE, positions, _progress);
                        positioningModule.Stabiliser.ReduceDeadzoneOffset();
                    }
                    else
                    {
                        // NONE causes the client to react to data without using Input.
                        SendInputAction(InputType.NONE, positions, _progress);
                    }
                }
                else if (!touchComplete)
                {
                    Positions downPositions = new Positions(downPos, _progress);
                    SendInputAction(InputType.UP, positions, _progress);

                    touchComplete = true;
                }
            }
            else
            {
                positioningModule.Stabiliser.ScaleDeadzoneByProgress(_progress, 0.02f);

                if (isTouching && !touchComplete)
                {
                    Positions downPositions = new Positions(downPos, _progress);
                    SendInputAction(InputType.UP, positions, _progress);
                }
                else
                {
                    SendInputAction(InputType.MOVE, positions, _progress);
                    positioningModule.Stabiliser.ReduceDeadzoneOffset();
                }

                touchComplete = false;
                isTouching = false;
                isDragging = false;
            }
        }

        private bool CheckForStartDrag(Vector2 _startPos, Vector2 _currentPos)
        {
            if (_currentPos != _startPos)
            {
                return true;
            }

            return false;
        }
    }
}
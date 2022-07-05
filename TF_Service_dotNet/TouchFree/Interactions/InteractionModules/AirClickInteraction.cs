using System;
using System.Linq;
using System.Numerics;
using Microsoft.Extensions.Options;
using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Library.Interactions.InteractionModules;

namespace Ultraleap.TouchFree.Library.Interactions
{
    public class AirClickInteraction : InteractionModule
    {
        public override InteractionType InteractionType { get; } = InteractionType.AIRCLICK;

        bool touchComplete = false;

        private Vector2 downPos;

        bool isDragging = false;
        float dragStartDistanceThresholdMm = 30f;

        bool clickProgressing = false;
        float prevAngle;
        float startAngle;
        float endAngle;
        public float maxAngleChange = 30;
        public float minAngleChangePerSecond = 180;
        bool progressHit1 = false;
        bool isTouching = false;
        long previousTimeStamp = 0;

        private readonly ExtrapolationPositionModifier extrapolation;
        private readonly PositionFilter filter;

        public AirClickInteraction(
            IHandManager _handManager,
            IVirtualScreen _virtualScreen,
            IConfigManager _configManager,
            IOptions<InteractionTuning> _interactionTuning,
            IPositioningModule _positioningModule,
            IPositionStabiliser _positionStabiliser) : base(_handManager, _virtualScreen, _configManager, _positioningModule, _positionStabiliser)
        {
            extrapolation = new ExtrapolationPositionModifier(_interactionTuning);
            filter = new PositionFilter(_interactionTuning);

            positionConfiguration = new[]
            {
                new PositionTrackerConfiguration(TrackedPosition.INDEX_STABLE, 1)
            };
        }

        protected override Positions ApplyAdditionalPositionModifiers(Positions positions)
        {
            var returnPositions = base.ApplyAdditionalPositionModifiers(positions);
            returnPositions.CursorPosition = extrapolation.ApplyModification(returnPositions.CursorPosition);
            returnPositions.CursorPosition = filter.ApplyModification(returnPositions.CursorPosition);
            return returnPositions;
        }

        public float CalculateProgress(Leap.Hand _hand, float confidence)
        {
            if (_hand == null)
            {
                touchComplete = false;
                isDragging = false;
                isTouching = false;
                return 0;
            }
            
            Vector3 palmForward = Utilities.LeapVectorToNumerics(_hand.Fingers.Single(x => x.Type == Leap.Finger.FingerType.TYPE_MIDDLE).bones[0].NextJoint - _hand.PalmPosition);
            palmForward = palmForward / palmForward.Length();
            Vector3 indexForward = Utilities.LeapVectorToNumerics(_hand.Fingers.Single(x => x.Type == Leap.Finger.FingerType.TYPE_INDEX).Direction);
            indexForward = indexForward / indexForward.Length();

            float dot = Vector3.Dot(palmForward, indexForward);

            float angle = (float)(Math.Acos(dot) * 180 / Math.PI);

            float simpleAngle = Math.Abs(dot - 1) * 90;
            float progress = 0;

            if (previousTimeStamp != 0)
            {
                long dtMicroseconds = (latestTimestamp - previousTimeStamp);
                float dt = dtMicroseconds / (1000f * 1000f);     // Seconds

                if (!isTouching)
                {
                    if ((angle - prevAngle) * confidence > minAngleChangePerSecond * dt)  // Multiply by confidence to make it harder to use when disused
                    {
                        // we are moving fast enough!
                        if (!clickProgressing)
                        {
                            clickProgressing = true;
                            startAngle = angle;
                        }

                        float angleChange = angle - startAngle;
                        progress = Math.Clamp(Utilities.MapRangeToRange(maxAngleChange - angleChange, maxAngleChange, 0, 0, 1), 0, 1);

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

                    progress = Math.Clamp(Utilities.MapRangeToRange(angle, endAngle, startAngle, 1, 0), 0, 1);
                }

                prevAngle = angle;
            }

            previousTimeStamp = latestTimestamp;

            return progress;
        }

        protected override InputActionResult UpdateData(Leap.Hand _hand, float confidence)
        {
            if (_hand == null)
            {

                touchComplete = false;
                isDragging = false;
                isTouching = false;

                if (hadHandLastFrame)
                {
                    // We lost the hand so cancel anything we may have been doing
                    return CreateInputActionResult(InputType.CANCEL, positions, 0);
                }
                return new InputActionResult();
            }

            var _progress = CalculateProgress(_hand, confidence);

            InputActionResult result = new InputActionResult();

            if (_progress >= 1 || (_progress > 0.8f && isDragging))
            {
                // we are touching the screen
                if (!isTouching)
                {
                    result = CreateInputActionResult(InputType.DOWN, positions, _progress);
                    positionStabiliser.SetDeadzoneOffset();
                    positionStabiliser.currentDeadzoneRadius = dragStartDistanceThresholdMm;
                    downPos = positions.CursorPosition;
                    isTouching = true;
                }
                else if (!ignoreDragging)
                {
                    if (!isDragging && CheckForStartDrag(downPos, positions.CursorPosition))
                    {
                        isDragging = true;
                        positionStabiliser.StartShrinkingDeadzone(0.9f);
                    }

                    if (isDragging)
                    {
                        result = CreateInputActionResult(InputType.MOVE, positions, _progress);
                        positionStabiliser.ReduceDeadzoneOffset();
                    }
                    else
                    {
                        // NONE causes the client to react to data without using Input.
                        result = CreateInputActionResult(InputType.NONE, positions, _progress);
                    }
                }
                else if (!touchComplete)
                {
                    result = CreateInputActionResult(InputType.UP, positions, _progress);

                    touchComplete = true;
                }
            }
            else
            {
                positionStabiliser.ScaleDeadzoneByProgress(_progress, 0.02f);

                if (isTouching && !touchComplete)
                {
                    result = CreateInputActionResult(InputType.UP, positions, _progress);
                }
                else
                {
                    result = CreateInputActionResult(InputType.MOVE, positions, _progress);
                    positionStabiliser.ReduceDeadzoneOffset();
                }

                touchComplete = false;
                isTouching = false;
                isDragging = false;
            }

            return result;
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
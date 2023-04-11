using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Numerics;
using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Library.Connections;

namespace Ultraleap.TouchFree.Library.Interactions.InteractionModules;

public class AirClickInteraction : InteractionModule
{
    public override InteractionType InteractionType => InteractionType.AIRCLICK;

    private bool _touchComplete = false;

    private Vector2 _downPos;

    private bool _isDragging = false;
    private readonly float _dragStartDistanceThresholdMm = 30f;

    private bool _clickProgressing = false;
    private float _prevAngle;
    private float _startAngle;
    private float _endAngle;
    private readonly float _maxAngleChange = 30;
    private readonly float _minAngleChangePerSecond = 180;
    private bool _progressHit1 = false;
    private bool _isTouching = false;
    private long _previousTimeStamp = 0;

    private readonly ExtrapolationPositionModifier _extrapolation;
    private readonly PositionFilter _filter;

    public AirClickInteraction(
        IHandManager handManager,
        IVirtualScreen virtualScreen,
        IConfigManager configManager,
        IClientConnectionManager connectionManager,
        IOptions<InteractionTuning> interactionTuning,
        IPositioningModule positioningModule,
        IPositionStabiliser positionStabiliser) : base(handManager, virtualScreen, configManager, connectionManager, positioningModule, positionStabiliser)
    {
        _extrapolation = new ExtrapolationPositionModifier(interactionTuning);
        _filter = new PositionFilter(interactionTuning);

        PositionConfiguration = new[]
        {
            new PositionTrackerConfiguration(TrackedPosition.INDEX_STABLE, 1)
        };
    }

    protected override Positions ApplyAdditionalPositionModifiers(Positions pos) =>
        base.ApplyAdditionalPositionModifiers(pos)
            .ApplyModifier(_extrapolation)
            .ApplyModifier(_filter);

    private float CalculateProgress(Leap.Hand hand, float confidence)
    {
        if (hand == null)
        {
            _touchComplete = false;
            _isDragging = false;
            _isTouching = false;
            return 0;
        }

        Vector3 palmForward = Utilities.LeapVectorToNumerics(hand.Fingers.Single(x => x.Type == Leap.Finger.FingerType.TYPE_MIDDLE).bones[0].NextJoint - hand.PalmPosition);
        palmForward /= palmForward.Length();
        Vector3 indexForward = Utilities.LeapVectorToNumerics(hand.Fingers.Single(x => x.Type == Leap.Finger.FingerType.TYPE_INDEX).Direction);
        indexForward /= indexForward.Length();

        float dot = Vector3.Dot(palmForward, indexForward);

        float angle = (float)(Math.Acos(dot) * 180 / Math.PI);

        float progress = 0;

        if (_previousTimeStamp != 0)
        {
            long dtMicroseconds = (LatestTimestamp - _previousTimeStamp);
            float dt = dtMicroseconds / (1000f * 1000f);     // Seconds

            if (!_isTouching)
            {
                if ((angle - _prevAngle) * confidence > _minAngleChangePerSecond * dt)  // Multiply by confidence to make it harder to use when disused
                {
                    // we are moving fast enough!
                    if (!_clickProgressing)
                    {
                        _clickProgressing = true;
                        _startAngle = angle;
                    }

                    float angleChange = angle - _startAngle;
                    progress = Math.Clamp(Utilities.MapRangeToRange(_maxAngleChange - angleChange, _maxAngleChange, 0, 0, 1), 0, 1);

                    if (progress == 1 && !_progressHit1)
                    {
                        _progressHit1 = true;
                        _endAngle = angle;
                    }
                }
                else
                {
                    _clickProgressing = false;
                }

                if (_progressHit1)
                {
                    progress = 1;
                }
            }
            else
            {
                _progressHit1 = false;
                _clickProgressing = false;

                progress = Math.Clamp(Utilities.MapRangeToRange(angle, _endAngle, _startAngle, 1, 0), 0, 1);
            }

            _prevAngle = angle;
        }

        _previousTimeStamp = LatestTimestamp;

        return progress;
    }

    protected override InputActionResult UpdateData(Leap.Hand hand, float confidence)
    {
        if (hand == null)
        {
            _touchComplete = false;
            _isDragging = false;
            _isTouching = false;

            if (HadHandLastFrame)
            {
                // We lost the hand so cancel anything we may have been doing
                return CreateInputActionResult(InputType.CANCEL, positions, 0);
            }
            return new InputActionResult();
        }

        var progress = CalculateProgress(hand, confidence);

        InputActionResult result = new InputActionResult();

        if (progress >= 1 || (progress > 0.8f && _isDragging))
        {
            // we are touching the screen
            if (!_isTouching)
            {
                result = CreateInputActionResult(InputType.DOWN, positions, progress);
                PositionStabiliser.SetDeadzoneOffset();
                PositionStabiliser.CurrentDeadzoneRadius = _dragStartDistanceThresholdMm;
                _downPos = positions.CursorPosition;
                _isTouching = true;
            }
            else if (!IgnoreDragging)
            {
                if (!_isDragging && CheckForStartDrag(_downPos, positions.CursorPosition))
                {
                    _isDragging = true;
                    PositionStabiliser.StartShrinkingDeadzone(0.9f);
                }

                if (_isDragging)
                {
                    result = CreateInputActionResult(InputType.MOVE, positions, progress);
                    PositionStabiliser.ReduceDeadzoneOffset();
                }
                else
                {
                    // NONE causes the client to react to data without using Input.
                    result = CreateInputActionResult(InputType.NONE, positions, progress);
                }
            }
            else if (!_touchComplete)
            {
                result = CreateInputActionResult(InputType.UP, positions, progress);

                _touchComplete = true;
            }
        }
        else
        {
            PositionStabiliser.ScaleDeadzoneByProgress(progress, 0.02f);

            if (_isTouching && !_touchComplete)
            {
                result = CreateInputActionResult(InputType.UP, positions, progress);
            }
            else
            {
                result = CreateInputActionResult(InputType.MOVE, positions, progress);
                PositionStabiliser.ReduceDeadzoneOffset();
            }

            _touchComplete = false;
            _isTouching = false;
            _isDragging = false;
        }

        return result;
    }
}
using System.Numerics;
using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Library.Connections;

namespace Ultraleap.TouchFree.Library.Interactions.InteractionModules;

public class HoverAndHoldInteraction : InteractionModule
{
    public override InteractionType InteractionType => InteractionType.HOVER;

    private readonly ProgressTimer _progressTimer = new(600f);

    private readonly float _hoverDeadzoneEnlargementDistance = 5f;
    private readonly float _timerDeadzoneEnlargementDistance = 5f;

    private readonly float _deadzoneShrinkSpeed = 0.3f;

    private float _hoverTriggerTime = 500f;
    private readonly float _clickHoldTime = 200f;

    private Vector2 _previousHoverPosDeadzone = Vector2.Zero;
    private Vector2 _previousScreenPos = Vector2.Zero;

    private bool _hoverTriggered = false;
    private float _hoverTriggeredDeadzoneRadius = 0f;
    private readonly TimestampStopwatch _hoverTriggerTimer = new();

    private bool _clickHeld = false;
    private bool _clickAlreadySent = false;
    private readonly TimestampStopwatch _clickingTimer = new();

    public HoverAndHoldInteraction(
        IHandManager handManager,
        IVirtualScreen virtualScreen,
        IConfigManager configManager,
        IClientConnectionManager connectionManager,
        IPositioningModule positioningModule,
        IPositionStabiliser positionStabiliser) : base(handManager, virtualScreen, configManager, connectionManager, positioningModule, positionStabiliser)
    {
        PositionConfiguration = new[]
        {
            new PositionTrackerConfiguration(TrackedPosition.INDEX_STABLE, 1)
        };
    }

    protected override InputActionResult UpdateData(Leap.Hand hand, float confidence)
    {
        if (hand == null)
        {
            if (HadHandLastFrame)
            {
                // We lost the hand so cancel anything we may have been doing
                return CreateInputActionResult(InputType.CANCEL, positions, 0);
            }

            return new InputActionResult();
        }

        Vector2 cursorPositionMm = VirtualScreen.PixelsToMillimeters(positions.CursorPosition);
        Vector2 hoverPosMm = ApplyHoverzone(cursorPositionMm);
        positions = new Positions(VirtualScreen.MillimetersToPixels(hoverPosMm), positions.DistanceFromScreen);

        return HandleInteractions();
    }

    private Vector2 ApplyHoverzone(Vector2 screenPosMm)
    {
        float deadzoneRad = PositionStabiliser.DefaultDeadzoneRadius + _hoverDeadzoneEnlargementDistance;
        _previousHoverPosDeadzone = PositionStabiliser.ApplyDeadzoneSized(_previousHoverPosDeadzone, screenPosMm, deadzoneRad);
        return _previousHoverPosDeadzone;
    }

    private InputActionResult HandleInteractions()
    {
        var inputActionResult = CreateInputActionResult(InputType.MOVE, positions, _progressTimer.GetProgress(LatestTimestamp));

        if (!_clickHeld && !_hoverTriggered && positions.CursorPosition == _previousScreenPos)
        {
            if (!_hoverTriggerTimer.IsRunning)
            {
                _hoverTriggerTimer.Restart(LatestTimestamp);
            }
            else if (_hoverTriggerTimer.HasBeenRunningForThreshold(LatestTimestamp, _hoverTriggerTime))
            {
                _hoverTriggered = true;
                _hoverTriggerTimer.Stop();
                _hoverTriggeredDeadzoneRadius = PositionStabiliser.CurrentDeadzoneRadius;
                _previousScreenPos = positions.CursorPosition; // To prevent instant-abandonment of hover
            }
        }

        if (_hoverTriggered)
        {
            if (positions.CursorPosition == _previousScreenPos)
            {
                if (!_clickHeld)
                {
                    if (!_progressTimer.IsRunning && _progressTimer.GetProgress(LatestTimestamp) == 0f)
                    {
                        _progressTimer.Restart(LatestTimestamp);
                    }
                    else if (_progressTimer.IsRunning && _progressTimer.GetProgress(LatestTimestamp) == 1f)
                    {
                        PositionStabiliser.CurrentDeadzoneRadius = (_timerDeadzoneEnlargementDistance + PositionStabiliser.DefaultDeadzoneRadius);
                        _progressTimer.Stop();
                        _clickHeld = true;
                        _clickingTimer.Restart(LatestTimestamp);
                        inputActionResult = CreateInputActionResult(InputType.DOWN, positions, 1f);
                    }
                    else
                    {
                        float maxDeadzoneRadius = _timerDeadzoneEnlargementDistance + PositionStabiliser.DefaultDeadzoneRadius;
                        float deadzoneRadius = Utilities.Lerp(_hoverTriggeredDeadzoneRadius, maxDeadzoneRadius, _progressTimer.GetProgress(LatestTimestamp));

                        PositionStabiliser.CurrentDeadzoneRadius = deadzoneRadius;
                    }
                }
                else
                {
                    if (!_clickAlreadySent && _clickingTimer.HasBeenRunningForThreshold(LatestTimestamp, _clickHoldTime))
                    {
                        inputActionResult = CreateInputActionResult(InputType.UP, positions, _progressTimer.GetProgress(LatestTimestamp));
                        _clickAlreadySent = true;
                    }
                }
            }
            else
            {
                if (_clickHeld && !_clickAlreadySent)
                {
                    // Handle unclick if move before timer's up
                    inputActionResult = CreateInputActionResult(InputType.UP, positions, _progressTimer.GetProgress(LatestTimestamp));
                }

                _progressTimer.Stop();

                _hoverTriggered = false;
                _hoverTriggerTimer.Stop();

                _clickHeld = false;
                _clickAlreadySent = false;
                _clickingTimer.Stop();

                PositionStabiliser.StartShrinkingDeadzone(_deadzoneShrinkSpeed);
            }
        }

        _previousScreenPos = positions.CursorPosition;

        return inputActionResult;
    }

    protected override void OnInteractionSettingsUpdated(InteractionConfigInternal interactionConfig)
    {
        base.OnInteractionSettingsUpdated(interactionConfig);
        if (interactionConfig.HoverAndHold != null)
        {
            _hoverTriggerTime = interactionConfig.HoverAndHold.HoverStartTimeS * 1000; // s to ms
            _progressTimer.TimeLimit = interactionConfig.HoverAndHold.HoverCompleteTimeS * 1000; // s to ms
        }
    }
}
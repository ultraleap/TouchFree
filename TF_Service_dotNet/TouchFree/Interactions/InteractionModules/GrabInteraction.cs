using System;
using System.Numerics;
using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Library.Connections;
using Ultraleap.TouchFree.Library.Interactions.GrabDetector;

namespace Ultraleap.TouchFree.Library.Interactions.InteractionModules;

public class GrabInteraction : InteractionModule
{
    public override InteractionType InteractionType => InteractionType.GRAB;

    private readonly float _deadzoneEnlargementDistance = 20f;
    private readonly float _deadzoneShrinkSpeed = 0.3f;

    private readonly float _maxHandVelocity = 150f;

    private readonly GeneralisedGrabDetector _grabDetector;
    private readonly float _dragStartDistanceThresholdMm = 10f;

    private bool _pressing = false;
    private bool _requireHold = false;

    private readonly int _requiredHoldFrames = 1;
    private int _heldFrames = 0;

    private bool _requireClick = false;

    // Dragging
    private Vector2 _cursorDownPos;
    private bool _isDragging;

    private Tuple<long, Positions> _previousPosition = new(0, new Positions());

    public GrabInteraction(
        IHandManager handManager,
        IVirtualScreen virtualScreen,
        IConfigManager configManager,
        IClientConnectionManager connectionManager,
        IPositioningModule positioningModule,
        IPositionStabiliser positionStabiliser) : base(handManager, virtualScreen, configManager, connectionManager, positioningModule, positionStabiliser)
    {
        _grabDetector = new GeneralisedGrabDetector();
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

        float velocity = hand.PalmVelocity.Magnitude;
        if (_previousPosition.Item1 != 0)
        {
            // Use the cursor velocity for x-y velocity
            //
            // I find that this velocity is quite similar to hand.PalmVelocity.Magnitude, but (as expected)
            // this velocity calculation gets much closer to 0 when the hand is more still.
            Vector3 previousWorldPosMm = VirtualScreen.VirtualScreenPositionToWorld(_previousPosition.Item2.CursorPosition, _previousPosition.Item2.DistanceFromScreen * 1000);
            Vector3 currentWorldPosMm = VirtualScreen.VirtualScreenPositionToWorld(positions.CursorPosition, positions.DistanceFromScreen * 1000);
            float changeInPos = (currentWorldPosMm - previousWorldPosMm).Length();
            float changeInTime = (LatestTimestamp - _previousPosition.Item1) / (1000f * 1000f);
            velocity = changeInPos / changeInTime;
        }
        var inputActionResult = HandleInteractions(hand, velocity);
        _previousPosition = new Tuple<long, Positions>(LatestTimestamp, positions);

        return inputActionResult;
    }

    private InputActionResult HandleInteractions(Leap.Hand hand, float velocity)
    {
        // If already pressing, continue regardless of velocity
        if (_grabDetector.IsGrabbing(hand) && (_pressing || velocity < (_maxHandVelocity * 1000f)))
        {
            return HandleInvoke();
        }
        else if (_pressing)
        {
            return HandleUnclick();
        }
        else
        {
            return CreateInputActionResult(InputType.MOVE, positions, _grabDetector.GeneralisedGrabStrength);
        }
    }

    private InputActionResult HandleInvoke()
    {
        // we are touching the screen
        if (!_pressing)
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
        var inputActionResult = CreateInputActionResult(InputType.DOWN, positions, _grabDetector.GeneralisedGrabStrength);
        _pressing = true;
        if (IgnoreDragging)
        {
            _requireHold = true;
            _heldFrames = 0;
            _requireClick = false;
        }

        // Adjust deadzone
        PositionStabiliser.StopShrinkingDeadzone();
        float newDeadzoneRadius = _deadzoneEnlargementDistance + PositionStabiliser.DefaultDeadzoneRadius;
        PositionStabiliser.CurrentDeadzoneRadius = newDeadzoneRadius;
        _cursorDownPos = positions.CursorPosition;

        return inputActionResult;
    }

    private InputActionResult HandlePressHold()
    {
        if (_isDragging)
        {
            return CreateInputActionResult(InputType.MOVE, positions, _grabDetector.GeneralisedGrabStrength);
        }
        else
        {
            Positions downPositions = positions with { CursorPosition = _cursorDownPos };
            if (IgnoreDragging)
            {
                if (_requireHold)
                {
                    if (_heldFrames >= _requiredHoldFrames)
                    {
                        _requireHold = false;
                        _requireClick = true;
                        _heldFrames = 0;
                    }
                    else
                    {
                        _heldFrames += 1;
                    }
                    return CreateInputActionResult(InputType.MOVE, downPositions, _grabDetector.GeneralisedGrabStrength);
                }
                else if (_requireClick)
                {
                    PositionStabiliser.StartShrinkingDeadzone(_deadzoneShrinkSpeed);
                    _requireClick = false;
                    return CreateInputActionResult(InputType.UP, downPositions, _grabDetector.GeneralisedGrabStrength);
                }
                else
                {
                    return CreateInputActionResult(InputType.MOVE, positions, _grabDetector.GeneralisedGrabStrength);
                }
            }
            else
            {
                // Lock in to the touch down position until dragging occurs.
                if (CheckForStartDrag(_cursorDownPos, positions.CursorPosition) && !IgnoreDragging)
                {
                    _isDragging = true;
                    PositionStabiliser.StartShrinkingDeadzone(_deadzoneShrinkSpeed);
                }

                return CreateInputActionResult(InputType.MOVE, downPositions, _grabDetector.GeneralisedGrabStrength);
            }
        }
    }

    private InputActionResult HandleUnclick()
    {
        var inputActionResult = new InputActionResult();
        // Check if an unclick is needed, and perform if so
        if (!IgnoreDragging)
        {
            if (!_requireHold && !_requireClick)
            {
                inputActionResult = CreateInputActionResult(InputType.UP, positions, _grabDetector.GeneralisedGrabStrength);
            }

            PositionStabiliser.StartShrinkingDeadzone(_deadzoneShrinkSpeed);
        }

        _pressing = false;
        _isDragging = false;

        return inputActionResult;
    }

    protected override bool CheckForStartDrag(Vector2 startPos, Vector2 currentPos)
    {
        var a = VirtualScreen.PixelsToMillimeters(startPos);
        var b = VirtualScreen.PixelsToMillimeters(currentPos);
        var distFromStartPos = (a - b).Length();

        if (distFromStartPos > _dragStartDistanceThresholdMm)
        {
            return true;
        }

        return false;
    }
}
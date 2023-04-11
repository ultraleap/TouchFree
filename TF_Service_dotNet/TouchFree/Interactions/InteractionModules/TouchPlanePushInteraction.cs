using System;
using System.Numerics;
using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Library.Connections;

namespace Ultraleap.TouchFree.Library.Interactions.InteractionModules;

public class TouchPlanePushInteraction : InteractionModule
{
    public override InteractionType InteractionType => InteractionType.TOUCHPLANE;

    // The distance from the touchPlane (in mm) at which the progressToClick is 0
    private const float _touchPlaneZeroProgressMm = 100f;

    // The distance from screen (in mm) at which the progressToClick is 1
    private float _touchPlaneDistanceMm = 50f;

    private bool _pressing = false;
    private bool _pressComplete = false;

    private Vector2 _downPos;

    // Used to ignore hands that initialise while past the touchPlane.
    // Particularly for those that are cancelled by InteractionZones
    private bool _handReady = false;

    private readonly float _dragStartDistanceThresholdMm = 30f;
    private bool _isDragging = false;

    public TouchPlanePushInteraction(
        IHandManager handManager,
        IVirtualScreen virtualScreen,
        IConfigManager configManager,
        IClientConnectionManager connectionManager,
        IPositioningModule positioningModule,
        IPositionStabiliser positionStabiliser) : base(handManager, virtualScreen, configManager, connectionManager, positioningModule, positionStabiliser)
    {
        PositionConfiguration = new[]
        {
            new PositionTrackerConfiguration(TrackedPosition.NEAREST, 1)
        };
    }

    protected override InputActionResult UpdateData(Leap.Hand hand, float confidence)
    {
        if (hand == null)
        {
            _pressComplete = false;
            _isDragging = false;
            _pressing = false;
            _handReady = false;

            if (HadHandLastFrame)
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

        float progressToClick = Math.Clamp(1f - Utilities.InverseLerp(_touchPlaneDistanceMm, _touchPlaneDistanceMm + _touchPlaneZeroProgressMm, DistanceFromScreenMm), 0f, 1f);

        // determine if the fingertip is across one of the surface thresholds (hover/press) and send event
        if (DistanceFromScreenMm < _touchPlaneDistanceMm)
        {
            if (_handReady)
            {
                // we are touching the screen
                if (!_pressing)
                {
                    _downPos = currentCursorPosition;
                    _pressing = true;

                    if (!IgnoreDragging)
                    {
                        PositionStabiliser.CurrentDeadzoneRadius = PositionStabiliser.DefaultDeadzoneRadius + _dragStartDistanceThresholdMm;
                    }

                    return CreateInputActionResult(InputType.DOWN, positions, progressToClick);
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
                        return CreateInputActionResult(InputType.MOVE, positions, progressToClick);
                    }
                    else
                    {
                        // NONE causes the client to react to data without using Input.
                        return CreateInputActionResult(InputType.NONE, positions, progressToClick);
                    }
                }
                else if (!_pressComplete)
                {
                    _pressComplete = true;

                    Positions downPositions = new Positions(_downPos, positions.DistanceFromScreen);
                    PositionStabiliser.ResetValues();
                    return CreateInputActionResult(InputType.UP, downPositions, progressToClick);
                }
            }
        }
        else
        {
            InputActionResult result;
            if (_pressing && !_pressComplete)
            {
                PositionStabiliser.ResetValues();
                Positions downPositions = new Positions(_downPos, positions.DistanceFromScreen);
                result = CreateInputActionResult(InputType.UP, downPositions, progressToClick);
            }
            else
            {
                result = CreateInputActionResult(InputType.MOVE, positions, progressToClick);
            }

            _pressComplete = false;
            _pressing = false;
            _isDragging = false;
            _handReady = true;

            return result;
        }

        return new InputActionResult();
    }

    protected override void OnInteractionSettingsUpdated(InteractionConfigInternal interactionConfig)
    {
        base.OnInteractionSettingsUpdated(interactionConfig);

        _touchPlaneDistanceMm = interactionConfig.TouchPlane.TouchPlaneActivationDistanceMm;
        PositionConfiguration = new[]
        {
            new PositionTrackerConfiguration(interactionConfig.TouchPlane.TouchPlaneTrackedPosition, 1)
        };
    }
}
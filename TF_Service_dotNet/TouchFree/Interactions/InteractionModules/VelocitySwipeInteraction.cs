using System.Diagnostics;
using System.Numerics;
using Microsoft.Extensions.Options;
using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Library.Connections;

namespace Ultraleap.TouchFree.Library.Interactions.InteractionModules;

public class VelocitySwipeInteraction : InteractionModule
{
    public override InteractionType InteractionType => InteractionType.VELOCITYSWIPE;

    private float _minScrollVelocityMmps = 625f;
    private float _upwardsMinVelocityDecreaseMmps = 50f;
    private float _downwardsMinVelocityIncreaseMmps = 50f;
    private float _maxReleaseVelocityMmps = 200f;

    private float _maxLateralVelocityMmps = 300f;
    private float _maxOpposingVelocityMmps = 65f;

    private float _minSwipeLength = 10f;
    private float _maxSwipeWidth = 10f;
    private float _swipeWidthScaling = 0.2f;

    private double _scrollDelayMs = 450;
    private readonly Stopwatch _scrollDelayStopwatch = new();

    private Axis _lockAxisToOnly = Axis.None;
    private bool _allowBidirectional = false;

    private readonly PositionFilter _filter;


    private bool _pressing = false;
    private Direction _currentDirection;

    private bool _scrollDisallowed = false;

    private long _previousTime = 0;

    private Vector2 _previousScreenPos = Vector2.Zero;
    private Vector2 _scrollOrigin = Vector2.Zero;
    private Vector2? _potentialScrollOrigin;


    public VelocitySwipeInteraction(
        IHandManager handManager,
        IVirtualScreen virtualScreen,
        IConfigManager configManager,
        IClientConnectionManager connectionManager,
        IOptions<InteractionTuning> interactionTuning,
        IPositioningModule positioningModule,
        IPositionStabiliser positionStabiliser) : base(handManager, virtualScreen, configManager, connectionManager, positioningModule, positionStabiliser)
    {
        if (configManager.InteractionConfig?.VelocitySwipe != null)
        {
            OnInteractionConfigUpdated(configManager.InteractionConfig);
        }

        _filter = new PositionFilter(interactionTuning);

        PositionConfiguration = new[]
        {
            new PositionTrackerConfiguration(TrackedPosition.INDEX_TIP, 1)
        };

        configManager.OnInteractionConfigUpdated += OnInteractionConfigUpdated;
    }

    private void OnInteractionConfigUpdated(InteractionConfigInternal config)
    {
        _minScrollVelocityMmps = config.VelocitySwipe.MinScrollVelocity_mmps;
        _upwardsMinVelocityDecreaseMmps = config.VelocitySwipe.UpwardsMinVelocityDecrease_mmps;
        _downwardsMinVelocityIncreaseMmps = config.VelocitySwipe.DownwardsMinVelocityIncrease_mmps;
        _maxReleaseVelocityMmps = config.VelocitySwipe.MaxReleaseVelocity_mmps;
        _maxLateralVelocityMmps = config.VelocitySwipe.MaxLateralVelocity_mmps;
        _maxOpposingVelocityMmps = config.VelocitySwipe.MaxOpposingVelocity_mmps;
        _scrollDelayMs = config.VelocitySwipe.ScrollDelayMs;
        _minSwipeLength = config.VelocitySwipe.MinSwipeLength;
        _maxSwipeWidth = config.VelocitySwipe.MaxSwipeWidth;
        _swipeWidthScaling = config.VelocitySwipe.SwipeWidthScaling;

        if (config.VelocitySwipe.AllowHorizontalScroll && config.VelocitySwipe.AllowVerticalScroll)
        {
            _allowBidirectional = config.VelocitySwipe.AllowBidirectionalScroll;
        }
        else if (config.VelocitySwipe.AllowHorizontalScroll)
        {
            _lockAxisToOnly = Axis.X;
        }
        else if (config.VelocitySwipe.AllowVerticalScroll)
        {
            _lockAxisToOnly = Axis.Y;
        }
    }

    protected override InputActionResult UpdateData(Leap.Hand hand, float confidence)
    {
        if (hand == null)
        {
            _pressing = false;

            if (HadHandLastFrame)
            {
                // We lost the hand so cancel anything we may have been doing
                return CreateInputActionResult(InputType.CANCEL, positions, 0);
            }
            return new InputActionResult();
        }

        return HandleInteractions(confidence);
    }

    protected override Positions ApplyAdditionalPositionModifiers(Positions pos) =>
        base.ApplyAdditionalPositionModifiers(pos)
            .ApplyModifier(_filter);

    private InputActionResult HandleInteractions(float confidence)
    {
        Vector2 dPerpPx = positions.CursorPosition - _previousScreenPos;
        Vector2 dPerp = VirtualScreen.PixelsToMillimeters(dPerpPx);

        long dtMicroseconds = (LatestTimestamp - _previousTime);
        float dt = dtMicroseconds / (1000f * 1000f);     // Seconds

        dPerp = dPerp * confidence / dt; // Multiply by confidence to make it harder to use when disused

        Vector2 absPerp = Vector2.Abs(dPerp);

        InputActionResult inputActionResult;

        if (!_pressing && CheckIfScrollStart(dPerp, absPerp))
        {
            if (_potentialScrollOrigin.HasValue)
            {
                var changeFromPossibleOrigin = Vector2.Abs(positions.CursorPosition - _potentialScrollOrigin.Value);
                if (changeFromPossibleOrigin.X > _minSwipeLength || changeFromPossibleOrigin.Y > _minSwipeLength)
                {
                    _pressing = true;
                    _scrollOrigin = _previousScreenPos;
                    _potentialScrollOrigin = null;

                    SetDirection(dPerp, absPerp);

                    inputActionResult = CreateInputActionResult(InputType.DOWN, positions, 1f);
                }
                else
                {
                    inputActionResult = CreateInputActionResult(InputType.MOVE, positions, 0);
                }
            }
            else
            {
                _potentialScrollOrigin = _previousScreenPos;
                inputActionResult = CreateInputActionResult(InputType.MOVE, positions, 0);
            }
        }
        else if (_pressing && CheckIfScrollEnd(dPerp))
        {
            _scrollDelayStopwatch.Restart();
            _scrollDisallowed = true;
            _pressing = false;
            inputActionResult = CreateInputActionResult(InputType.UP, positions, 0);
        }
        else
        {
            _potentialScrollOrigin = null;
            inputActionResult = CreateInputActionResult(InputType.MOVE, positions, _pressing ? 1 : 0);
        }

        _previousScreenPos = positions.CursorPosition;
        _previousTime = LatestTimestamp;

        return inputActionResult;
    }

    private void SetDirection(Vector2 dPerp, Vector2 absPerp)
    {
        if (absPerp.X >= absPerp.Y)
        {
            _currentDirection = dPerp.X > 0 ? Direction.Right : Direction.Left;
        }
        else
        {
            _currentDirection = dPerp.Y > 0 ? Direction.Up : Direction.Down;
        }
    }

    private bool CheckIfScrollStart(Vector2 dPerp, Vector2 absPerp)
    {
        if (!CheckIfScrollAllowed(dPerp))
        {
            return false;
        }

        if (_allowBidirectional)
        {
            if (absPerp.X > _minScrollVelocityMmps || VerticalVelocityOverMinScrollVelocity(dPerp))
            {
                return true;
            }
        }
        else
        {
            if (((absPerp.X > _minScrollVelocityMmps) && (absPerp.Y < _maxLateralVelocityMmps) && _lockAxisToOnly != Axis.Y) ||
                (VerticalVelocityOverMinScrollVelocity(dPerp) && (absPerp.X < _maxLateralVelocityMmps) && _lockAxisToOnly != Axis.X))
            {
                return true;
            }
        }

        return false;
    }

    private bool VerticalVelocityOverMinScrollVelocity(Vector2 dPerp) =>
        dPerp.Y > (_minScrollVelocityMmps - _upwardsMinVelocityDecreaseMmps)
        || -dPerp.Y > (_downwardsMinVelocityIncreaseMmps + _minScrollVelocityMmps);

    private bool CheckIfScrollAllowed(Vector2 dPerp)
    {
        if (!_scrollDisallowed) return true;
        if (_scrollDelayStopwatch.IsRunning && _scrollDelayStopwatch.ElapsedMilliseconds > _scrollDelayMs)
        {
            _scrollDisallowed = _currentDirection switch
            {
                Direction.Left => dPerp.X >= _maxOpposingVelocityMmps,
                Direction.Right => dPerp.X <= -_maxOpposingVelocityMmps,
                Direction.Up => dPerp.Y <= -_maxOpposingVelocityMmps,
                Direction.Down => dPerp.Y >= _maxOpposingVelocityMmps,
                _ => _scrollDisallowed
            };
        }

        return false;
    }

    private bool CheckIfScrollEnd(Vector2 dPerp)
    {
        var changeFromScrollOriginPx = positions.CursorPosition - _scrollOrigin;
        var changeFromScrollOriginMm = Vector2.Abs(VirtualScreen.PixelsToMillimeters(changeFromScrollOriginPx));

        return _currentDirection switch
        {
            Direction.Left => dPerp.X > -_maxReleaseVelocityMmps || changeFromScrollOriginMm.Y > (_maxSwipeWidth + _swipeWidthScaling * changeFromScrollOriginMm.X),
            Direction.Right => dPerp.X < _maxReleaseVelocityMmps || changeFromScrollOriginMm.Y > (_maxSwipeWidth + _swipeWidthScaling * changeFromScrollOriginMm.X),
            Direction.Up => dPerp.Y < _maxReleaseVelocityMmps || changeFromScrollOriginMm.X > (_maxSwipeWidth + _swipeWidthScaling * changeFromScrollOriginMm.Y),
            Direction.Down => dPerp.Y > -_maxReleaseVelocityMmps || changeFromScrollOriginMm.X > (_maxSwipeWidth + _swipeWidthScaling * changeFromScrollOriginMm.Y),
            _ => false
        };
    }

    private enum Direction
    {
        Left,
        Right,
        Up,
        Down
    }

    private enum Axis
    {
        None,
        X,
        Y
    }
}
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Numerics;
using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Library.Interactions.InteractionModules;

namespace Ultraleap.TouchFree.Library.Interactions
{
    public class VelocitySwipeInteraction : InteractionModule
    {
        public override InteractionType InteractionType { get; } = InteractionType.VELOCITYSWIPE;

        private float minScrollVelocity_mmps = 625f;
        private float upwardsMinVelocityDecrease_mmps = 50f;
        private float downwardsMinVelocityIncrease_mmps = 50f;
        private float maxReleaseVelocity_mmps = 200f;

        private float maxLateralVelocity_mmps = 300f;
        private float maxOpposingVelocity_mmps = 65f;

        private float minSwipeLength = 10f;
        private float maxSwipeWidth = 10f;
        private float swipeWidthScaling = 0.2f;

        private double scrollDelayMs = 450;
        private readonly Stopwatch scrollDelayStopwatch = new Stopwatch();

        private Axis lockAxisToOnly = Axis.NONE;
        private bool allowBidirectional = false;

        private readonly PositionFilter filter;


        private bool pressing = false;
        private Direction currentDirection;

        private bool scrollDisallowed = false;

        private long previousTime = 0;

        private Vector2 previousScreenPos = Vector2.Zero;
        private Vector2 scrollOrigin = Vector2.Zero;
        private Vector2? potentialScrollOrigin;


        public VelocitySwipeInteraction(
            IHandManager _handManager,
            IVirtualScreen _virtualScreen,
            IConfigManager _configManager,
            IOptions<InteractionTuning> _interactionTuning,
            IPositioningModule _positioningModule,
            IPositionStabiliser _positionStabiliser) : base(_handManager, _virtualScreen, _configManager, _positioningModule, _positionStabiliser)
        {
            if (_configManager.InteractionConfig?.VelocitySwipe != null)
            {
                OnInteractionConfigUpdated(_configManager.InteractionConfig);
            }

            filter = new PositionFilter(_interactionTuning);

            positionConfiguration = new[]
            {
                new PositionTrackerConfiguration(TrackedPosition.INDEX_TIP, 1)
            };

            _configManager.OnInteractionConfigUpdated += OnInteractionConfigUpdated;
        }

        private void OnInteractionConfigUpdated(InteractionConfigInternal config)
        {
            minScrollVelocity_mmps = config.VelocitySwipe.MinScrollVelocity_mmps;
            upwardsMinVelocityDecrease_mmps = config.VelocitySwipe.UpwardsMinVelocityDecrease_mmps;
            downwardsMinVelocityIncrease_mmps = config.VelocitySwipe.DownwardsMinVelocityIncrease_mmps;
            maxReleaseVelocity_mmps = config.VelocitySwipe.MaxReleaseVelocity_mmps;
            maxLateralVelocity_mmps = config.VelocitySwipe.MaxLateralVelocity_mmps;
            maxOpposingVelocity_mmps = config.VelocitySwipe.MaxOpposingVelocity_mmps;
            scrollDelayMs = config.VelocitySwipe.ScrollDelayMs;
            minSwipeLength = config.VelocitySwipe.MinSwipeLength;
            maxSwipeWidth = config.VelocitySwipe.MaxSwipeWidth;
            swipeWidthScaling = config.VelocitySwipe.SwipeWidthScaling;

            if (config.VelocitySwipe.AllowHorizontalScroll && config.VelocitySwipe.AllowVerticalScroll)
            {
                allowBidirectional = config.VelocitySwipe.AllowBidirectionalScroll;
            }
            else if (config.VelocitySwipe.AllowHorizontalScroll)
            {
                lockAxisToOnly = Axis.X;
            }
            else if (config.VelocitySwipe.AllowVerticalScroll)
            {
                lockAxisToOnly = Axis.Y;
            }
        }

        protected override InputActionResult UpdateData(Leap.Hand hand, float confidence)
        {
            if (hand == null)
            {
                pressing = false;

                if (hadHandLastFrame)
                {
                    // We lost the hand so cancel anything we may have been doing
                    return CreateInputActionResult(InputType.CANCEL, positions, 0);
                }
                return new InputActionResult();
            }

            return HandleInteractions(confidence);
        }

        protected override Positions ApplyAdditionalPositionModifiers(Positions positions)
        {
            var returnPositions = base.ApplyAdditionalPositionModifiers(positions);
            returnPositions.CursorPosition = filter.ApplyModification(returnPositions.CursorPosition);
            return returnPositions;
        }

        private InputActionResult HandleInteractions(float confidence)
        {
            Vector2 dPerpPx = positions.CursorPosition - previousScreenPos;
            Vector2 dPerp = virtualScreen.PixelsToMillimeters(dPerpPx);

            long dtMicroseconds = (latestTimestamp - previousTime);
            float dt = dtMicroseconds / (1000f * 1000f);     // Seconds

            dPerp = dPerp * confidence / dt; // Multiply by confidence to make it harder to use when disused

            Vector2 absPerp = Vector2.Abs(dPerp);

            InputActionResult inputActionResult;

            if (!pressing && CheckIfScrollStart(dPerp, absPerp))
            {
                if (potentialScrollOrigin.HasValue)
                {
                    var changeFromPossibleOrigin = Vector2.Abs(positions.CursorPosition - potentialScrollOrigin.Value);
                    if (changeFromPossibleOrigin.X > minSwipeLength || changeFromPossibleOrigin.Y > minSwipeLength)
                    {
                        pressing = true;
                        scrollOrigin = previousScreenPos;
                        potentialScrollOrigin = null;

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
                    potentialScrollOrigin = previousScreenPos;
                    inputActionResult = CreateInputActionResult(InputType.MOVE, positions, 0);
                }
            }
            else if (pressing && CheckIfScrollEnd(dPerp))
            {
                scrollDelayStopwatch.Restart();
                scrollDisallowed = true;
                pressing = false;
                inputActionResult = CreateInputActionResult(InputType.UP, positions, 0);
            }
            else
            {
                potentialScrollOrigin = null;
                inputActionResult = CreateInputActionResult(InputType.MOVE, positions, pressing ? 1 : 0);
            }

            previousScreenPos = positions.CursorPosition;
            previousTime = latestTimestamp;

            return inputActionResult;
        }

        void SetDirection(Vector2 _dPerp, Vector2 _absPerp)
        {
            if (_absPerp.X >= _absPerp.Y)
            {
                currentDirection = _dPerp.X > 0 ? Direction.RIGHT : Direction.LEFT;
            }
            else
            {
                currentDirection = _dPerp.Y > 0 ? Direction.UP : Direction.DOWN;
            }
        }

        bool CheckIfScrollStart(Vector2 _dPerp, Vector2 _absPerp)
        {
            if (!CheckIfScrollAllowed(_dPerp))
            {
                return false;
            }

            if (allowBidirectional)
            {
                if (_absPerp.X > minScrollVelocity_mmps || VerticalVelocityOverMinScrollVelocity(_dPerp))
                {
                    return true;
                }
            }
            else
            {
                if (((_absPerp.X > minScrollVelocity_mmps) && (_absPerp.Y < maxLateralVelocity_mmps) && lockAxisToOnly != Axis.Y) ||
                    (VerticalVelocityOverMinScrollVelocity(_dPerp) && (_absPerp.X < maxLateralVelocity_mmps) && lockAxisToOnly != Axis.X))
                {
                    return true;
                }
            }

            return false;
        }

        bool VerticalVelocityOverMinScrollVelocity(Vector2 _dPerp)
        {
            return _dPerp.Y > (minScrollVelocity_mmps - upwardsMinVelocityDecrease_mmps) || -_dPerp.Y > (downwardsMinVelocityIncrease_mmps + minScrollVelocity_mmps);
        }

        bool CheckIfScrollAllowed(Vector2 _dPerp)
        {
            if (scrollDisallowed)
            {
                if (scrollDelayStopwatch.IsRunning && scrollDelayStopwatch.ElapsedMilliseconds > scrollDelayMs)
                {
                    switch (currentDirection)
                    {
                        case Direction.LEFT:
                            scrollDisallowed = _dPerp.X >= maxOpposingVelocity_mmps;
                            break;
                        case Direction.RIGHT:
                            scrollDisallowed = _dPerp.X <= -maxOpposingVelocity_mmps;
                            break;
                        case Direction.UP:
                            scrollDisallowed = _dPerp.Y <= -maxOpposingVelocity_mmps;
                            break;
                        case Direction.DOWN:
                            scrollDisallowed = _dPerp.Y >= maxOpposingVelocity_mmps;
                            break;
                    }
                }

                return false;
            }

            return true;
        }

        bool CheckIfScrollEnd(Vector2 _dPerp)
        {
            var changeFromScrollOriginPx = positions.CursorPosition - scrollOrigin;
            var changeFromScrollOriginMm = Vector2.Abs(virtualScreen.PixelsToMillimeters(changeFromScrollOriginPx));

            switch (currentDirection)
            {
                case Direction.LEFT:
                    return _dPerp.X > -maxReleaseVelocity_mmps || changeFromScrollOriginMm.Y > (maxSwipeWidth + swipeWidthScaling * changeFromScrollOriginMm.X);
                case Direction.RIGHT:
                    return _dPerp.X < maxReleaseVelocity_mmps || changeFromScrollOriginMm.Y > (maxSwipeWidth + swipeWidthScaling * changeFromScrollOriginMm.X);
                case Direction.UP:
                    return _dPerp.Y < maxReleaseVelocity_mmps || changeFromScrollOriginMm.X > (maxSwipeWidth + swipeWidthScaling * changeFromScrollOriginMm.Y);
                case Direction.DOWN:
                    return _dPerp.Y > -maxReleaseVelocity_mmps || changeFromScrollOriginMm.X > (maxSwipeWidth + swipeWidthScaling * changeFromScrollOriginMm.Y);
                default:
                    return false;
            }
        }

        enum Direction
        {
            LEFT,
            RIGHT,
            UP,
            DOWN
        }

        enum Axis
        {
            NONE,
            X,
            Y
        }
    }
}
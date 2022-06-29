﻿using System;
using System.Numerics;
using System.Diagnostics;

using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Library.Interactions.InteractionModules;
using Microsoft.Extensions.Options;

namespace Ultraleap.TouchFree.Library.Interactions
{
    public class VelocitySwipeInteraction : InteractionModule
    {
        public override InteractionType InteractionType { get; } = InteractionType.VELOCITYSWIPE;

        private readonly float minScrollVelocity_mmps = 500f;
        private readonly float maxReleaseVelocity_mmps = 40f;

        private readonly float maxOpposingVelocity_mmps = 150f;
        private readonly float minSwipeWidth = 10f;
        private readonly float swipeWidthScaling = 0.2f;

        private readonly double scrollDelayMs = 300;
        private readonly Stopwatch scrollDelayStopwatch = new Stopwatch();

        private readonly Axis lockAxisToOnly = Axis.NONE;
        private readonly bool allowBidirectional = false;

        private readonly PositionFilter filter;


        private bool pressing = false;
        private Direction currentDirection;

        private bool scrollDisallowed = false;

        private long previousTime = 0;

        private Vector2 previousScreenPos = Vector2.Zero;
        private Vector2 scrollOrigin = Vector2.Zero;
        private Vector2? potentialScrollOrigin;


        public VelocitySwipeInteraction(
            HandManager _handManager,
            IVirtualScreen _virtualScreen,
            IConfigManager _configManager,
            IOptions<InteractionTuning> _interactionTuning,
            IPositioningModule _positioningModule,
            IPositionStabiliser _positionStabiliser) : base(_handManager, _virtualScreen, _configManager, _positioningModule, _positionStabiliser)
        {
            if (_interactionTuning?.Value?.VelocitySwipeSettings != null)
            {
                minScrollVelocity_mmps = _interactionTuning.Value.VelocitySwipeSettings.MinScrollVelocity_mmps;
                maxReleaseVelocity_mmps = _interactionTuning.Value.VelocitySwipeSettings.MaxReleaseVelocity_mmps;
                maxOpposingVelocity_mmps = _interactionTuning.Value.VelocitySwipeSettings.MaxOpposingVelocity_mmps;
                scrollDelayMs = _interactionTuning.Value.VelocitySwipeSettings.ScrollDelayMs;

                if (_interactionTuning.Value.VelocitySwipeSettings.AllowHorizontalScroll && _interactionTuning.Value.VelocitySwipeSettings.AllowVerticalScroll)
                {
                    allowBidirectional = _interactionTuning.Value.VelocitySwipeSettings.AllowBidirectionalScroll;
                }
                else if (_interactionTuning.Value.VelocitySwipeSettings.AllowHorizontalScroll)
                {
                    lockAxisToOnly = Axis.X;
                }
                else if (_interactionTuning.Value.VelocitySwipeSettings.AllowVerticalScroll)
                {
                    lockAxisToOnly = Axis.Y;
                }
            }

            filter = new PositionFilter(_interactionTuning);

            positionConfiguration = new[]
            {
                new PositionTrackerConfiguration(TrackedPosition.INDEX_TIP, 1)
            };
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
                    if (changeFromPossibleOrigin.X > minSwipeWidth || changeFromPossibleOrigin.Y > minSwipeWidth)
                    {
                        pressing = true;
                        scrollOrigin = previousScreenPos;
                        potentialScrollOrigin = null;

                        SetDirection(dPerp, absPerp);

                        inputActionResult = CreateInputActionResult(InputType.DOWN, positions, 0);
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
            if(!CheckIfScrollAllowed(_dPerp))
            {
                return false;
            }

            if (allowBidirectional)
            {
                if (_absPerp.X > minScrollVelocity_mmps || _absPerp.Y > minScrollVelocity_mmps)
                {
                    return true;
                }
            }
            else
            {
                if (((_absPerp.X > minScrollVelocity_mmps) && (_absPerp.Y < maxOpposingVelocity_mmps) && lockAxisToOnly != Axis.Y) ||
                    ((_absPerp.Y > minScrollVelocity_mmps) && (_absPerp.X < maxOpposingVelocity_mmps) && lockAxisToOnly != Axis.X))
                {
                    return true;
                }
            }

            return false;
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
                    return _dPerp.X > -maxReleaseVelocity_mmps || changeFromScrollOriginMm.Y > (minSwipeWidth + swipeWidthScaling * changeFromScrollOriginMm.X);
                case Direction.RIGHT:
                    return _dPerp.X < maxReleaseVelocity_mmps || changeFromScrollOriginMm.Y > (minSwipeWidth + swipeWidthScaling * changeFromScrollOriginMm.X);
                case Direction.UP:
                    return _dPerp.Y < maxReleaseVelocity_mmps || changeFromScrollOriginMm.X > (minSwipeWidth + swipeWidthScaling * changeFromScrollOriginMm.Y);
                case Direction.DOWN:
                    return _dPerp.Y > -maxReleaseVelocity_mmps || changeFromScrollOriginMm.X > (minSwipeWidth + swipeWidthScaling * changeFromScrollOriginMm.Y);
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
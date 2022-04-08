using System;
using System.Numerics;
using System.Diagnostics;

using Ultraleap.TouchFree.Library.Configuration;

namespace Ultraleap.TouchFree.Library.Interactions
{
    public class VelocitySwipeInteraction : InteractionModule
    {
        public override InteractionType InteractionType { get; } = InteractionType.TOUCHPLANE;

        private Vector2 previousScreenPos = Vector2.Zero;

        float minScrollVelocity_mmps = 500f;
        float maxReleaseVelocity_mmps = 0f;

        float maxOpposingVelocity_mmps = 150f;

        bool pressing = false;
        Direction currentDirection;

        double scrollDelayMs = 500;
        Stopwatch scrollDelayStopwatch = new Stopwatch();
        bool scrollDisallowed = false;

        long previousTime = 0;

        Axis lockAxisToOnly = Axis.NONE;
        bool allowBidirectional = false;

        public VelocitySwipeInteraction(
            HandManager _handManager,
            IVirtualScreen _virtualScreen,
            IConfigManager _configManager,
            IPositioningModule _positioningModule) : base(_handManager, _virtualScreen, _configManager, _positioningModule, TrackedPosition.INDEX_TIP)
        {
        }

        protected override void UpdateData(Leap.Hand hand)
        {
            if (hand == null)
            {
                if (hadHandLastFrame)
                {
                    // We lost the hand so cancel anything we may have been doing
                    SendInputAction(InputType.CANCEL, positions, 0);
                }

                pressing = false;
                return;
            }

            HandleInteractions();
        }

        private void HandleInteractions()
        {
            Vector2 dPerpPx = positions.CursorPosition - previousScreenPos;
            Vector2 dPerp = virtualScreen.PixelsToMillimeters(dPerpPx);

            long dtMicroseconds = (latestTimestamp - previousTime);
            float dt = dtMicroseconds / (1000f * 1000f);     // Seconds

            dPerp = dPerp / dt;

            Vector2 absPerp = Vector2.Abs(dPerp);

            if (!pressing && CheckIfScrollStart(absPerp))
            {
                pressing = true;

                SetDirection(dPerp, absPerp);

                SendInputAction(InputType.DOWN, positions, 0);
            }
            else if (pressing && CheckIfChangedDirection(dPerp))
            {
                scrollDelayStopwatch.Restart();
                scrollDisallowed = true;
                pressing = false;
                SendInputAction(InputType.UP, positions, 0);
            }
            else
            {
                if (pressing)
                {
                    SendInputAction(InputType.MOVE, positions, 1);
                }
                else
                {
                    SendInputAction(InputType.MOVE, positions, 0);
                }
            }

            previousScreenPos = positions.CursorPosition;
            previousTime = latestTimestamp;
        }

        void SetDirection(Vector2 _dPerp, Vector2 _absPerp)
        {
            if (_absPerp.X > minScrollVelocity_mmps)
            {
                if(_dPerp.X > 0)
                {
                    currentDirection = Direction.RIGHT;
                }
                else
                {
                    currentDirection = Direction.LEFT;
                }
            }
            else
            {
                if (_dPerp.Y > 0)
                {
                    currentDirection = Direction.UP;
                }
                else
                {
                    currentDirection = Direction.DOWN;
                }
            }
        }

        bool CheckIfScrollStart(Vector2 _absPerp)
        {
            if(!CheckIfScrollAllowed(_absPerp))
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

        bool CheckIfScrollAllowed(Vector2 _absPerp)
        {
            if (scrollDisallowed)
            {
                if (scrollDelayStopwatch.IsRunning && scrollDelayStopwatch.ElapsedMilliseconds > scrollDelayMs)
                {
                    if (_absPerp.X < maxOpposingVelocity_mmps && _absPerp.Y < maxOpposingVelocity_mmps)
                    {
                        scrollDisallowed = false;
                    }
                }

                return false;
            }

            return true;
        }

        bool CheckIfChangedDirection(Vector2 _dPerp)
        {
            switch (currentDirection)
            {
                case Direction.LEFT:
                    if(_dPerp.X > -maxReleaseVelocity_mmps)
                    {
                        return true;
                    }
                    break;
                case Direction.RIGHT:
                    if (_dPerp.X < maxReleaseVelocity_mmps)
                    {
                        return true;
                    }
                    break;
                case Direction.UP:
                    if (_dPerp.Y < maxReleaseVelocity_mmps)
                    {
                        return true;
                    }
                    break;
                case Direction.DOWN:
                    if (_dPerp.Y > -maxReleaseVelocity_mmps)
                    {
                        return true;
                    }
                    break;
            }

            return false;
        }

        protected override void OnInteractionSettingsUpdated(InteractionConfigInternal _config)
        {
            base.OnInteractionSettingsUpdated(_config);

            minScrollVelocity_mmps = _config.minScrollVelocity_mmps;
            maxReleaseVelocity_mmps = _config.maxReleaseVelocity_mmps;
            maxOpposingVelocity_mmps = _config.maxOpposingVelocity_mmps;
            lockAxisToOnly = (Axis)_config.lockAxisToOnly;
            allowBidirectional = _config.allowBidirectional;
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
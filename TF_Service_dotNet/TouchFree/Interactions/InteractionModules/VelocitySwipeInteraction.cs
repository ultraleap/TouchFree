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

        float minScrollVelocity_mmps = 700f;
        float maxReleaseVelocity_mmps = 150f;

        bool pressing = false;
        Direction currentDirection;

        double scrollDelayMs = 300;
        Stopwatch scrollDelayStopwatch = new Stopwatch();

        long previousTime = 0;

        Axis lockAxisToOnly = Axis.Y;

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
            if(scrollDelayStopwatch.IsRunning && scrollDelayStopwatch.ElapsedMilliseconds < scrollDelayMs)
            {
                return false;
            }

            if(((_absPerp.X > minScrollVelocity_mmps) && (_absPerp.Y < maxReleaseVelocity_mmps) && lockAxisToOnly != Axis.Y) || 
                ((_absPerp.Y > minScrollVelocity_mmps) && (_absPerp.X < maxReleaseVelocity_mmps) && lockAxisToOnly != Axis.X))
            {
                return true;
            }

            return false;
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
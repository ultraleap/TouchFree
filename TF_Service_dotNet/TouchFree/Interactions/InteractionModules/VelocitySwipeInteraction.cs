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

        float minScrollVelocitymm = 20f;
        float maxReleaseVelocitymm = 2f;

        bool pressing = false;

        public VelocitySwipeInteraction(
            HandManager _handManager,
            IVirtualScreen _virtualScreen,
            IConfigManager _configManager,
            IPositioningModule _positioningModule) : base(_handManager, _virtualScreen, _configManager, _positioningModule, TrackedPosition.INDEX_STABLE)
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

            if (!pressing && ((MathF.Abs(dPerp.X) > minScrollVelocitymm) || (MathF.Abs(dPerp.Y) > minScrollVelocitymm)))
            {
                pressing = true;

                SendInputAction(InputType.DOWN, positions, 0);
            }
            else if (pressing && ((MathF.Abs(dPerp.X) < maxReleaseVelocitymm) && (MathF.Abs(dPerp.Y) < maxReleaseVelocitymm)))
            {
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
        }
    }
}
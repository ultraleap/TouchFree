using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Ultraleap.TouchFree.Library.Interactions.PositionTrackers;

namespace Ultraleap.TouchFree.Library.Interactions
{
    public class PositioningModule : IPositioningModule
    {
        private Positions positions;

        private readonly IVirtualScreen virtualScreen;

        private readonly IEnumerable<IPositionTracker> positionTrackers;

        public PositioningModule(IVirtualScreen _virtualScreen, IEnumerable<IPositionTracker> _positionTrackers)
        {
            virtualScreen = _virtualScreen;
            positionTrackers = _positionTrackers;
        }

        public Positions CalculatePositions(Leap.Hand hand, IEnumerable<PositionTrackerConfiguration> configuration)
        {
            if (hand == null)
            {
                return positions;
            }

            int totalWeights = configuration.Sum(x => x.weighting);
            Vector3 worldPosM = new Vector3();
            
            foreach (var positionItem in configuration)
            {
                worldPosM += GetPositionFromTracker(positionItem.trackedPosition, hand) * positionItem.weighting / totalWeights;
            }

            Vector3 screenPos = virtualScreen.WorldPositionToVirtualScreen(worldPosM);

            // float distanceFromScreen (measured in meters)
            positions.DistanceFromScreen = screenPos.Z;

            // Vector2 position in screen-space (measured in pixels)
            positions.CursorPosition = new Vector2(screenPos.X, screenPos.Y);

            return positions;
        }

        public Positions ApplyStabiliation(Positions positions, IPositionStabiliser stabiliser)
        {
            Vector2 screenPosMm = virtualScreen.PixelsToMillimeters(positions.CursorPosition);
            screenPosMm = stabiliser.ApplyDeadzone(screenPosMm);
            positions.CursorPosition = virtualScreen.MillimetersToPixels(screenPosMm);

            return positions;
        }

        public Vector3 GetPositionFromTracker(TrackedPosition trackedPosition, Leap.Hand hand)
        {
            var trackerToUse = positionTrackers.Single(x => x.TrackedPosition == trackedPosition);
            return trackerToUse.GetTrackedPosition(hand);
        }
    }
}
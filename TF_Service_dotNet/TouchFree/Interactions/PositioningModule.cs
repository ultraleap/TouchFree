using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Ultraleap.TouchFree.Library.Interactions.PositionTrackers;

namespace Ultraleap.TouchFree.Library.Interactions
{
    public class PositioningModule : IPositioningModule
    {
        public TrackedPosition TrackedPosition { set { trackedPosition = value; UpdateTrackerToUse(); } }
        private TrackedPosition trackedPosition = TrackedPosition.INDEX_STABLE;

        private Positions positions;

        private IVirtualScreen virtualScreen;

        public IPositionStabiliser Stabiliser { get { return stabiliser; } }
        private readonly IPositionStabiliser stabiliser;

        private readonly IEnumerable<IPositionTracker> positionTrackers;

        public IPositionTracker TrackerToUse { get; private set; }
        private readonly IPositionTracker defaultTracker;

        public PositioningModule(IPositionStabiliser _stabiliser, IVirtualScreen _virtualScreen, IEnumerable<IPositionTracker> _positionTrackers)
        {
            stabiliser = _stabiliser;
            virtualScreen = _virtualScreen;
            positionTrackers = _positionTrackers;

            defaultTracker = positionTrackers.Single(x => x.GetType() == typeof(IndexStableTracker));

            UpdateTrackerToUse();

            Enable();
        }

        protected void Enable()
        {
            stabiliser.ResetValues();
        }

        public Positions CalculatePositions(Leap.Hand hand)
        {
            if (hand == null)
            {
                return positions;
            }

            Vector3 worldPos = TrackerToUse.GetTrackedPosition(hand);
            Vector3 screenPos = virtualScreen.WorldPositionToVirtualScreen(worldPos);
            Vector2 screenPosMm = virtualScreen.PixelsToMillimeters(new Vector2(screenPos.X, screenPos.Y));
            float distanceFromScreen = screenPos.Z;

            screenPosMm = stabiliser.ApplyDeadzone(screenPosMm);

            Vector2 oneToOnePosition = virtualScreen.MillimetersToPixels(screenPosMm);

            // float distanceFromScreen (measured in meters)
            positions.DistanceFromScreen = distanceFromScreen;

            // Vector2 position in screen-space (measured in pixels)
            positions.CursorPosition = oneToOnePosition;

            return positions;
        }

        private void UpdateTrackerToUse()
        {
            TrackerToUse = positionTrackers.SingleOrDefault(x => x.TrackedPosition == trackedPosition) ?? defaultTracker;
        }
    }
}
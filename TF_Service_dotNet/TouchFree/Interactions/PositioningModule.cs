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

        private IVirtualScreenManager virtualScreenManager;

        public IPositionStabiliser Stabiliser { get { return stabiliser; } }
        private readonly IPositionStabiliser stabiliser;

        private readonly IEnumerable<IPositionTracker> positionTrackers;

        public IPositionTracker TrackerToUse { get; private set; }
        private readonly IPositionTracker defaultTracker;

        public PositioningModule(IPositionStabiliser _stabiliser, IVirtualScreenManager _virtualScreenManager, IEnumerable<IPositionTracker> _positionTrackers)
        {
            stabiliser = _stabiliser;
            virtualScreenManager = _virtualScreenManager;
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
            Vector3 screenPos = virtualScreenManager.virtualScreen.WorldPositionToVirtualScreen(worldPos, out _);
            Vector2 screenPosM = virtualScreenManager.virtualScreen.PixelsToMeters(new Vector2(screenPos.X, screenPos.Y));
            float distanceFromScreen = screenPos.Z;

            screenPosM = stabiliser.ApplyDeadzone(screenPosM);

            Vector2 oneToOnePosition = virtualScreenManager.virtualScreen.MetersToPixels(screenPosM);

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
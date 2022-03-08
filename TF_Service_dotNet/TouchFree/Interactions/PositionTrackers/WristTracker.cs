using Leap;

namespace Ultraleap.TouchFree.Library.Interactions.PositionTrackers
{
    public class WristTracker : IPositionTracker
    {
        public TrackedPosition TrackedPosition => TrackedPosition.WRIST;

        public Vector3 GetTrackedPosition(Hand hand)
        {
            return Utilities.LeapVectorToNumerics(hand.WristPosition);
        }
    }
}

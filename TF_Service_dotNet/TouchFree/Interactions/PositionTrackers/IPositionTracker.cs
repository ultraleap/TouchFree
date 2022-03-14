using System.Numerics;

namespace Ultraleap.TouchFree.Library.Interactions.PositionTrackers
{
    public interface IPositionTracker
    {
        TrackedPosition TrackedPosition { get; }
        Vector3 GetTrackedPosition(Leap.Hand hand);
    }
}

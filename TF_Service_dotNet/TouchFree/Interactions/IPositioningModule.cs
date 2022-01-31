
using System.Numerics;

namespace Ultraleap.TouchFree.Library.Interactions
{
    public interface IPositioningModule
    {
        TrackedPosition TrackedPosition { set; }
        Positions CalculatePositions(Leap.Hand hand);
        Vector3 GetTrackedPointingJoint(Leap.Hand hand);
        IPositionStabiliser Stabiliser { get; }
    }
}

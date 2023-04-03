using Leap;
using System.Linq;
using System.Numerics;

namespace Ultraleap.TouchFree.Library.Interactions.PositionTrackers;

public class IndexStableTracker : IPositionTracker
{
    public TrackedPosition TrackedPosition => TrackedPosition.INDEX_STABLE;

    public Vector3 GetTrackedPosition(Hand hand)
    {
        const float trackedJointDistanceOffset = 0.0533f;

        var bones = hand.Fingers.First(finger => (finger.Type == Finger.FingerType.TYPE_INDEX)).bones;

        Vector3 trackedJointVector = (Utilities.LeapVectorToNumerics(bones[0].NextJoint) + Utilities.LeapVectorToNumerics(bones[1].NextJoint)) / 2;
        trackedJointVector.Z -= trackedJointDistanceOffset;
        return trackedJointVector;
    }
}
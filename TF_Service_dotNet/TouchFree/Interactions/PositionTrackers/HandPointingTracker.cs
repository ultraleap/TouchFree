using Leap;
using System.Linq;
using System.Numerics;

namespace Ultraleap.TouchFree.Library.Interactions.PositionTrackers
{
    public class HandPointingTracker : IPositionTracker
    {
        public TrackedPosition TrackedPosition => TrackedPosition.HAND_POINTING;

        public Vector3 GetTrackedPosition(Hand hand)
        {
            var fingerKnuckle = hand.Fingers.Single(x => x.Type == Leap.Finger.FingerType.TYPE_INDEX).bones.First().NextJoint;
            var palmDirection = Utilities.LeapVectorToNumerics(fingerKnuckle - hand.WristPosition);

            var palmNormal = Utilities.LeapVectorToNumerics(hand.PalmNormal) * 1000;
            var palmPosition = Utilities.LeapVectorToNumerics(hand.PalmPosition);
            var palmProjection = palmPosition - (palmNormal * (palmPosition.Z / palmNormal.Z));

            var palmDirectionPosition = Utilities.LeapVectorToNumerics(fingerKnuckle);
            var palmDirectionProjection = palmPosition - (palmDirection * (palmPosition.Z / palmDirection.Z));

            var normalisedPalmDirection = palmDirection / palmDirection.Length();
            var vectorProd = Vector3.Cross(palmNormal, normalisedPalmDirection);
            var combinedDirection = (palmNormal + (normalisedPalmDirection * 2.25f) + (vectorProd * 0.75f));
            var combinedDirectionProjection = palmPosition - (combinedDirection * (palmPosition.Z / combinedDirection.Z));


            return new Vector3(combinedDirectionProjection.X, combinedDirectionProjection.Y, palmDirectionPosition.Z);
        }
    }
}
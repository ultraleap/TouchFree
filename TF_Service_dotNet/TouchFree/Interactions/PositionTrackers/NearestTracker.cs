using System.Numerics;
using Leap;

namespace Ultraleap.TouchFree.Library.Interactions.PositionTrackers
{
    public class NearestTracker : IPositionTracker
    {
        public TrackedPosition TrackedPosition => TrackedPosition.NEAREST;

        Leap.Finger.FingerType lastUsedFingerType = Leap.Finger.FingerType.TYPE_UNKNOWN;
        Leap.Bone.BoneType lastUsedBoneType = Leap.Bone.BoneType.TYPE_INVALID;

        private const float NEAREST_BONE_BIAS = 0.01f;

        public Vector3 GetTrackedPosition(Hand hand)
        {
            float nearestDistance = float.PositiveInfinity;
            Vector3 nearestJointPos = Vector3.Zero;
            Leap.Finger.FingerType fingerType = Leap.Finger.FingerType.TYPE_UNKNOWN;
            Leap.Bone.BoneType boneType = Leap.Bone.BoneType.TYPE_INVALID;

            // Loop through all fingers and bones
            foreach (var finger in hand.Fingers)
            {
                foreach (var bone in finger.bones)
                {
                    Vector3 jointPos = Utilities.LeapVectorToNumerics(bone.NextJoint);
                    float screenDistance = jointPos.Z;

                    // add the bias for the previous finger bone position
                    if (finger.Type == lastUsedFingerType && bone.Type == lastUsedBoneType)
                    {
                        screenDistance -= NEAREST_BONE_BIAS;
                    }

                    if (nearestDistance > screenDistance)
                    {
                        // We are the nearest joint
                        nearestDistance = screenDistance;
                        nearestJointPos = jointPos;
                        fingerType = finger.Type;
                        boneType = bone.Type;
                    }
                }
            }

            lastUsedFingerType = fingerType;
            lastUsedBoneType = boneType;
            return nearestJointPos;
        }
    }
}

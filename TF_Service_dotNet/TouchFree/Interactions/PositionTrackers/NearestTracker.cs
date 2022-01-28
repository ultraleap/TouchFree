using System;
using System.Linq;
using System.Numerics;
using Leap;

namespace Ultraleap.TouchFree.Library.Interactions.PositionTrackers
{
    public class NearestTracker : IPositionTracker
    {
        public NearestTracker(IVirtualScreenManager _virtualScreenManager)
        {
            virtualScreenManager = _virtualScreenManager;
        }

        public TrackedPosition TrackedPosition => TrackedPosition.NEAREST;

        Leap.Finger.FingerType lastUsedFingerType = Leap.Finger.FingerType.TYPE_UNKNOWN;
        Leap.Bone.BoneType lastUsedBoneType = Leap.Bone.BoneType.TYPE_INVALID;
        private readonly IVirtualScreenManager virtualScreenManager;

        private const float NEAREST_BONE_BIAS = 0.01f;

        public Vector3 GetTrackedPosition(Hand hand)
        {
            float nearestDistance = float.PositiveInfinity;
            Vector3 nearestJointPos = Vector3.Zero;
            Leap.Finger.FingerType fingerType = Leap.Finger.FingerType.TYPE_UNKNOWN;
            Leap.Bone.BoneType boneType = Leap.Bone.BoneType.TYPE_INVALID;

            // check the last used finger hasn't changed position by a lot. If it hasn't, use it
            // by default with a bias
            foreach (var finger in hand.Fingers)
            {
                if (finger.Type == lastUsedFingerType)
                {
                    foreach (var bone in finger.bones)
                    {
                        if (bone.Type == lastUsedBoneType)
                        {
                            Vector3 jointPos = Utilities.LeapVectorToNumerics(bone.NextJoint);

                            nearestDistance = virtualScreenManager.virtualScreen.DistanceFromScreenPlane(jointPos) - NEAREST_BONE_BIAS; // add a bias to the previous finger tip position

                            nearestJointPos = jointPos;
                            fingerType = finger.Type;
                            boneType = bone.Type;

                            break;
                        }
                    }
                }
            }

            // Loop through all other fingers
            foreach (var finger in hand.Fingers)
            {
                foreach (var bone in finger.bones)
                {
                    Vector3 jointPos = Utilities.LeapVectorToNumerics(bone.NextJoint);
                    float screenDistance = virtualScreenManager.virtualScreen.DistanceFromScreenPlane(jointPos);

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

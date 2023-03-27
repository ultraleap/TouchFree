using Leap;
using System.Numerics;

namespace Ultraleap.TouchFree.Library.Interactions.PositionTrackers;

public class NearestTracker : IPositionTracker
{
    public TrackedPosition TrackedPosition => TrackedPosition.NEAREST;

    private Leap.Finger.FingerType _lastUsedFingerType = Leap.Finger.FingerType.TYPE_UNKNOWN;
    private Leap.Bone.BoneType _lastUsedBoneType = Leap.Bone.BoneType.TYPE_INVALID;

    private const float _nearestBoneBias = 0.01f;

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
            if (finger.Type == _lastUsedFingerType)
            {
                foreach (var bone in finger.bones)
                {
                    if (bone.Type == _lastUsedBoneType)
                    {
                        Vector3 jointPos = Utilities.LeapVectorToNumerics(bone.NextJoint);

                        nearestDistance = jointPos.Z - _nearestBoneBias; // add a bias to the previous finger tip position

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
                float screenDistance = jointPos.Z;

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

        _lastUsedFingerType = fingerType;
        _lastUsedBoneType = boneType;
        return nearestJointPos;
    }
}
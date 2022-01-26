using System;
using System.Numerics;
using System.Linq;

using Leap;


namespace Ultraleap.TouchFree.Library.Interactions
{
    public class PositioningModule : IPositioningModule
    {
        public TrackedPosition TrackedPosition { set { trackedPosition = value; } }
        private TrackedPosition trackedPosition = TrackedPosition.INDEX_STABLE;

        private Positions positions;

        private IVirtualScreenManager virtualScreenManager;

        private const float NEAREST_BONE_BIAS = 0.01f;

        public IPositionStabiliser Stabiliser { get { return stabiliser; } }
        private readonly IPositionStabiliser stabiliser;

        public PositioningModule(IPositionStabiliser _stabiliser, TrackedPosition _position, IVirtualScreenManager _virtualScreenManager)
        {
            stabiliser = _stabiliser;
            trackedPosition = _position;
            virtualScreenManager = _virtualScreenManager;

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

            Tuple<Vector2, float> oneToOneData = CalculateOneToOnePositionData(hand);
            Vector2 oneToOnePosition = oneToOneData.Item1;
            float distanceFromScreen = oneToOneData.Item2;

            positions.DistanceFromScreen = distanceFromScreen;

            positions.CursorPosition = oneToOnePosition;

            return positions;
        }

        private Tuple<Vector2, float> CalculateOneToOnePositionData(Leap.Hand hand)
        {
            // Return the hand position as a tuple:
            // Vector2 position in screen-space (measured in pixels)
            // float distanceFromScreen (measured in meters)

            Vector3 worldPos = GetTrackedPosition(hand);
            Vector3 screenPos = virtualScreenManager.virtualScreen.WorldPositionToVirtualScreen(worldPos, out _);
            Vector2 screenPosM = virtualScreenManager.virtualScreen.PixelsToMeters(new Vector2(screenPos.X, screenPos.Y));
            float distanceFromScreen = screenPos.Z;

            screenPosM = stabiliser.ApplyDeadzone(screenPosM);

            Vector2 oneToOnePosition = virtualScreenManager.virtualScreen.MetersToPixels(screenPosM);

            return new Tuple<Vector2, float>(oneToOnePosition, distanceFromScreen);
        }

        private Vector3 GetTrackedPosition(Leap.Hand hand)
        {
            switch (trackedPosition)
            {
                case TrackedPosition.WRIST:
                    return Utilities.LeapVectorToNumerics(hand.WristPosition);
                case TrackedPosition.INDEX_TIP:
                    var tipPos = hand.Fingers.First(finger => (finger.Type == Finger.FingerType.TYPE_INDEX)).TipPosition;
                    return Utilities.LeapVectorToNumerics(tipPos);
                case TrackedPosition.NEAREST:
                    return GetNearestBoneToScreen(hand);
                case TrackedPosition.INDEX_STABLE:
                default:
                    return GetTrackedPointingJoint(hand);
            }
        }

        public Vector3 GetTrackedPointingJoint(Leap.Hand hand)
        {
            const float trackedJointDistanceOffset = 0.0533f;

            var bones = hand.Fingers.First(finger => (finger.Type == Finger.FingerType.TYPE_INDEX)).bones;

            Vector3 trackedJointVector = (Utilities.LeapVectorToNumerics(bones[0].NextJoint) + Utilities.LeapVectorToNumerics(bones[1].NextJoint)) / 2;
            trackedJointVector.Z += trackedJointDistanceOffset;
            return trackedJointVector;
        }

        Leap.Finger.FingerType lastUsedFingerType = Leap.Finger.FingerType.TYPE_UNKNOWN;
        Leap.Bone.BoneType lastUsedBoneType = Leap.Bone.BoneType.TYPE_INVALID;

        Vector3 GetNearestBoneToScreen(Leap.Hand hand)
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
                    foreach(var bone in finger.bones)
                    {
                        if(bone.Type == lastUsedBoneType)
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
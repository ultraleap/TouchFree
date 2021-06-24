using System;
using System.Collections;
using System.Collections.Generic;
using Leap.Unity;
using UnityEngine;
using Ultraleap.TouchFree.ServiceShared;

namespace Ultraleap.TouchFree.Service
{
    public class PositioningModule : MonoBehaviour
    {
        public enum TRACKED_POSITION
        {
            INDEX_STABLE,
            INDEX_TIP,
            WRIST,
            NEAREST
        }

        public TRACKED_POSITION trackedPosition = TRACKED_POSITION.INDEX_STABLE;

        [Tooltip("If assigned, the cursor snapper and stabiliser will be accessed from the utils object.")]
        public GameObject positioningUtils;

        public PositionStabiliser Stabiliser;

        [NonSerialized]
        public bool ApplyDragLerp;

        private const float DRAG_SMOOTHING_FACTOR = 10f;
        private Positions positions;

        private const float NEAREST_BONE_BIAS = 0.01f;

        protected void OnEnable()
        {
            if (positioningUtils != null)
            {
                Stabiliser = positioningUtils.GetComponent<PositionStabiliser>();
            }

            Stabiliser.ResetValues();
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

            float velocity = hand.PalmVelocity.Magnitude;
            Vector3 worldPos = GetTrackedPosition(hand);
            float smoothingTime = Time.deltaTime;
            if (ApplyDragLerp)
            {
                // Apply a different smoothing time if dragging
                smoothingTime *= DRAG_SMOOTHING_FACTOR;
            }
            worldPos = Stabiliser.ApplySmoothing(worldPos, velocity, smoothingTime);

            Vector3 screenPos = ConfigManager.GlobalSettings.virtualScreen.WorldPositionToVirtualScreen(worldPos, out _);
            Vector2 screenPosM = ConfigManager.GlobalSettings.virtualScreen.PixelsToMeters(screenPos);
            float distanceFromScreen = screenPos.z;

            screenPosM = Stabiliser.ApplyDeadzone(screenPosM);

            Vector2 oneToOnePosition = ConfigManager.GlobalSettings.virtualScreen.MetersToPixels(screenPosM);

            return new Tuple<Vector2, float>(oneToOnePosition, distanceFromScreen);
        }

        private Vector3 GetTrackedPosition(Leap.Hand hand)
        {
            switch (trackedPosition)
            {
                case TRACKED_POSITION.WRIST:
                    return hand.WristPosition.ToVector3();
                case TRACKED_POSITION.INDEX_TIP:
                    return hand.GetIndex().TipPosition.ToVector3();
                case TRACKED_POSITION.NEAREST:
                    return GetNearestBoneToScreen(hand);
                case TRACKED_POSITION.INDEX_STABLE:
                default:
                    return GetTrackedPointingJoint(hand);
            }
        }

        public Vector3 GetTrackedPointingJoint(Leap.Hand hand)
        {
            const float trackedJointDistanceOffset = 0.0533f;

            var bones = hand.GetIndex().bones;

            Vector3 trackedJointVector = (bones[0].NextJoint.ToVector3() + bones[1].NextJoint.ToVector3()) / 2;
            trackedJointVector.z += trackedJointDistanceOffset;
            return trackedJointVector;
        }

        Leap.Finger.FingerType lastUsedFingerType = Leap.Finger.FingerType.TYPE_UNKNOWN;
        Leap.Bone.BoneType lastUsedBoneType = Leap.Bone.BoneType.TYPE_INVALID;

        Vector3 GetNearestBoneToScreen(Leap.Hand hand)
        {
            float nearestDistance = Mathf.Infinity;
            Vector3 nearestJointPos = Vector3.zero;
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
                            Vector3 jointPos = bone.NextJoint.ToVector3();

                            nearestDistance = ConfigManager.GlobalSettings.virtualScreen.DistanceFromScreenPlane(jointPos) - NEAREST_BONE_BIAS; // add a bias to the previous finger tip position

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
                    Vector3 jointPos = bone.NextJoint.ToVector3();
                    float screenDistance = ConfigManager.GlobalSettings.virtualScreen.DistanceFromScreenPlane(jointPos);

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
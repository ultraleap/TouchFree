using System;
using System.Collections;
using System.Collections.Generic;
using Leap.Unity;
using UnityEngine;

namespace Ultraleap.ScreenControl.Core
{
    public struct Positions
    {
        /**
         * Cursor position is used to guide the position of the cursor representation.
         * It is calculated in Screen Space
         */
        public Vector2 CursorPosition;

        /**
         * Distance from screen is the physical distance of the hand from the screen.
         * It is calculated in meters.
         */
        public float DistanceFromScreen;

        public Positions(Vector2 _cursorPosition, float _distanceFromScreen)
        {
            CursorPosition = _cursorPosition;
            DistanceFromScreen = _distanceFromScreen;
        }
    }

    public class PositioningModule : MonoBehaviour
    {
        public enum TRACKED_POSITION
        {
            FINGER,
            WRIST
        }

        public TRACKED_POSITION trackedPosition = TRACKED_POSITION.FINGER;

        [Tooltip("If assigned, the cursor snapper and stabiliser will be accessed from the utils object.")]
        public GameObject positioningUtils;

        public PositionStabiliser Stabiliser;

        [NonSerialized]
        public bool ApplyDragLerp;

        private const float DRAG_SMOOTHING_FACTOR = 10f;
        private Positions positions;

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
                case TRACKED_POSITION.FINGER:
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
    }
}
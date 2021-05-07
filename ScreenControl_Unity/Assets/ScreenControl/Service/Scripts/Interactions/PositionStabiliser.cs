using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ultraleap.ScreenControl.Core
{
    public class PositionStabiliser : MonoBehaviour
    {
        [HideInInspector] public float defaultDeadzoneRadius;
        public float smoothingRate;
        public float internalShrinkFactor = 2f;

        [Header("Progress based scaling")]
        public AnimationCurve deadzoneProgressScaling;

        // This is the radius that actually gets applied
        [HideInInspector] public float currentDeadzoneRadius;

        // Shrinking Params
        [HideInInspector] public bool isShrinking = false;
        private float shrinkingSpeed;

        private bool havePreviousPositionSmoothing;
        private Vector3 previousPositionSmoothing;

        private bool havePreviousPositionDeadzone;
        private Vector2 previousPositionDeadzoneDefaultSize;
        private Vector2 previousPositionDeadzoneCurrentSize;

        private void OnEnable()
        {
            ResetValues();
            InteractionConfig.OnConfigUpdated += OnSettingsUpdated;
            OnSettingsUpdated();
        }

        void OnDisable()
        {
            InteractionConfig.OnConfigUpdated -= OnSettingsUpdated;
        }

        // Apply smoothing
        public Vector3 ApplySmoothing(Vector3 position, float currentVelocity, float deltaTime)
        {
            if(defaultDeadzoneRadius == 0)
            {
                return position;
            }

            Vector3 smoothedPosition;

            if (!havePreviousPositionSmoothing)
            {
                havePreviousPositionSmoothing = true;
                smoothedPosition = position;
            }
            else
            {
                // When velocity is high, increase the lerp speed to reduce filtering effect
                float lerpRate = Mathf.Lerp(smoothingRate, 1f / deltaTime, currentVelocity);
                smoothedPosition = Vector3.Lerp(previousPositionSmoothing, position, lerpRate * deltaTime);
            }

            previousPositionSmoothing = smoothedPosition;
            return smoothedPosition;
        }

        public Vector2 ApplyDeadzone(Vector2 position)
        {
            if (defaultDeadzoneRadius == 0)
            {
                return position;
            }

            Vector2 constrainedPositionDefault;
            Vector2 constrainedPositionCurrent;

            if (!havePreviousPositionDeadzone)
            {
                havePreviousPositionDeadzone = true;
                constrainedPositionDefault = position;
                constrainedPositionCurrent = position;
            }
            else
            {
                // Calculate default deadzone
                constrainedPositionDefault = ApplyDeadzoneSized(previousPositionDeadzoneDefaultSize, position, defaultDeadzoneRadius);

                if (isShrinking)
                {
                    ShrinkDeadzone(constrainedPositionDefault);
                }

                // Calculate current deadzone
                constrainedPositionCurrent = ApplyDeadzoneSized(previousPositionDeadzoneCurrentSize, position, currentDeadzoneRadius);
            }

            previousPositionDeadzoneCurrentSize = constrainedPositionCurrent;
            previousPositionDeadzoneDefaultSize = constrainedPositionDefault;
            return constrainedPositionCurrent;
        }

        public static Vector2 ApplyDeadzoneSized(Vector2 previous, Vector2 current, float radius)
        {
            Vector2 constrainedPosition = Vector2.zero;
            float distance = Vector2.Distance(previous, current);
            if (distance > radius)
            {
                Vector2 unitVector = (previous - current).normalized;
                constrainedPosition = current + (unitVector * radius);
            }
            else
            {
                constrainedPosition = previous;
            }

            return constrainedPosition;
        }

        public void ResetValues()
        {
            havePreviousPositionDeadzone = false;
            shrinkingSpeed = 0;
            isShrinking = false;
            currentDeadzoneRadius = defaultDeadzoneRadius;
        }

        public void StartShrinkingDeadzone(float speed)
        {
            if (currentDeadzoneRadius == defaultDeadzoneRadius)
            {
                // If it's already shrunk, don't try and re-shrink
                return;
            }
            shrinkingSpeed = speed;
            isShrinking = true;
        }

        public void StopShrinkingDeadzone()
        {
            shrinkingSpeed = 0;
            isShrinking = false;
        }

        void ShrinkDeadzone(Vector2 constrainedPositionDefault)
        {
            Vector2 defaultPositionChange = (constrainedPositionDefault - previousPositionDeadzoneDefaultSize);
            Vector2 previousConstraintVector = (previousPositionDeadzoneDefaultSize - previousPositionDeadzoneCurrentSize);

            float internalConstraintVariable = previousConstraintVector.magnitude + internalShrinkFactor * defaultDeadzoneRadius;

            if (internalConstraintVariable < currentDeadzoneRadius)
            {
                currentDeadzoneRadius = internalConstraintVariable;
            }
            else if (previousConstraintVector != Vector2.zero)
            {
                float distanceAwayFromConstraint = Vector2.Dot(defaultPositionChange, previousConstraintVector) / previousConstraintVector.magnitude;
                distanceAwayFromConstraint = Mathf.Max(0, distanceAwayFromConstraint);

                float shrinkDistance = distanceAwayFromConstraint * shrinkingSpeed;
                currentDeadzoneRadius -= shrinkDistance;

                if (currentDeadzoneRadius < defaultDeadzoneRadius)
                {
                    currentDeadzoneRadius = defaultDeadzoneRadius;
                    StopShrinkingDeadzone();
                }
            }
        }

        public void ScaleDeadzoneByProgress(float _progressToClick)
        {
            // Assumes deadzoneProgressScaling runs from 0.0 to 1.0.
            var scaledValue = deadzoneProgressScaling.Evaluate(_progressToClick);
            var deadZoneRadius = defaultDeadzoneRadius * scaledValue;

            currentDeadzoneRadius = deadZoneRadius;
        }

        void OnSettingsUpdated()
        {
            defaultDeadzoneRadius = ConfigManager.InteractionConfig.DeadzoneRadius;
        }
    }
}
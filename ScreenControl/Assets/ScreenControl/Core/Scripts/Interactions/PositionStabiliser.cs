using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ultraleap.ScreenControl.Core
{
    public enum ShrinkType
    {
        NONE,
        TIME_BASED,
        MOTION_BASED,
    }

    public class PositionStabiliser : MonoBehaviour
    {
        public float defaultDeadzoneRadius;
        public float smoothingRate;
        public float characteristicVelocity;
        public float internalShrinkFactor = 2f;

        [Header("Distance based scaling")]
        public AnimationCurve deadzoneDistanceScaling;

        // This is the radius that actually gets applied
        private float currentDeadzoneRadius;

        // Shrinking Params
        private ShrinkType shrinkingType = ShrinkType.NONE;
        private float shrinkingSpeed;

        private bool havePreviousPositionSmoothing;
        private Vector3 previousPositionSmoothing;

        private bool havePreviousPositionDeadzone;
        private Vector2 previousPositionDeadzoneUnconstrained;
        private Vector2 previousPositionDeadzoneDefaultSize;
        private Vector2 previousPositionDeadzoneCurrentSize;

        private void OnEnable()
        {
            InteractionConfig.OnConfigUpdated += OnSettingsUpdated;
            OnSettingsUpdated();
        }

        void OnDisable()
        {
            InteractionConfig.OnConfigUpdated -= OnSettingsUpdated;
        }

        // Change the current Deadzone Radius. Does not affect the default.
        public void SetCurrentDeadzoneRadius(float radius)
        {
            currentDeadzoneRadius = radius;
        }

        // Get the current Deadzone Radius.
        public float GetCurrentDeadzoneRadius()
        {
            return currentDeadzoneRadius;
        }

        // Apply smoothing
        public Vector3 ApplySmoothing(Vector3 position, float currentVelocity, float deltaTime)
        {
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

        public Vector2 ApplyDeadzone(Vector2 position, float deltaTime = 0f)
        {
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

                if (shrinkingType == ShrinkType.MOTION_BASED)
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
                        float velocityFactor = 1f;

                        if ((deltaTime != 0f) && (characteristicVelocity != 0f))
                        {
                            // Calculate a velocity-dependent factor
                            float calcVel = defaultPositionChange.magnitude / deltaTime;
                            float calcFactor = calcVel / characteristicVelocity;
                            velocityFactor = Mathf.Min(calcFactor, 1f);
                        }

                        shrinkDistance *= velocityFactor;
                        currentDeadzoneRadius -= shrinkDistance;

                        if (currentDeadzoneRadius < defaultDeadzoneRadius)
                        {
                            currentDeadzoneRadius = defaultDeadzoneRadius;
                            StopShrinkingDeadzone();
                        }
                    }
                }

                // Calculate current deadzone
                constrainedPositionCurrent = ApplyDeadzoneSized(previousPositionDeadzoneCurrentSize, position, currentDeadzoneRadius);

            }
            previousPositionDeadzoneUnconstrained = position;
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

        public void Start()
        {
            ResetValues();
        }

        public void ResetValues()
        {
            havePreviousPositionDeadzone = false;
            shrinkingSpeed = 0;
            shrinkingType = ShrinkType.NONE;
            currentDeadzoneRadius = defaultDeadzoneRadius;
        }

        public bool IsShrinkingDeadzone()
        {
            return (shrinkingType != ShrinkType.NONE);
        }

        public void StartShrinkingDeadzone(ShrinkType setting, float speed)
        {
            if (currentDeadzoneRadius == defaultDeadzoneRadius)
            {
                // If it's already shrunk, don't try and re-shrink
                return;
            }
            shrinkingSpeed = speed;
            shrinkingType = setting;

            if (setting == ShrinkType.TIME_BASED)
            {
                // Immediately shrink the deadzone size to a smaller size, if we're using a time-based approach
                // This is so that we don't waste time shrinking with no effect
                float enforcedDeadzoneRadius = (previousPositionDeadzoneUnconstrained - previousPositionDeadzoneCurrentSize).magnitude;
                currentDeadzoneRadius = Mathf.Max(defaultDeadzoneRadius, enforcedDeadzoneRadius);
            }
        }

        public void StopShrinkingDeadzone()
        {
            shrinkingSpeed = 0;
            shrinkingType = ShrinkType.NONE;
        }

        public void ScaleDeadzoneByDistance(float screenDistance)
        {
            // Assumes deadZoneDistanceScaling runs from 1.0 to 0.0.
            // If screenDistance = DistanceBetweenHoverStartToButtonPressM then normalisedDistance = 0.0.
            // If screenDistance = 0.0 (at touch plane) then normalisedDistance = 1.0.
            // Therefore deadZoneRadius increases in size the closer to the screen it gets, starting to grow at 0.1f.
            var normalisedDistance = deadzoneDistanceScaling.Evaluate(screenDistance / 0.1f);
            var deadZoneRadius = defaultDeadzoneRadius * normalisedDistance;

            SetCurrentDeadzoneRadius(deadZoneRadius);
        }

        private void Update()
        {
            if (shrinkingType == ShrinkType.TIME_BASED)
            {
                float newDeadzoneRadius = currentDeadzoneRadius - shrinkingSpeed * Time.deltaTime;
                if (newDeadzoneRadius < defaultDeadzoneRadius || shrinkingSpeed == 0)
                {
                    currentDeadzoneRadius = defaultDeadzoneRadius;
                    StopShrinkingDeadzone();
                }
                else
                {
                    currentDeadzoneRadius = newDeadzoneRadius;
                }
            }
        }

        void OnSettingsUpdated()
        {
            defaultDeadzoneRadius = ConfigManager.InteractionConfig.DeadzoneRadius;
        }
    }
}
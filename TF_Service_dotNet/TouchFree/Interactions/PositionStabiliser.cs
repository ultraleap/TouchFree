using System;
using System.Numerics;
using Ultraleap.TouchFree.Library.Configuration;

namespace Ultraleap.TouchFree.Library.Interactions
{
    public class PositionStabiliser : IPositionStabiliser
    {
        public float defaultDeadzoneRadius { get; set; }
        public float internalShrinkFactor { get; set; }

        // This is the radius that actually gets applied
        public float currentDeadzoneRadius { get; set; }

        // Shrinking Params
        public bool isShrinking { get; set; }
        private float shrinkingSpeed;

        private bool havePreviousPositionDeadzone;
        private Vector2 previousPositionDeadzoneDefaultSize;
        private Vector2 previousPositionDeadzoneCurrentSize;

        private Vector2 deadzoneOffset;
        Vector2 lastRawPos;

        public void SetDeadzoneOffset()
        {
            if (defaultDeadzoneRadius > 0)
            {
                deadzoneOffset = previousPositionDeadzoneCurrentSize - lastRawPos;
            }
        }

        public void ReduceDeadzoneOffset()
        {
            deadzoneOffset *= 0.9f;
        }

        public PositionStabiliser(IConfigManager _configManager)
        {
            _configManager.OnInteractionConfigUpdated += OnSettingsUpdated;
            OnSettingsUpdated(_configManager.InteractionConfig);
            ResetValues();
        }

        public Vector2 ApplyDeadzone(Vector2 position)
        {
            lastRawPos = position;
            position += deadzoneOffset;

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

        public Vector2 ApplyDeadzoneSized(Vector2 previous, Vector2 current, float radius)
        {
            Vector2 constrainedPosition;

            float distance = Vector2.Distance(previous, current);
            if (distance > radius)
            {
                Vector2 unitVector = Vector2.Normalize(previous - current);
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

            if (previousConstraintVector != Vector2.Zero)
            {
                float distanceAwayFromConstraint = Vector2.Dot(defaultPositionChange, previousConstraintVector) / previousConstraintVector.Length();
                distanceAwayFromConstraint = Math.Max(0, distanceAwayFromConstraint);

                float shrinkDistance = distanceAwayFromConstraint * shrinkingSpeed;
                currentDeadzoneRadius -= shrinkDistance;

                if (currentDeadzoneRadius < defaultDeadzoneRadius)
                {
                    currentDeadzoneRadius = defaultDeadzoneRadius;
                    StopShrinkingDeadzone();
                }
            }
        }

        public void ScaleDeadzoneByProgress(float _progressToClick, float _maxDeadzoneIncrease)
        {
            // Assumes _progressToClick is clamped.
            var scaledValue = _progressToClick * _progressToClick;
            var deadZoneRadius = Utilities.Lerp(defaultDeadzoneRadius, defaultDeadzoneRadius + _maxDeadzoneIncrease, scaledValue);

            currentDeadzoneRadius = deadZoneRadius;
        }

        void OnSettingsUpdated(InteractionConfig _config)
        {
            defaultDeadzoneRadius = _config.DeadzoneRadius;
        }
    }
}
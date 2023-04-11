using System;
using System.Numerics;
using Ultraleap.TouchFree.Library.Configuration;

namespace Ultraleap.TouchFree.Library.Interactions;

public class PositionStabiliser : IPositionStabiliser
{
    public float DefaultDeadzoneRadius { get; set; }
    public float InternalShrinkFactor { get; set; }

    // This is the radius that actually gets applied
    public float CurrentDeadzoneRadius { get; set; }

    // Shrinking Params
    public bool IsShrinking { get; set; }
    
    private float _shrinkingSpeed;

    private bool _havePreviousPositionDeadzone;
    private Vector2 _previousPositionDeadzoneDefaultSize;
    private Vector2 _previousPositionDeadzoneCurrentSize;

    private Vector2 _deadzoneOffset;
    private Vector2 _lastRawPos;

    public void SetDeadzoneOffset()
    {
        if (DefaultDeadzoneRadius > 0)
        {
            _deadzoneOffset = _previousPositionDeadzoneCurrentSize - _lastRawPos;
        }
    }

    public void ReduceDeadzoneOffset()
    {
        _deadzoneOffset *= 0.9f;
    }

    public PositionStabiliser(IConfigManager configManager)
    {
        configManager.OnInteractionConfigUpdated += OnSettingsUpdated;
        OnSettingsUpdated(configManager.InteractionConfig);
        ResetValues();
    }

    public Vector2 ApplyDeadzone(Vector2 position)
    {
        _lastRawPos = position;
        position += _deadzoneOffset;

        if (DefaultDeadzoneRadius == 0)
        {
            return position;
        }

        Vector2 constrainedPositionDefault;
        Vector2 constrainedPositionCurrent;

        if (!_havePreviousPositionDeadzone)
        {
            _havePreviousPositionDeadzone = true;
            constrainedPositionDefault = position;
            constrainedPositionCurrent = position;
        }
        else
        {
            // Calculate default deadzone
            constrainedPositionDefault = ApplyDeadzoneSized(_previousPositionDeadzoneDefaultSize, position, DefaultDeadzoneRadius);

            if (IsShrinking)
            {
                ShrinkDeadzone(constrainedPositionDefault);
            }

            // Calculate current deadzone
            constrainedPositionCurrent = ApplyDeadzoneSized(_previousPositionDeadzoneCurrentSize, position, CurrentDeadzoneRadius);
        }

        _previousPositionDeadzoneCurrentSize = constrainedPositionCurrent;
        _previousPositionDeadzoneDefaultSize = constrainedPositionDefault;
        return constrainedPositionCurrent;
    }

    public Vector2 ApplyDeadzoneSized(Vector2 previous, Vector2 current, float radiusMm)
    {
        Vector2 constrainedPosition;

        float distance = Vector2.Distance(previous, current);
        if (distance > radiusMm)
        {
            Vector2 unitVector = Vector2.Normalize(previous - current);
            constrainedPosition = current + (unitVector * radiusMm);
        }
        else
        {
            constrainedPosition = previous;
        }

        return constrainedPosition;
    }

    public void ResetValues()
    {
        _havePreviousPositionDeadzone = false;
        _shrinkingSpeed = 0;
        IsShrinking = false;
        CurrentDeadzoneRadius = DefaultDeadzoneRadius;
    }

    public void StartShrinkingDeadzone(float speed)
    {
        if (CurrentDeadzoneRadius == DefaultDeadzoneRadius)
        {
            // If it's already shrunk, don't try and re-shrink
            return;
        }
        _shrinkingSpeed = speed;
        IsShrinking = true;
    }

    public void StopShrinkingDeadzone()
    {
        _shrinkingSpeed = 0;
        IsShrinking = false;
    }

    private void ShrinkDeadzone(Vector2 constrainedPositionDefault)
    {
        Vector2 defaultPositionChange = (constrainedPositionDefault - _previousPositionDeadzoneDefaultSize);
        Vector2 previousConstraintVector = (_previousPositionDeadzoneDefaultSize - _previousPositionDeadzoneCurrentSize);

        if (previousConstraintVector != Vector2.Zero)
        {
            float distanceAwayFromConstraint = Vector2.Dot(defaultPositionChange, previousConstraintVector) / previousConstraintVector.Length();
            distanceAwayFromConstraint = Math.Max(0, distanceAwayFromConstraint);

            float shrinkDistance = distanceAwayFromConstraint * _shrinkingSpeed;
            CurrentDeadzoneRadius -= shrinkDistance;

            if (CurrentDeadzoneRadius < DefaultDeadzoneRadius)
            {
                CurrentDeadzoneRadius = DefaultDeadzoneRadius;
                StopShrinkingDeadzone();
            }
        }
    }

    public void ScaleDeadzoneByProgress(float progressToClick, float maxDeadzoneIncrease)
    {
        // Assumes progressToClick is clamped.
        var scaledValue = progressToClick * progressToClick;
        var deadZoneRadius = Utilities.Lerp(DefaultDeadzoneRadius, DefaultDeadzoneRadius + maxDeadzoneIncrease, scaledValue);

        CurrentDeadzoneRadius = deadZoneRadius;
    }

    private void OnSettingsUpdated(InteractionConfigInternal config)
    {
        DefaultDeadzoneRadius = config.DeadzoneRadiusMm;
    }
}
﻿using System.Numerics;

namespace Ultraleap.TouchFree.Library.Interactions;

public interface IPositionStabiliser
{
    Vector2 ApplyDeadzone(Vector2 position);
    void ResetValues();
    void StartShrinkingDeadzone(float speed);
    void StopShrinkingDeadzone();
    Vector2 ApplyDeadzoneSized(Vector2 previous, Vector2 current, float radius);
    void SetDeadzoneOffset();
    void ReduceDeadzoneOffset();
    void ScaleDeadzoneByProgress(float progressToClick, float maxDeadzoneIncrease);

    float DefaultDeadzoneRadius { get; set; }
    float CurrentDeadzoneRadius { get; set; }
}
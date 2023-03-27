using System;

namespace Ultraleap.TouchFree.Library.Interactions;

public class ProgressTimer
{
    public float TimeLimit { get; set; }

    private long? _startMilliseconds;

    public ProgressTimer(float timeLimit) => TimeLimit = timeLimit;
    public bool IsRunning => _startMilliseconds.HasValue;
    public void Restart(long timestamp) => _startMilliseconds = ConvertTimestamp(timestamp);

    public void Stop() => _startMilliseconds = null;
    public bool HasBeenRunningForThreshold(long currentTimestamp, double threshold) =>
        IsRunning
        && ConvertTimestamp(currentTimestamp) - _startMilliseconds >= threshold;

    public float GetProgress(long currentTimestamp)
    {
        if (!IsRunning)
        {
            return 0.0f;
        }

        float progress = (ConvertTimestamp(currentTimestamp) - _startMilliseconds!.Value) / TimeLimit;
        progress = Math.Min(progress, 1f);
        return progress;
    }
    
    private static long ConvertTimestamp(long timestamp) => timestamp / 1000;
}
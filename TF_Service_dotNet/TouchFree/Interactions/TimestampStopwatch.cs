namespace Ultraleap.TouchFree.Library.Interactions;

public class TimestampStopwatch
{
    private long? _startMilliseconds;
    public bool IsRunning => _startMilliseconds.HasValue;
    public void Restart(long timestamp) => _startMilliseconds = timestamp / 1000;
    public void Stop() => _startMilliseconds = null;
    public bool HasBeenRunningForThreshold(long currentTimestamp, double threshold) => IsRunning && currentTimestamp / 1000 - _startMilliseconds >= threshold;
}
namespace Ultraleap.TouchFree.Library.Interactions
{
    public class TimestampStopwatch
    {
        private long? startMilliseconds;

        public void Restart(long timestamp)
        {
            startMilliseconds = timestamp / 1000;
        }

        public bool IsRunning
        {
            get
            {
                return startMilliseconds.HasValue;
            }
        }

        public bool HasBeenRunningForThreshold(long currentTimestamp, double threshold)
        {
            return IsRunning && currentTimestamp / 1000 - startMilliseconds >= threshold;
        }

        public void Stop()
        {
            startMilliseconds = null;
        }
    }
}

using System;

namespace Ultraleap.TouchFree.Library.Interactions
{
    public class ProgressTimer
    {
        public float timeLimit = 500f;

        private long? startMilliseconds;

        public void Restart(long timestamp)
        {
            startMilliseconds = ConvertTimestamp(timestamp);
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
            return IsRunning && ConvertTimestamp(currentTimestamp) - startMilliseconds >= threshold;
        }

        public void Stop()
        {
            startMilliseconds = null;
        }

        public ProgressTimer(float _timeLimit)
        {
            timeLimit = _timeLimit;
        }

        private long ConvertTimestamp(long timestamp)
        {
            return timestamp / 1000;
        }

        public float GetProgress(long currentTimestamp)
        {
            if (!IsRunning)
            {
                return 0.0f;
            }

            float progress = (ConvertTimestamp(currentTimestamp) - startMilliseconds.Value) / timeLimit;
            progress = Math.Min(progress, 1f);
            return progress;
        }
    }
}
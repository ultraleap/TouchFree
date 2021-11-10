using System;

using Stopwatch = System.Diagnostics.Stopwatch;

namespace Ultraleap.TouchFree.Library.Interactions
{
    public class ProgressTimer
    {
        public float timeLimit = 500f;

        private Stopwatch stopwatch = new Stopwatch();

        public ProgressTimer(float _timeLimit)
        {
            timeLimit = _timeLimit;
        }

        public bool IsRunning
        {
            get { return stopwatch.IsRunning; }
        }

        public float Progress
        {
            get { return GetProgress(); }
        }

        private float GetProgress()
        {
            float progress = stopwatch.ElapsedMilliseconds / timeLimit;
            progress = Math.Min(progress, 1f);
            return progress;
        }

        public void StartTimer()
        {
            stopwatch.Restart();
        }

        public void StopTimer()
        {
            stopwatch.Stop();
        }

        public void ResetTimer()
        {
            stopwatch.Reset();
        }
    }
}
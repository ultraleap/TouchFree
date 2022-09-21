using System;
using System.Timers;

namespace Ultraleap.TouchFree.Service
{
    public class UpdateBehaviour : IDisposable
    {
        public delegate void UpdateEvent();
        public event UpdateEvent OnUpdate;

        private Timer updateLoop;
        private const float TargetFPS = 60f;
        
        public UpdateBehaviour(float framerate = TargetFPS)
        {
            SetTimer(framerate);
        }

        private void SetTimer(float framerate)
        {
            updateLoop = new(1000f / framerate);
            updateLoop.AutoReset = false;
            updateLoop.Enabled = true;
            updateLoop.Elapsed += OnTimerElapsed;
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            updateLoop.Start();
            OnUpdate?.Invoke();
        }

        public Timer UpdateTimer()
        {
            return updateLoop;
        }

        public void Dispose()
        {
            updateLoop?.Dispose();
        }
    }
}

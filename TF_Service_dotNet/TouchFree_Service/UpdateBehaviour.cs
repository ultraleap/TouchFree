using System;
using System.Timers;

namespace Ultraleap.TouchFree.Service
{
    public class UpdateBehaviour : IDisposable
    {
        public delegate void UpdateEvent();
        public event UpdateEvent OnUpdate
        {
            add { onUpdate -= value; onUpdate += value; }
            remove => onUpdate -= value;
        }

        private event UpdateEvent onUpdate;

        private Timer updateLoop;
        private const float TargetFPS = 60f;
        
        public UpdateBehaviour(float framerate = TargetFPS)
        {
            SetTimer(framerate);
        }

        private void SetTimer(float framerate)
        {
            updateLoop = new(1000f / framerate);
            updateLoop.AutoReset = true;
            updateLoop.Enabled = true;
            updateLoop.Elapsed += OnTimerElapsed;
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            onUpdate?.Invoke();
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

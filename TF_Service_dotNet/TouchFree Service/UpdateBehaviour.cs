using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Ultraleap.TouchFree.Service
{
    public class UpdateBehaviour
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
            updateLoop.AutoReset = true;
            updateLoop.Enabled = true;
            updateLoop.Elapsed += OnTimerElapsed;
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            OnUpdate?.Invoke();
        }

        public Timer UpdateTimer()
        {
            return updateLoop;
        }
    }
}

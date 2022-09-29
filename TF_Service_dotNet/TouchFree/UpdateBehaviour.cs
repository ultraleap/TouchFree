using System;
using System.Timers;
using static Ultraleap.TouchFree.Library.IUpdateBehaviour;

namespace Ultraleap.TouchFree.Library
{
    public class UpdateBehaviour : IDisposable, IUpdateBehaviour
    {
        public event UpdateEvent OnUpdate
        {
            add { onUpdate -= value; onUpdate += value; }
            remove => onUpdate -= value;
        }

        private event UpdateEvent onUpdate;


        public event UpdateEvent OnSlowUpdate
        {
            add { onSlowUpdate -= value; onSlowUpdate += value; }
            remove => onSlowUpdate -= value;
        }

        private event UpdateEvent onSlowUpdate;

        private Timer updateLoop;
        private const float TargetFPS = 60f;

        private const int slowUpdateCount = 4;
        private int slowUpdateIteration = 0;

        private readonly object invokeLock = new object();

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
            if (System.Threading.Monitor.TryEnter(invokeLock))
            {
                try
                {
                    onUpdate?.Invoke();
                    slowUpdateIteration++;
                    if (slowUpdateIteration == slowUpdateCount)
                    {
                        slowUpdateIteration = 0;
                        onSlowUpdate?.Invoke();
                    }
                }
                finally
                {
                    System.Threading.Monitor.Exit(invokeLock);
                }
            }
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

using System;
using System.Timers;
using static Ultraleap.TouchFree.Library.IUpdateBehaviour;

namespace Ultraleap.TouchFree.Library;

public class UpdateBehaviour : IDisposable, IUpdateBehaviour
{
    /// <summary>
    /// Update event. Does not allow multiple subscriptions of the same function.
    /// </summary>
    public event UpdateEvent OnUpdate
    {
        add { _onUpdate -= value; _onUpdate += value; }
        remove => _onUpdate -= value;
    }

    private UpdateEvent _onUpdate;
    
    /// <summary>
    /// Slow update event. Does not allow multiple subscriptions of the same function.
    /// </summary>
    public event UpdateEvent OnSlowUpdate
    {
        add { _onSlowUpdate -= value; _onSlowUpdate += value; }
        remove => _onSlowUpdate -= value;
    }

    private UpdateEvent _onSlowUpdate;

    private Timer _updateLoop;
    private const float _targetFps = 60f;

    private const int _slowUpdateCount = 5;
    private int _slowUpdateIteration = 0;

    private readonly object _invokeLock = new();

    public UpdateBehaviour(float framerate = _targetFps) => SetTimer(framerate);

    private void SetTimer(float framerate)
    {
        _updateLoop = new Timer(1000f / framerate)
        {
            AutoReset = true,
            Enabled = true
        };
        _updateLoop.Elapsed += OnTimerElapsed;
    }

    private void OnTimerElapsed(object sender, ElapsedEventArgs e)
    {
        if (System.Threading.Monitor.TryEnter(_invokeLock))
        {
            try
            {
                _onUpdate?.Invoke();
                _slowUpdateIteration++;
                if (_slowUpdateIteration >= _slowUpdateCount)
                {
                    _slowUpdateIteration = 0;
                    _onSlowUpdate?.Invoke();
                }
            }
            finally
            {
                System.Threading.Monitor.Exit(_invokeLock);
            }
        }
    }

    public void Dispose()
    {
        _updateLoop?.Dispose();
        GC.SuppressFinalize(this);
    }
}
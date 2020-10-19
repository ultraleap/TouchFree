using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

public class ProgressTimer : MonoBehaviour
{
    public float timeLimit = 500f;

    private Stopwatch stopwatch = new Stopwatch();
    
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
        progress = Mathf.Min(progress, 1f);
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
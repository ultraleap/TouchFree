using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ultraleap.TouchFree.Tooling;
using UnityEngine.Video;
using System.Diagnostics;

public class MiniCTI : MonoBehaviour
{
    public GameObject miniCTI;
    public VideoPlayer player;

    public float newUserTimeS = 10;
    public float forceVideoTimeS = 20;

    bool forceVideo = true;

    Stopwatch newUserStopwatch = new Stopwatch();
    Stopwatch forceVideoStopwatch = new Stopwatch();

    private void Awake()
    {
        InputActionManager.TransmitInputAction += HandleInputAction;

        currentCTIWait = StartCoroutine(ShowMiniCTIAfterWait());

        player.url = Application.streamingAssetsPath + "/Mini CTI.mp4";

        Ultraleap.TouchFree.Tooling.Connection.ConnectionManager.HandFound += HandFound;
        Ultraleap.TouchFree.Tooling.Connection.ConnectionManager.HandsLost += HandLost;

        forceVideoStopwatch.Start();
    }

    private void HandleInputAction(InputAction _inputData)
    {
        if(_inputData.InputType == InputType.DOWN)
        {
            if (!forceVideo || (forceVideo && forceVideoStopwatch.ElapsedMilliseconds > forceVideoTimeS * 1000))
            {
                forceVideo = false;
                miniCTI.SetActive(false);

                if (currentCTIWait != null)
                {
                    StopCoroutine(currentCTIWait);
                    currentCTIWait = null;
                }
            }
        }

        if(_inputData.InputType == InputType.UP)
        {   
            if(currentCTIWait != null)
            {
                StopCoroutine(currentCTIWait);
            }

            currentCTIWait = StartCoroutine(ShowMiniCTIAfterWait());
        }
    }

    Coroutine currentCTIWait = null;

    IEnumerator ShowMiniCTIAfterWait()
    {
        yield return new WaitForSeconds(4);

        miniCTI.SetActive(true);
        currentCTIWait = null;
    }

    private void HandLost()
    {
        newUserStopwatch.Restart();
    }

    private void HandFound()
    {
        if(newUserStopwatch.ElapsedMilliseconds > newUserTimeS * 1000)
        {
            // new user
            miniCTI.SetActive(true);
            forceVideo = true;
            forceVideoStopwatch.Restart();

            if (currentCTIWait != null)
            {
                StopCoroutine(currentCTIWait);
                currentCTIWait = null;
            }
        }

        newUserStopwatch.Stop();
    }
}
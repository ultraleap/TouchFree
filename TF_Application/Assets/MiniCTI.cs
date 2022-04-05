using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ultraleap.TouchFree.Tooling;
using UnityEngine.Video;

public class MiniCTI : MonoBehaviour
{
    public GameObject miniCTI;
    public VideoPlayer player;

    private void Awake()
    {
        InputActionManager.TransmitInputAction += HandleInputAction;
        miniCTI.SetActive(false);

        currentCTIWait = StartCoroutine(ShowMiniCTIAfterWait());

        player.url = Application.streamingAssetsPath + "/Mini CTI.mp4";
    }

    private void HandleInputAction(InputAction _inputData)
    {
        if(_inputData.InputType == InputType.DOWN)
        {
            miniCTI.SetActive(false);

            if (currentCTIWait != null)
            {
                StopCoroutine(currentCTIWait);
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
        yield return new WaitForSeconds(5);

        miniCTI.SetActive(true);
        currentCTIWait = null;
    }
}
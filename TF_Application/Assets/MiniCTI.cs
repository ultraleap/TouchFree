using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ultraleap.TouchFree.Tooling;

public class MiniCTI : MonoBehaviour
{
    public GameObject miniCTI;

    private void Awake()
    {
        InputActionManager.TransmitInputAction += HandleInputAction;
        miniCTI.SetActive(false);

        currentCTIWait = StartCoroutine(ShowMiniCTIAfterWait());
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
        yield return new WaitForSeconds(6);

        miniCTI.SetActive(true);
        currentCTIWait = null;
    }
}
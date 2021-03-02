using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableOnFocusLoss : MonoBehaviour
{
    public GameObject targetObj;

    void OnApplicationFocus(bool hasFocus)
    {
        targetObj.SetActive(hasFocus);
    }
}

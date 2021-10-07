using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraPreviewScreen : MonoBehaviour
{
    public Toggle enableOverexposureHighlighting;
    public Material leftCameraMat;
    public Material rightCameraMat;

    void OnEnable()
    {
        enableOverexposureHighlighting.onValueChanged.AddListener(OnOverExposureValueChanged);
        OnOverExposureValueChanged(enableOverexposureHighlighting.isOn);
    }

    void OnDisable()
    {
        enableOverexposureHighlighting.onValueChanged.RemoveListener(OnOverExposureValueChanged);
    }

    void OnOverExposureValueChanged(bool state)
    {
        if (state) {
            leftCameraMat.SetFloat("_threshold", 0.5f);
            rightCameraMat.SetFloat("_threshold", 0.5f);
        } else {
            leftCameraMat.SetFloat("_threshold", 1.0f);
            rightCameraMat.SetFloat("_threshold", 1.0f);
        }
    }
}

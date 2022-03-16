using System;
using UnityEngine;
using UnityEngine.UI;

public class CameraPreviewScreen : MonoBehaviour
{
    public Toggle enableOverexposureHighlighting;
    public Material leftCameraMat;
    public Material rightCameraMat;

    [SerializeField]
    private float exposureThresholdValue = 0.5f;

    public Slider maskingSiderL, maskingSiderR, maskingSiderT, maskingSiderB;

    [Tooltip("The ratio of the slider length to the masking capacity. Total capacity is the full width/height of the camera image.")]
    public float sliderRatio = 0.5f;

    float currentMaskLeft, currentMaskRight, currentMaskTop, currentMaskBottom;
    bool newMaskDataReceived = false;

    public GameObject maskingDiabledWarningObject;

    public GameObject handsCameraObject;

    void OnEnable()
    {
        handsCameraObject.SetActive(true);
        enableOverexposureHighlighting.onValueChanged.AddListener(OnOverExposureValueChanged);
        OnOverExposureValueChanged(enableOverexposureHighlighting.isOn);

        if(DiagnosticAPIManager.diagnosticAPI == null)
        {
            DiagnosticAPIManager.diagnosticAPI = new DiagnosticAPI(this);
        }

        DiagnosticAPI.OnGetMaskingResponse += SetSliders;
        DiagnosticAPI.OnMaskingVersionCheck += HandleMaskingVersionCheck;

        maskingSiderL.onValueChanged.AddListener(OnSliderChanged);
        maskingSiderR.onValueChanged.AddListener(OnSliderChanged);
        maskingSiderT.onValueChanged.AddListener(OnSliderChanged);
        maskingSiderB.onValueChanged.AddListener(OnSliderChanged);

        DiagnosticAPIManager.diagnosticAPI.GetImageMask();

        HandleMaskingVersionCheck(DiagnosticAPIManager.maskingAvailable);
    }

    void OnDisable()
    {
        handsCameraObject.SetActive(false);
        enableOverexposureHighlighting.onValueChanged.RemoveListener(OnOverExposureValueChanged);
        DiagnosticAPI.OnGetMaskingResponse -= SetSliders;
        DiagnosticAPI.OnMaskingVersionCheck -= HandleMaskingVersionCheck;

        maskingSiderL.onValueChanged.RemoveListener(OnSliderChanged);
        maskingSiderR.onValueChanged.RemoveListener(OnSliderChanged);
        maskingSiderT.onValueChanged.RemoveListener(OnSliderChanged);
        maskingSiderB.onValueChanged.RemoveListener(OnSliderChanged);
    }

    private void Update()
    {
        if(newMaskDataReceived)
        {
            SetSliders(currentMaskLeft, currentMaskRight, currentMaskTop, currentMaskBottom);
            newMaskDataReceived = false;
        }
    }

    public void HandleMaskingVersionCheck(bool _maskingAvailable)
    {
        maskingSiderL.gameObject.SetActive(_maskingAvailable);
        maskingSiderR.gameObject.SetActive(_maskingAvailable);
        maskingSiderT.gameObject.SetActive(_maskingAvailable);
        maskingSiderB.gameObject.SetActive(_maskingAvailable);
        maskingDiabledWarningObject.SetActive(!_maskingAvailable);
    }

    void OnOverExposureValueChanged(bool state)
    {
        if (state)
        {
            leftCameraMat.SetFloat("_threshold", exposureThresholdValue);
            rightCameraMat.SetFloat("_threshold", exposureThresholdValue);
        }
        else
        {
            leftCameraMat.SetFloat("_threshold", 1.0f);
            rightCameraMat.SetFloat("_threshold", 1.0f);
        }
    }

    void SetMasking()
    {
        currentMaskLeft = maskingSiderL.value * sliderRatio;
        currentMaskRight = maskingSiderR.value * sliderRatio;
        currentMaskTop = maskingSiderB.value * sliderRatio;
        currentMaskBottom = maskingSiderT.value * sliderRatio;

        DiagnosticAPIManager.diagnosticAPI.SetMasking(
            currentMaskLeft,
            currentMaskRight,
            currentMaskTop,
            currentMaskBottom);
    }

    public void SetSliders(float _left, float _right, float _top, float _bottom)
    {
        maskingSiderL.SetValueWithoutNotify(_left / sliderRatio);
        maskingSiderR.SetValueWithoutNotify(_right / sliderRatio);
        maskingSiderT.SetValueWithoutNotify(_bottom / sliderRatio);// These are reversed as the shader for rendering camera feeds is upside-down
        maskingSiderB.SetValueWithoutNotify(_top / sliderRatio);// These are reversed as the shader for rendering camera feeds is upside-down
    }

    public void OnSliderChanged(float _)
    {
        SetMasking();
    }
}
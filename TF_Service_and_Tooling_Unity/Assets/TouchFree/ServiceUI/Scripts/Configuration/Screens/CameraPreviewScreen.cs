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

    DiagnosticAPI diagnosticAPI;

    public Slider maskingSiderL, maskingSiderR, maskingSiderT, maskingSiderB;

    [Tooltip("The ratio of the slider length to the masking capacity. Total capacity is the full width/height of the camera image.")]
    public float sliderRatio = 0.5f;

    DiagnosticAPI.ImageMaskData currentMaskData;
    bool newMaskDataReceived = false;

    public GameObject maskingDiabledWarningObject;

    void OnEnable()
    {
        enableOverexposureHighlighting.onValueChanged.AddListener(OnOverExposureValueChanged);
        OnOverExposureValueChanged(enableOverexposureHighlighting.isOn);

        if(diagnosticAPI == null)
        {
            diagnosticAPI = new DiagnosticAPI(this);
        }

        DiagnosticAPI.OnGetMaskingResponse += SetSliders;
        DiagnosticAPI.OnMaskingVersionCheck += HandleMaskingVersionCheck;

        maskingSiderL.onValueChanged.AddListener(OnSliderChanged);
        maskingSiderR.onValueChanged.AddListener(OnSliderChanged);
        maskingSiderT.onValueChanged.AddListener(OnSliderChanged);
        maskingSiderB.onValueChanged.AddListener(OnSliderChanged);

        diagnosticAPI.Request("GetDevices"); // get the connected device ID
        diagnosticAPI.Request("GetVersion");
    }

    void OnDisable()
    {
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
            maskingSiderL.SetValueWithoutNotify((float)currentMaskData.left / sliderRatio);
            maskingSiderR.SetValueWithoutNotify((float)currentMaskData.right / sliderRatio);
            maskingSiderT.SetValueWithoutNotify((float)currentMaskData.lower / sliderRatio);// These are reversed as the shader for rendering camera feeds is upside-down
            maskingSiderB.SetValueWithoutNotify((float)currentMaskData.upper / sliderRatio);// These are reversed as the shader for rendering camera feeds is upside-down
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
        currentMaskData.device_id = diagnosticAPI.connectedDeviceID;
        currentMaskData.left = maskingSiderL.value * sliderRatio;
        currentMaskData.right = maskingSiderR.value * sliderRatio;
        currentMaskData.upper = maskingSiderB.value * sliderRatio; // These are reversed as the shader for rendering camera feeds is upside-down
        currentMaskData.lower = maskingSiderT.value * sliderRatio; // These are reversed as the shader for rendering camera feeds is upside-down

        diagnosticAPI.Request("SetImageMask:" + JsonUtility.ToJson(currentMaskData));
    }

    public void SetSliders(DiagnosticAPI.ImageMaskData _maskValues)
    {
        currentMaskData = _maskValues;
        maskingSiderL.SetValueWithoutNotify((float)currentMaskData.left / sliderRatio);
        maskingSiderR.SetValueWithoutNotify((float)currentMaskData.right / sliderRatio);
        maskingSiderT.SetValueWithoutNotify((float)currentMaskData.lower / sliderRatio);// These are reversed as the shader for rendering camera feeds is upside-down
        maskingSiderB.SetValueWithoutNotify((float)currentMaskData.upper / sliderRatio);// These are reversed as the shader for rendering camera feeds is upside-down
    }

    public void OnSliderChanged(float _)
    {
        SetMasking();
    }
}
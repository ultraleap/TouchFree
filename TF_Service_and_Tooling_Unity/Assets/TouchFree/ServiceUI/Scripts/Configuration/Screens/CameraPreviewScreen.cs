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

    void OnEnable()
    {
        enableOverexposureHighlighting.onValueChanged.AddListener(OnOverExposureValueChanged);
        OnOverExposureValueChanged(enableOverexposureHighlighting.isOn);

        if(diagnosticAPI == null)
        {
            diagnosticAPI = new DiagnosticAPI();
        }

        DiagnosticAPI.OnGetMaskingResponse += ReceiveMaskValues;

        maskingSiderL.onValueChanged.AddListener(OnSliderChanged);
        maskingSiderR.onValueChanged.AddListener(OnSliderChanged);
        maskingSiderT.onValueChanged.AddListener(OnSliderChanged);
        maskingSiderB.onValueChanged.AddListener(OnSliderChanged);

        diagnosticAPI.Request("GetImageMask:1");
    }

    void OnDisable()
    {
        enableOverexposureHighlighting.onValueChanged.RemoveListener(OnOverExposureValueChanged);
        DiagnosticAPI.OnGetMaskingResponse -= ReceiveMaskValues;

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

    void OnOverExposureValueChanged(bool state)
    {
        if (state) {
            leftCameraMat.SetFloat("_threshold", exposureThresholdValue);
            rightCameraMat.SetFloat("_threshold", exposureThresholdValue);
        } else {
            leftCameraMat.SetFloat("_threshold", 1.0f);
            rightCameraMat.SetFloat("_threshold", 1.0f);
        }
    }

    void SetMasking()
    {
        currentMaskData.device_id = 1; // not used but must not be 0
        currentMaskData.left = maskingSiderL.value * sliderRatio;
        currentMaskData.right = maskingSiderR.value * sliderRatio;
        currentMaskData.upper = maskingSiderB.value * sliderRatio; // These are reversed as the shader for rendering camera feeds is upside-down
        currentMaskData.lower = maskingSiderT.value * sliderRatio; // These are reversed as the shader for rendering camera feeds is upside-down

        diagnosticAPI.Request("SetImageMask:" + JsonUtility.ToJson(currentMaskData));
    }

    void SetSliders(DiagnosticAPI.ImageMaskData _maskValues)
    {
        currentMaskData = _maskValues;
        maskingSiderL.SetValueWithoutNotify((float)currentMaskData.left / sliderRatio);
        maskingSiderR.SetValueWithoutNotify((float)currentMaskData.right / sliderRatio);
        maskingSiderT.SetValueWithoutNotify((float)currentMaskData.lower / sliderRatio);// These are reversed as the shader for rendering camera feeds is upside-down
        maskingSiderB.SetValueWithoutNotify((float)currentMaskData.upper / sliderRatio);// These are reversed as the shader for rendering camera feeds is upside-down
    }

    public void ReceiveMaskValues(DiagnosticAPI.ImageMaskData _maskValues)
    {
        // This method is called on a thread via WebSocketSharp so Invoke a Delegate to ensure it is performed on the main thread
        var updateValuesDelegate = new Action(delegate ()
        {
            SetSliders(_maskValues);
        });

        updateValuesDelegate.Invoke();
    }

    public void OnSliderChanged(float _)
    {
        SetMasking();
    }
}
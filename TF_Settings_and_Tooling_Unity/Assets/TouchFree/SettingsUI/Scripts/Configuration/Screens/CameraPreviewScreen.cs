using System;
using UnityEngine;
using UnityEngine.UI;

using Ultraleap.TouchFree.Tooling.Tracking;
using Ultraleap.TouchFree.Tooling.Connection;

public class CameraPreviewScreen : MonoBehaviour
{
    public Toggle enableOverexposureHighlighting;

    public Toggle cameraReversedToggle;
    public Leap.Unity.LeapImageRetriever leapImageRetriever;

    [SerializeField]
    private float exposureThresholdValue = 0.5f;

    public Slider maskingSiderL, maskingSiderR, maskingSiderT, maskingSiderB;

    [Tooltip("The ratio of the slider length to the masking capacity. Total capacity is the full width/height of the camera image.")]
    public float sliderRatio = 0.5f;

    float currentMaskLeft, currentMaskRight, currentMaskTop, currentMaskBottom;
    bool newMaskDataReceived = false;

    public GameObject handsCameraObject;

    public Scrollbar contentScrollbar;

    void OnEnable()
    {
        leapImageRetriever.enabled = false;
        leapImageRetriever.enabled = true;
        leapImageRetriever.Reconstruct();

        handsCameraObject.SetActive(true);
        enableOverexposureHighlighting.onValueChanged.AddListener(OnOverExposureValueChanged);
        cameraReversedToggle.onValueChanged.AddListener(OnCameraReversedChanged);
        OnOverExposureValueChanged(enableOverexposureHighlighting.isOn);

        TrackingManager.RequestTrackingState(HandleTrackingResponse);

        maskingSiderL.onValueChanged.AddListener(OnSliderChanged);
        maskingSiderR.onValueChanged.AddListener(OnSliderChanged);
        maskingSiderT.onValueChanged.AddListener(OnSliderChanged);
        maskingSiderB.onValueChanged.AddListener(OnSliderChanged);

        contentScrollbar.value = 1;
    }

    void OnDisable()
    {
        handsCameraObject.SetActive(false);
        enableOverexposureHighlighting.onValueChanged.RemoveListener(OnOverExposureValueChanged);
        cameraReversedToggle.onValueChanged.RemoveListener(OnCameraReversedChanged);

        maskingSiderL.onValueChanged.RemoveListener(OnSliderChanged);
        maskingSiderR.onValueChanged.RemoveListener(OnSliderChanged);
        maskingSiderT.onValueChanged.RemoveListener(OnSliderChanged);
        maskingSiderB.onValueChanged.RemoveListener(OnSliderChanged);
    }

    private void Update()
    {
        if (newMaskDataReceived)
        {
            SetSliders(currentMaskLeft, currentMaskRight, currentMaskTop, currentMaskBottom);
            newMaskDataReceived = false;
        }
    }

    void OnOverExposureValueChanged(bool state)
    {
        if (state)
        {
            Shader.SetGlobalFloat("_threshold", exposureThresholdValue);
        }
        else
        {
            Shader.SetGlobalFloat("_threshold", 1.0f);
        }
    }

    void OnCameraReversedChanged(bool state)
    {
        var newState = new TrackingState(null, cameraReversedToggle.isOn, null, null);

        TrackingManager.RequestTrackingChange(newState);
    }

    public void OnSliderChanged(float _)
    {
        SetMasking();
    }

    void SetMasking()
    {
        var mask = new MaskData(
            maskingSiderT.value * sliderRatio,
            maskingSiderB.value * sliderRatio,
            maskingSiderR.value * sliderRatio,
            maskingSiderL.value * sliderRatio
        );

        var newState = new TrackingState(mask, null, null, null);

        TrackingManager.RequestTrackingChange(newState);
    }

    private void HandleTrackingResponse(TrackingStateResponse _response)
    {
        var maskData = _response.mask.Value.content;
        var cameraReversed = _response.cameraReversed.Value.content;
        var allowImages = _response.allowImages.Value.content;

        if (maskData.HasValue) {
            SetSliders(
                maskData.Value.left,
                maskData.Value.right,
                maskData.Value.upper,
                maskData.Value.lower
            );
        }

        if (cameraReversed.HasValue) {

            cameraReversedToggle.isOn = cameraReversed.Value;
        }

        if (allowImages.HasValue && !allowImages.Value)
        {
            Leap.Unity.LeapImageRetriever.EyeTextureData.ResetGlobalShaderValues();
        }
    }

    public void SetSliders(float _left, float _right, float _top, float _bottom)
    {
        maskingSiderL.SetValueWithoutNotify(_left / sliderRatio);
        maskingSiderR.SetValueWithoutNotify(_right / sliderRatio);
        maskingSiderT.SetValueWithoutNotify(_bottom / sliderRatio);// These are reversed as the shader for rendering camera feeds is upside-down
        maskingSiderB.SetValueWithoutNotify(_top / sliderRatio);// These are reversed as the shader for rendering camera feeds is upside-down
    }
}
using System;
using System.Globalization;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class InteractionTypeElements
{
    public InteractionSelection interactionSelection;
    public CanvasGroup[] typeSpecificElements;
}

public class SettingsConfigUI : ConfigUI
{
    // UI elements
    [Header("Resolution")]
    public InputField resolutionWidth;
    public InputField resolutionHeight;

    [Header("Cursor Size")]
    public InputField cursorDotSize;
    public Slider cursorDotSizeSlider;
    public InputField cursorMaxRingScale;
    public Slider cursorMaxRingScaleSlider;
    public InputField cursorMaxRingScaleAtDistance;
    public Slider cursorMaxRingScaleAtDistanceSlider;

    [Header("Cursor Colours")]
    public InputField cursorRingColor;
    public Slider cursorRingOpacity;
    public InputField cursorDotFillColor;
    public Slider cursorDotFillOpacity;
    public InputField cursorDotBorderColor;
    public Slider cursorDotBorderOpacity;

    public Toggle cursorColorPresetToggleLight;
    public Toggle cursorColorPresetToggleDark;
    public Toggle cursorColorPresetToggleLightContrast;
    public Toggle cursorColorPresetToggleDarkContrast;
    public Toggle cursorColorPresetToggleCustom;

    public GameObject[] customCursorColourLabels;

    [Header("Call to Interact")]
    public Toggle CTIEnableTog;
    public InputField CTIShowTimer;
    public InputField CTIHideTimer;
    public Dropdown CTIFileDropdown;
    public Toggle CTIEndOnPresentTog;
    public Toggle CTIEndOnInteractionTog;
    public GameObject activityHideOption;

    [Header("Misc")]
    public Toggle scrollingOrDraggingTog;
    public Toggle setupOnStartup;
    public Toggle sendHoverEventsTog;
    public GameObject hoverEventsWarning;

    public Slider cursorDeadzoneSlider;

    public InputField cursorVerticalOffset;
    public Slider cursorVerticalOffsetSlider;

    public InputField HoverCursorStartTime;
    public Slider HoverCursorStartTimeSlider;
    public InputField HoverCursorCompleteTime;
    public Slider HoverCursorCompleteTimeSlider;

    [Header("Interaction Type")]
    public Toggle interactionTypeTogglePush;
    public Toggle interactionTypeTogglePoke;
    public Toggle interactionTypeTogglePinch;
    public Toggle interactionTypeToggleHover;

    [Header("Interaction Preview")]
    public GameObject pushPreview;
    public GameObject pokePreview;
    public GameObject grabPreview;
    public GameObject hoverPreview;

    [Header("Cursor Preview")]
    public GameObject pushCursorPreview;
    public Image pushCursorPreviewDot;
    public Image pushCursorPreviewBorder;
    public Image pushCursorPreviewRing;
    public GameObject grabCursorPreview;
    public Image grabCursorPreviewDot;
    public Image grabCursorPreviewBorder;
    public Image grabCursorPreviewLine;
    public GameObject hoverCursorPreview;
    public Image hoverCursorPreviewDot;
    public Image hoverCursorPreviewBorder;
    public Image hoverCursorPreviewRing;
    public Image hoverCursorPreviewRingBorder;

    public GameObject resetToDefaultWarning;

    [Tooltip("List all Settings elements that relate to the interactionType. This way they can be greyed out if not currently in use by the current InteractionType")]
    public InteractionTypeElements[] interactionTypeElements;

    private void Awake()
    {
        InitialiseUI();
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB");
        Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-GB");
        resetToDefaultWarning.SetActive(false);
    }

    void InitialiseUI()
    {
        cursorDotSizeSlider.minValue = ScreenControlUtility.ToDisplayUnits(SettingsConfig.CursorDotSize_Min);
        cursorDotSizeSlider.maxValue = ScreenControlUtility.ToDisplayUnits(SettingsConfig.CursorDotSize_Max);
        cursorMaxRingScaleSlider.minValue = SettingsConfig.CursorRingMaxScale_Min;
        cursorMaxRingScaleSlider.maxValue = SettingsConfig.CursorRingMaxScale_Max;
        cursorMaxRingScaleAtDistanceSlider.minValue = ScreenControlUtility.ToDisplayUnits(SettingsConfig.CursorMaxRingScaleAtDistance_Min);
        cursorMaxRingScaleAtDistanceSlider.maxValue = ScreenControlUtility.ToDisplayUnits(SettingsConfig.CursorMaxRingScaleAtDistance_Max);

        cursorDeadzoneSlider.minValue = SettingsConfig.CursorDeadzone_Min;
        cursorDeadzoneSlider.maxValue = SettingsConfig.CursorDeadzone_Max;

        cursorVerticalOffsetSlider.minValue = ScreenControlUtility.ToDisplayUnits(SettingsConfig.CursorVerticalOffset_Min);
        cursorVerticalOffsetSlider.maxValue = ScreenControlUtility.ToDisplayUnits(SettingsConfig.CursorVerticalOffset_Max);

        HoverCursorStartTimeSlider.minValue = SettingsConfig.HoverCursorStartTime_Min;
        HoverCursorStartTimeSlider.maxValue = SettingsConfig.HoverCursorStartTime_Max;
        HoverCursorCompleteTimeSlider.minValue = SettingsConfig.HoverCursorCompleteTime_Min;
        HoverCursorCompleteTimeSlider.maxValue = SettingsConfig.HoverCursorCompleteTime_Max;

        customCursorDotFillColour = SettingsConfig.Config.CursorDotFillColor;
        customCursorDotBorderColour = SettingsConfig.Config.CursorDotBorderColor;
        customCursorRingColour = SettingsConfig.Config.CursorRingColor;
        customCursorDotFillOpacity = SettingsConfig.Config.CursorDotFillOpacity;
        customCursorDotBorderOpacity = SettingsConfig.Config.CursorDotBorderOpacity;
        customCursorRingOpacity = SettingsConfig.Config.CursorRingOpacity;

        if (SettingsConfig.Config.CursorColorPreset == CursorColourPreset.custom)
        {
            // we are in custom, so only worry about custom colours
            foreach (var label in customCursorColourLabels)
                label.SetActive(true);
        }
        else
        {
            foreach (var label in customCursorColourLabels)
                label.SetActive(false);
        }
    }

    public void ResetToDefaultValues()
    {
        CallToInteractConfig.SetAllValuesToDefault();
        CallToInteractConfig.SaveConfig();
        SettingsConfig.SetAllValuesToDefault();
        SettingsConfig.SaveConfig();
        LoadConfigValuesIntoFields();
        ValidateCursorColourPresets();
        SaveValuesToConfig();
    }

    public void SetResolution()
    {
        var width = int.Parse(resolutionWidth.text);
        var height = int.Parse(resolutionHeight.text);

        if (width < 200)
        {
            width = 200;
            resolutionWidth.text = "200";
        }

        if (height < 200)
        {
            height = 200;
            resolutionHeight.text = "200";
        }

        GlobalSettings.ScreenWidth = width;
        GlobalSettings.ScreenHeight = height;
        PhysicalConfigurable.UpdateConfig(PhysicalConfigurable.Config);
        ConfigurationSetupController.Instance.RefreshConfigActive();
        OnValueChanged();
    }

    protected override void AddValueChangedListeners()
    {
        cursorDotSize.onEndEdit.AddListener(OnValueChanged);
        cursorDotSizeSlider.onValueChanged.AddListener(OnValueChanged);
        cursorMaxRingScale.onEndEdit.AddListener(OnValueChanged);
        cursorMaxRingScaleSlider.onValueChanged.AddListener(OnValueChanged);
        cursorMaxRingScaleAtDistance.onEndEdit.AddListener(OnValueChanged);
        cursorMaxRingScaleAtDistanceSlider.onValueChanged.AddListener(OnValueChanged);

        cursorRingColor.onEndEdit.AddListener(OnValueChanged);
        cursorRingOpacity.onValueChanged.AddListener(OnValueChanged);
        cursorDotFillColor.onEndEdit.AddListener(OnValueChanged);
        cursorDotFillOpacity.onValueChanged.AddListener(OnValueChanged);
        cursorDotBorderColor.onEndEdit.AddListener(OnValueChanged);
        cursorDotBorderOpacity.onValueChanged.AddListener(OnValueChanged);

        scrollingOrDraggingTog.onValueChanged.AddListener(OnValueChanged);
        setupOnStartup.onValueChanged.AddListener(OnValueChanged);
        cursorDeadzoneSlider.onValueChanged.AddListener(OnValueChanged);
        cursorVerticalOffset.onEndEdit.AddListener(OnValueChanged);
        cursorVerticalOffsetSlider.onValueChanged.AddListener(OnValueChanged);
        sendHoverEventsTog.onValueChanged.AddListener(OnValueChanged);

        HoverCursorStartTime.onEndEdit.AddListener(OnValueChanged);
        HoverCursorStartTimeSlider.onValueChanged.AddListener(OnValueChanged);
        HoverCursorCompleteTime.onEndEdit.AddListener(OnValueChanged);
        HoverCursorCompleteTimeSlider.onValueChanged.AddListener(OnValueChanged);

        interactionTypeTogglePush.onValueChanged.AddListener(OnValueChanged);
        interactionTypeTogglePoke.onValueChanged.AddListener(OnValueChanged);
        interactionTypeTogglePinch.onValueChanged.AddListener(OnValueChanged);
        interactionTypeToggleHover.onValueChanged.AddListener(OnValueChanged);

        cursorColorPresetToggleLight.onValueChanged.AddListener(ValidateCursorColourPresets);
        cursorColorPresetToggleDark.onValueChanged.AddListener(ValidateCursorColourPresets);
        cursorColorPresetToggleLightContrast.onValueChanged.AddListener(ValidateCursorColourPresets);
        cursorColorPresetToggleDarkContrast.onValueChanged.AddListener(ValidateCursorColourPresets);
        cursorColorPresetToggleCustom.onValueChanged.AddListener(ValidateCursorColourPresets);

        CTIEnableTog.onValueChanged.AddListener(OnValueChanged);
        CTIShowTimer.onEndEdit.AddListener(OnValueChanged);
        CTIHideTimer.onEndEdit.AddListener(OnValueChanged);
        CTIFileDropdown.onValueChanged.AddListener(OnValueChanged);

        CTIEndOnPresentTog.onValueChanged.AddListener(OnValueChanged);
        CTIEndOnInteractionTog.onValueChanged.AddListener(OnValueChanged);
    }

    protected override void RemoveValueChangedListeners()
    {
        cursorDotSize.onEndEdit.RemoveListener(OnValueChanged);
        cursorDotSizeSlider.onValueChanged.RemoveListener(OnValueChanged);
        cursorMaxRingScale.onEndEdit.RemoveListener(OnValueChanged);
        cursorMaxRingScaleSlider.onValueChanged.RemoveListener(OnValueChanged);
        cursorMaxRingScaleAtDistance.onEndEdit.RemoveListener(OnValueChanged);
        cursorMaxRingScaleAtDistanceSlider.onValueChanged.RemoveListener(OnValueChanged);

        cursorRingColor.onEndEdit.RemoveListener(OnValueChanged);
        cursorRingOpacity.onValueChanged.RemoveListener(OnValueChanged);
        cursorDotFillColor.onEndEdit.RemoveListener(OnValueChanged);
        cursorDotFillOpacity.onValueChanged.RemoveListener(OnValueChanged);
        cursorDotBorderColor.onEndEdit.RemoveListener(OnValueChanged);
        cursorDotBorderOpacity.onValueChanged.RemoveListener(OnValueChanged);

        scrollingOrDraggingTog.onValueChanged.RemoveListener(OnValueChanged);
        setupOnStartup.onValueChanged.RemoveListener(OnValueChanged);
        cursorDeadzoneSlider.onValueChanged.RemoveListener(OnValueChanged);
        cursorVerticalOffset.onEndEdit.RemoveListener(OnValueChanged);
        cursorVerticalOffsetSlider.onValueChanged.RemoveListener(OnValueChanged);
        sendHoverEventsTog.onValueChanged.RemoveListener(OnValueChanged);

        HoverCursorStartTime.onEndEdit.RemoveListener(OnValueChanged);
        HoverCursorStartTimeSlider.onValueChanged.RemoveListener(OnValueChanged);
        HoverCursorCompleteTime.onEndEdit.RemoveListener(OnValueChanged);
        HoverCursorCompleteTimeSlider.onValueChanged.RemoveListener(OnValueChanged);

        interactionTypeTogglePush.onValueChanged.RemoveListener(OnValueChanged);
        interactionTypeTogglePoke.onValueChanged.RemoveListener(OnValueChanged);
        interactionTypeTogglePinch.onValueChanged.RemoveListener(OnValueChanged);
        interactionTypeToggleHover.onValueChanged.RemoveListener(OnValueChanged);

        cursorColorPresetToggleLight.onValueChanged.RemoveListener(ValidateCursorColourPresets);
        cursorColorPresetToggleDark.onValueChanged.RemoveListener(ValidateCursorColourPresets);
        cursorColorPresetToggleLightContrast.onValueChanged.RemoveListener(ValidateCursorColourPresets);
        cursorColorPresetToggleDarkContrast.onValueChanged.RemoveListener(ValidateCursorColourPresets);
        cursorColorPresetToggleCustom.onValueChanged.RemoveListener(ValidateCursorColourPresets);

        CTIEnableTog.onValueChanged.RemoveListener(OnValueChanged);
        CTIShowTimer.onEndEdit.RemoveListener(OnValueChanged);
        CTIHideTimer.onEndEdit.RemoveListener(OnValueChanged);
        CTIFileDropdown.onValueChanged.RemoveListener(OnValueChanged);

        CTIEndOnPresentTog.onValueChanged.RemoveListener(OnValueChanged);
        CTIEndOnInteractionTog.onValueChanged.RemoveListener(OnValueChanged);
    }

    protected override void LoadConfigValuesIntoFields()
    {
        resolutionWidth.text = Screen.currentResolution.width.ToString();
        resolutionHeight.text = Screen.currentResolution.height.ToString();

        cursorDotSize.SetTextWithoutNotify(ScreenControlUtility.ToDisplayUnits(SettingsConfig.Config.CursorDotSizeM).ToString("#0.00#"));
        cursorDotSizeSlider.SetValueWithoutNotify(ScreenControlUtility.ToDisplayUnits(SettingsConfig.Config.CursorDotSizeM));
        cursorMaxRingScale.SetTextWithoutNotify(SettingsConfig.Config.CursorRingMaxScale.ToString("#0.00#"));
        cursorMaxRingScaleSlider.SetValueWithoutNotify(SettingsConfig.Config.CursorRingMaxScale);
        cursorMaxRingScaleAtDistance.SetTextWithoutNotify(ScreenControlUtility.ToDisplayUnits(SettingsConfig.Config.CursorMaxRingScaleAtDistanceM).ToString("#0.00#"));
        cursorMaxRingScaleAtDistanceSlider.SetValueWithoutNotify(ScreenControlUtility.ToDisplayUnits(SettingsConfig.Config.CursorMaxRingScaleAtDistanceM));
        scrollingOrDraggingTog.SetIsOnWithoutNotify(SettingsConfig.Config.UseScrollingOrDragging);
        setupOnStartup.SetIsOnWithoutNotify(SettingsConfig.Config.ShowSetupScreenOnStartup);
        sendHoverEventsTog.SetIsOnWithoutNotify(SettingsConfig.Config.SendHoverEvents);

        cursorRingColor.SetTextWithoutNotify(SettingsConfig.Config.CursorRingColor);
        cursorRingOpacity.SetValueWithoutNotify(SettingsConfig.Config.CursorRingOpacity);
        cursorDotFillColor.SetTextWithoutNotify(SettingsConfig.Config.CursorDotFillColor);
        cursorDotFillOpacity.SetValueWithoutNotify(SettingsConfig.Config.CursorDotFillOpacity);
        cursorDotBorderColor.SetTextWithoutNotify(SettingsConfig.Config.CursorDotBorderColor);
        cursorDotBorderOpacity.SetValueWithoutNotify(SettingsConfig.Config.CursorDotBorderOpacity);

        cursorDeadzoneSlider.SetValueWithoutNotify(SettingsConfig.Config.DeadzoneRadius);
        cursorVerticalOffset.SetTextWithoutNotify(ScreenControlUtility.ToDisplayUnits(SettingsConfig.Config.CursorVerticalOffset).ToString("#0.00#"));
        cursorVerticalOffsetSlider.SetValueWithoutNotify(ScreenControlUtility.ToDisplayUnits(SettingsConfig.Config.CursorVerticalOffset));

        HoverCursorStartTime.SetTextWithoutNotify(SettingsConfig.Config.HoverCursorStartTimeS.ToString("#0.00#"));
        HoverCursorStartTimeSlider.SetValueWithoutNotify(SettingsConfig.Config.HoverCursorStartTimeS);
        HoverCursorCompleteTime.SetTextWithoutNotify(SettingsConfig.Config.HoverCursorCompleteTimeS.ToString("#0.00#"));
        HoverCursorCompleteTimeSlider.SetValueWithoutNotify(SettingsConfig.Config.HoverCursorCompleteTimeS);

        interactionTypeTogglePush.SetIsOnWithoutNotify(false);
        interactionTypeTogglePoke.SetIsOnWithoutNotify(false);
        interactionTypeTogglePinch.SetIsOnWithoutNotify(false);
        interactionTypeToggleHover.SetIsOnWithoutNotify(false);
        switch (SettingsConfig.Config.InteractionSelection)
        {
            case InteractionSelection.Push:
                interactionTypeTogglePush.SetIsOnWithoutNotify(true);
                break;
            case InteractionSelection.Poke:
                interactionTypeTogglePoke.SetIsOnWithoutNotify(true);
                break;
            case InteractionSelection.Grab:
                interactionTypeTogglePinch.SetIsOnWithoutNotify(true);
                break;
            case InteractionSelection.Hover:
                interactionTypeToggleHover.SetIsOnWithoutNotify(true);
                break;
        }

        cursorColorPresetToggleLight.SetIsOnWithoutNotify(false);
        cursorColorPresetToggleDark.SetIsOnWithoutNotify(false);
        cursorColorPresetToggleLightContrast.SetIsOnWithoutNotify(false);
        cursorColorPresetToggleDarkContrast.SetIsOnWithoutNotify(false);
        cursorColorPresetToggleCustom.SetIsOnWithoutNotify(false);
        switch (SettingsConfig.Config.CursorColorPreset)
        {
            case CursorColourPreset.light:
                cursorColorPresetToggleLight.SetIsOnWithoutNotify(true);
                break;
            case CursorColourPreset.dark:
                cursorColorPresetToggleDark.SetIsOnWithoutNotify(true);
                break;
            case CursorColourPreset.white_Contrast:
                cursorColorPresetToggleLightContrast.SetIsOnWithoutNotify(true);
                break;
            case CursorColourPreset.black_Contrast:
                cursorColorPresetToggleDarkContrast.SetIsOnWithoutNotify(true);
                break;
            case CursorColourPreset.custom:
                cursorColorPresetToggleCustom.SetIsOnWithoutNotify(true);
                break;
        }

        if(SettingsConfig.Config.CursorColorPreset == CursorColourPreset.custom)
        {
            foreach (var label in customCursorColourLabels)
                label.SetActive(true);
        }
        else
        {
            foreach (var label in customCursorColourLabels)
                label.SetActive(false);
        }

        ValidateInteractionTypeElements();

        CTIEnableTog.SetIsOnWithoutNotify(CallToInteractConfig.Config.Enabled);
        CTIShowTimer.SetTextWithoutNotify(CallToInteractConfig.Config.ShowTimeAfterNoHandPresent.ToString("#0.00#"));
        CTIHideTimer.SetTextWithoutNotify(CallToInteractConfig.Config.HideTimeAfterHandPresent.ToString("#0.00#"));

        switch (CallToInteractConfig.Config.hideType)
        {
            case HideRequirement.PRESENT:
                CTIEndOnPresentTog.SetIsOnWithoutNotify(true);
                CTIEndOnInteractionTog.SetIsOnWithoutNotify(false);
                activityHideOption.SetActive(true);
                break;
            case HideRequirement.INTERACTION:
                CTIEndOnPresentTog.SetIsOnWithoutNotify(false);
                CTIEndOnInteractionTog.SetIsOnWithoutNotify(true);
                activityHideOption.SetActive(false);
                break;
        }

        hoverEventsWarning.SetActive(sendHoverEventsTog.isOn);

        LoadCTIFilesIntoDropdown();
        DisplayCursorPreview();
        DisplayIntractionPreview();
    }

    void LoadCTIFilesIntoDropdown()
    {
        CTIFileDropdown.ClearOptions();

        // find all CTI files
        string[] fileNames = CallToInteractController.GetCTIFileNames();

        if (fileNames != null)
        {
            foreach (var option in fileNames)
            {
                CTIFileDropdown.options.Add(new Dropdown.OptionData(option));
            }

            int dropdownIndex = 0;
            var dropdownOptions = CTIFileDropdown.options;

            for (int i = 0; i < dropdownOptions.Count; i++)
            {
                if (dropdownOptions[i].text == CallToInteractConfig.Config.CurrentFileName)
                {
                    dropdownIndex = i;
                    break;
                }
            }

            CTIFileDropdown.SetValueWithoutNotify(dropdownIndex);
            CTIFileDropdown.RefreshShownValue();
        }
    }

    protected override void ValidateValues()
    {
        var dotSize = Mathf.Clamp(cursorDotSizeSlider.value, ScreenControlUtility.ToDisplayUnits(SettingsConfig.CursorDotSize_Min), ScreenControlUtility.ToDisplayUnits(SettingsConfig.CursorDotSize_Max));
        cursorDotSizeSlider.SetValueWithoutNotify(dotSize);
        cursorDotSize.SetTextWithoutNotify(dotSize.ToString("#0.00#"));

        var ringScale = Mathf.Clamp(cursorMaxRingScaleSlider.value, SettingsConfig.CursorRingMaxScale_Min, SettingsConfig.CursorRingMaxScale_Max);
        cursorMaxRingScaleSlider.SetValueWithoutNotify(ringScale);
        cursorMaxRingScale.SetTextWithoutNotify(ringScale.ToString("#0.00#"));

        var maxDistance = Mathf.Clamp(cursorMaxRingScaleAtDistanceSlider.value, ScreenControlUtility.ToDisplayUnits(SettingsConfig.CursorMaxRingScaleAtDistance_Min), ScreenControlUtility.ToDisplayUnits(SettingsConfig.CursorMaxRingScaleAtDistance_Max));
        cursorMaxRingScaleAtDistanceSlider.SetValueWithoutNotify(maxDistance);
        cursorMaxRingScaleAtDistance.SetTextWithoutNotify(maxDistance.ToString("#0.00#"));

        var deadzoneRadius = Mathf.Clamp(cursorDeadzoneSlider.value, SettingsConfig.CursorDeadzone_Min, SettingsConfig.CursorDeadzone_Max);
        cursorDeadzoneSlider.SetValueWithoutNotify(deadzoneRadius);

        var verticalOffset = Mathf.Clamp(cursorVerticalOffsetSlider.value, ScreenControlUtility.ToDisplayUnits(SettingsConfig.CursorVerticalOffset_Min), ScreenControlUtility.ToDisplayUnits(SettingsConfig.CursorVerticalOffset_Max));
        cursorVerticalOffsetSlider.SetValueWithoutNotify(verticalOffset);
        cursorVerticalOffset.SetTextWithoutNotify(verticalOffset.ToString("#0.00#"));

        var hoverStartTime = Mathf.Clamp(HoverCursorStartTimeSlider.value, SettingsConfig.HoverCursorStartTime_Min, SettingsConfig.HoverCursorStartTime_Max);
        HoverCursorStartTime.SetTextWithoutNotify(hoverStartTime.ToString("#0.00#"));
        HoverCursorStartTimeSlider.SetValueWithoutNotify(hoverStartTime);

        var hoverCompleteTime = Mathf.Clamp(HoverCursorCompleteTimeSlider.value,SettingsConfig.HoverCursorCompleteTime_Min, SettingsConfig.HoverCursorCompleteTime_Max);
        HoverCursorCompleteTime.SetTextWithoutNotify(hoverCompleteTime.ToString("#0.00#"));
        HoverCursorCompleteTimeSlider.SetValueWithoutNotify(hoverCompleteTime);

        CTIShowTimer.SetTextWithoutNotify(TryParseNewStringToFloat(ref CallToInteractConfig.Config.ShowTimeAfterNoHandPresent, CTIShowTimer.text).ToString("#0.00#"));
        CTIHideTimer.SetTextWithoutNotify(TryParseNewStringToFloat(ref CallToInteractConfig.Config.HideTimeAfterHandPresent, CTIHideTimer.text).ToString("#0.00#"));

        switch (CallToInteractConfig.Config.hideType)
        {
            case HideRequirement.PRESENT:
                activityHideOption.SetActive(true);
                break;
            case HideRequirement.INTERACTION:
                activityHideOption.SetActive(false);
                break;
        }

        ValidateInteractionTypeElements();
        hoverEventsWarning.SetActive(sendHoverEventsTog.isOn);
    }

    string customCursorDotFillColour;
    string customCursorDotBorderColour;
    string customCursorRingColour;

    float customCursorDotFillOpacity;
    float customCursorDotBorderOpacity;
    float customCursorRingOpacity;

    private void ValidateCursorColourPresets(bool _ = false)
    {
        // When a toggle in pressed, we need to set the new preset and trigger a 'OnValueChanged()'
        if (cursorColorPresetToggleCustom.isOn)
        {
            // we are in custom, so only worry about custom colours
            foreach (var label in customCursorColourLabels)
                label.SetActive(true);

            if (SettingsConfig.Config.CursorColorPreset != CursorColourPreset.custom)
            {
                // it has just changes so set the known custom values
                cursorDotBorderColor.SetTextWithoutNotify(customCursorDotBorderColour);
                cursorDotFillColor.SetTextWithoutNotify(customCursorDotFillColour);
                cursorRingColor.SetTextWithoutNotify(customCursorRingColour);
                cursorDotBorderOpacity.SetValueWithoutNotify(customCursorDotBorderOpacity);
                cursorDotFillOpacity.SetValueWithoutNotify(customCursorDotFillOpacity);
                cursorRingOpacity.SetValueWithoutNotify(customCursorRingOpacity);

                OnValueChanged();
            }
            return;
        }
        else
        {
            foreach (var label in customCursorColourLabels)
                label.SetActive(false);
        }

        CursorColourPreset newPreset = CursorColourPreset.custom;

        if (cursorColorPresetToggleLight.isOn)
        {
            newPreset = CursorColourPreset.light;
        }
        else if (cursorColorPresetToggleDark.isOn)
        {
            newPreset = CursorColourPreset.dark;
        }
        else if (cursorColorPresetToggleLightContrast.isOn)
        {
            newPreset = CursorColourPreset.white_Contrast;
        }
        else if (cursorColorPresetToggleDarkContrast.isOn)
        {
            newPreset = CursorColourPreset.black_Contrast;
        }

        if (newPreset == SettingsConfig.Config.CursorColorPreset) // only continue on if they have clicked a new toggle button.
            return;

        switch (newPreset)
        {
            case CursorColourPreset.light:
                    cursorDotBorderColor.SetTextWithoutNotify(SettingsConfig.CursorColourPresetWhite);
                    cursorDotFillColor.SetTextWithoutNotify(SettingsConfig.CursorColourPresetLight);
                    cursorRingColor.SetTextWithoutNotify(SettingsConfig.CursorColourPresetLight);
                break;
            case CursorColourPreset.dark:
                    cursorDotBorderColor.SetTextWithoutNotify(SettingsConfig.CursorColourPresetBlack);
                    cursorDotFillColor.SetTextWithoutNotify(SettingsConfig.CursorColourPresetDark);
                    cursorRingColor.SetTextWithoutNotify(SettingsConfig.CursorColourPresetDark);
                break;
            case CursorColourPreset.white_Contrast:
                    cursorDotBorderColor.SetTextWithoutNotify(SettingsConfig.CursorColourPresetBlack);
                    cursorDotFillColor.SetTextWithoutNotify(SettingsConfig.CursorColourPresetWhite);
                    cursorRingColor.SetTextWithoutNotify(SettingsConfig.CursorColourPresetWhite);
                break;
            case CursorColourPreset.black_Contrast:
                    cursorDotBorderColor.SetTextWithoutNotify(SettingsConfig.CursorColourPresetWhite);
                    cursorDotFillColor.SetTextWithoutNotify(SettingsConfig.CursorColourPresetBlack);
                    cursorRingColor.SetTextWithoutNotify(SettingsConfig.CursorColourPresetBlack);
                break;
        }

        cursorDotBorderOpacity.SetValueWithoutNotify(SettingsConfig.CursorDotBorderOpacityPreset);
        cursorDotFillOpacity.SetValueWithoutNotify(SettingsConfig.CursorDotFillOpacityPreset);
        cursorRingOpacity.SetValueWithoutNotify(SettingsConfig.CursorRingOpacityPreset);

        OnValueChanged();
    }

    void ValidateInteractionTypeElements()
    {
        // Loop through the interactionTypeElements to find the current one. Also grey out all other elements first.
        int currentIndex = -1;

        for (int i = 0; i < interactionTypeElements.Length; i++)
        {
            if (interactionTypeElements[i].interactionSelection == SettingsConfig.Config.InteractionSelection)
                currentIndex = i;

            foreach(var element in interactionTypeElements[i].typeSpecificElements)
            {
                element.gameObject.SetActive(false);
                element.alpha = 0.2f;
                element.interactable = false;
            }
        }

        // Remove grey-out from current interactionTypeElements
        if(currentIndex != -1)
        {
            foreach (var element in interactionTypeElements[currentIndex].typeSpecificElements)
            {
                element.gameObject.SetActive(true);
                element.alpha = 1f;
                element.interactable = true;
            }
        }
    }

    void DisplayCursorPreview()
    {
        var dotFillColor = ScreenControlUtility.ParseColor(SettingsConfig.Config.CursorDotFillColor, SettingsConfig.Config.CursorDotFillOpacity);
        var dotBorderColor = ScreenControlUtility.ParseColor(SettingsConfig.Config.CursorDotBorderColor, SettingsConfig.Config.CursorDotBorderOpacity);
        var ringColor = ScreenControlUtility.ParseColor(SettingsConfig.Config.CursorRingColor, SettingsConfig.Config.CursorRingOpacity);

        pushCursorPreview.SetActive(false);
        grabCursorPreview.SetActive(false);
        hoverCursorPreview.SetActive(false);

        switch (SettingsConfig.Config.InteractionSelection)
        {
            case InteractionSelection.Push:
            case InteractionSelection.Poke:
                pushCursorPreview.SetActive(true);
                pushCursorPreviewBorder.color = dotBorderColor;
                pushCursorPreviewDot.color = dotFillColor;
                pushCursorPreviewRing.color = ringColor;
                break;
            case InteractionSelection.Grab:
                grabCursorPreview.SetActive(true);
                grabCursorPreviewBorder.color = dotBorderColor;
                grabCursorPreviewDot.color = dotFillColor;
                grabCursorPreviewLine.color = ringColor;
                break;
            case InteractionSelection.Hover:
                hoverCursorPreview.SetActive(true);
                hoverCursorPreviewBorder.color = dotBorderColor;
                hoverCursorPreviewDot.color = dotFillColor;
                hoverCursorPreviewRingBorder.color = dotBorderColor;
                hoverCursorPreviewRing.color = ringColor;
                break;
        }
    }

    void DisplayIntractionPreview()
    {
        pushPreview.SetActive(false);
        pokePreview.SetActive(false);
        grabPreview.SetActive(false);
        hoverPreview.SetActive(false);

        switch (SettingsConfig.Config.InteractionSelection)
        {
            case InteractionSelection.Push:
                pushPreview.SetActive(true);
                break;
            case InteractionSelection.Poke:
                pokePreview.SetActive(true);
                break;
            case InteractionSelection.Grab:
                grabPreview.SetActive(true);
                break;
            case InteractionSelection.Hover:
                hoverPreview.SetActive(true);
                break;
        }
    }

    protected override void SaveValuesToConfig()
    {
        var config = SettingsConfig.Config;

        config.CursorDotSizeM = TryParseNewStringToFloat(ref config.CursorDotSizeM, cursorDotSize.text, true);
        config.CursorRingMaxScale = TryParseNewStringToFloat(ref config.CursorRingMaxScale, cursorMaxRingScale.text);
        config.CursorMaxRingScaleAtDistanceM = TryParseNewStringToFloat(ref config.CursorMaxRingScaleAtDistanceM, cursorMaxRingScaleAtDistance.text, true);

        config.CursorRingColor = TryParseHexColour(ref config.CursorRingColor, cursorRingColor.text);
        config.CursorRingOpacity = cursorRingOpacity.value;
        config.CursorDotFillColor = TryParseHexColour(ref config.CursorDotFillColor, cursorDotFillColor.text);
        config.CursorDotFillOpacity = cursorDotFillOpacity.value;
        config.CursorDotBorderColor = TryParseHexColour(ref config.CursorDotBorderColor, cursorDotBorderColor.text);
        config.CursorDotBorderOpacity = cursorDotBorderOpacity.value;

        config.UseScrollingOrDragging = scrollingOrDraggingTog.isOn;
        config.ShowSetupScreenOnStartup = setupOnStartup.isOn;
        config.SendHoverEvents = sendHoverEventsTog.isOn;

        config.DeadzoneRadius = cursorDeadzoneSlider.value;
        config.CursorVerticalOffset = TryParseNewStringToFloat(ref config.CursorVerticalOffset, cursorVerticalOffset.text, true);

        config.HoverCursorStartTimeS = HoverCursorStartTimeSlider.value;
        config.HoverCursorCompleteTimeS = HoverCursorCompleteTimeSlider.value;

        if (interactionTypeTogglePush.isOn)
        {
            config.InteractionSelection = InteractionSelection.Push;
        }
        else if (interactionTypeTogglePoke.isOn)
        {
            config.InteractionSelection = InteractionSelection.Poke;
        }
        else if (interactionTypeTogglePinch.isOn)
        {
            config.InteractionSelection = InteractionSelection.Grab;
        }
        else
        {
            config.InteractionSelection = InteractionSelection.Hover;
        }

        if(cursorColorPresetToggleLight.isOn)
        {
            config.CursorColorPreset = CursorColourPreset.light;
        }
        else if(cursorColorPresetToggleDark.isOn)
        {
            config.CursorColorPreset = CursorColourPreset.dark;
        }
        else if(cursorColorPresetToggleLightContrast.isOn)
        {
            config.CursorColorPreset = CursorColourPreset.white_Contrast;
        }
        else if(cursorColorPresetToggleDarkContrast.isOn)
        {
            config.CursorColorPreset = CursorColourPreset.black_Contrast;
        }
        else
        {
            config.CursorColorPreset = CursorColourPreset.custom;

            customCursorDotFillColour = config.CursorDotFillColor;
            customCursorDotBorderColour = config.CursorDotBorderColor;
            customCursorRingColour = config.CursorRingColor;

            customCursorDotFillOpacity = config.CursorDotFillOpacity;
            customCursorDotBorderOpacity = config.CursorDotBorderOpacity;
            customCursorRingOpacity = config.CursorRingOpacity;
        }

        var CTIconfig = CallToInteractConfig.Config;

        CTIconfig.Enabled = CTIEnableTog.isOn;
        CTIconfig.ShowTimeAfterNoHandPresent = TryParseNewStringToFloat(ref CTIconfig.ShowTimeAfterNoHandPresent, CTIShowTimer.text);
        CTIconfig.HideTimeAfterHandPresent = TryParseNewStringToFloat(ref CTIconfig.HideTimeAfterHandPresent, CTIHideTimer.text);

        if(CTIEndOnPresentTog.isOn)
        {
            CTIconfig.hideType = HideRequirement.PRESENT;
        }
        else if(CTIEndOnInteractionTog.isOn)
        {
            CTIconfig.hideType = HideRequirement.INTERACTION;
        }

        if (CTIFileDropdown.options.Count != 0)
        {
            CTIconfig.CurrentFileName = CTIFileDropdown.options[CTIFileDropdown.value].text;
        }

        SettingsConfig.UpdateConfig(config);
        CallToInteractConfig.UpdateConfig(CTIconfig);
        RestartSaveConfigTimer();

        DisplayCursorPreview();
        DisplayIntractionPreview();
    }

    protected override void CommitValuesToFile()
    {
        ConfigFileUtils.SaveAllConfigFiles();
    }
}
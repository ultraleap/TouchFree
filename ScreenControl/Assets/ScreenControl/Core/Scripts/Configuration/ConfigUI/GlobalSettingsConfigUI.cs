using System;
using System.Globalization;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace Ultraleap.ScreenControl.Core
{
    [System.Serializable]
    public class InteractionTypeElements
    {
        public string name;
        public GameObject[] typeSpecificElements;
    }

    public class GlobalSettingsConfigUI : ConfigUI
    {
        #region Bounds
        public const float CursorDeadzone_Min = 0f;
        public const float CursorDeadzone_Max = 0.015f;

        public const float HoverCursorStartTime_Min = 0.1f;
        public const float HoverCursorStartTime_Max = 2f;
        public const float HoverCursorCompleteTime_Min = 0.1f;
        public const float HoverCursorCompleteTime_Max = 2f;
        #endregion

        // UI elements
        [Header("Resolution")]
        public InputField resolutionWidth;
        public InputField resolutionHeight;

        [Header("Misc")]
        public Toggle scrollingOrDraggingTog;

        public Slider cursorDeadzoneSlider;

        public InputField HoverStartTime;
        public Slider HoverStartTimeSlider;
        public InputField HoverCompleteTime;
        public Slider HoverCompleteTimeSlider;

        [Header("Interaction Type")]
        public Toggle interactionTypeTogglePush;
        public Toggle interactionTypeTogglePinch;
        public Toggle interactionTypeToggleHover;

        [Header("Interaction Preview")]
        public GameObject pushPreview;
        public GameObject grabPreview;
        public GameObject hoverPreview;

        public GameObject resetToDefaultWarning;

        [Tooltip("List all Settings elements that relate to the interactionType. Names: 'Push', 'Grab', 'Hover'")]
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
            cursorDeadzoneSlider.minValue = CursorDeadzone_Min;
            cursorDeadzoneSlider.maxValue = CursorDeadzone_Max;

            HoverStartTimeSlider.minValue = HoverCursorStartTime_Min;
            HoverStartTimeSlider.maxValue = HoverCursorStartTime_Max;
            HoverCompleteTimeSlider.minValue = HoverCursorCompleteTime_Min;
            HoverCompleteTimeSlider.maxValue = HoverCursorCompleteTime_Max;
        }

        public void ResetToDefaultValues()
        {
            ConfigManager.InteractionConfig.SetAllValuesToDefault();
            ConfigManager.InteractionConfig.SaveConfig();
            LoadConfigValuesIntoFields();
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

            ConfigManager.GlobalSettings.ScreenWidth = width;
            ConfigManager.GlobalSettings.ScreenHeight = height;
            ConfigManager.GlobalSettings.ConfigWasUpdated();
            ConfigurationSetupController.Instance.RefreshConfigActive();
            OnValueChanged();
        }

        protected override void AddValueChangedListeners()
        {
            scrollingOrDraggingTog.onValueChanged.AddListener(OnValueChanged);
            cursorDeadzoneSlider.onValueChanged.AddListener(OnValueChanged);

            HoverStartTime.onEndEdit.AddListener(OnValueChanged);
            HoverStartTimeSlider.onValueChanged.AddListener(OnValueChanged);
            HoverCompleteTime.onEndEdit.AddListener(OnValueChanged);
            HoverCompleteTimeSlider.onValueChanged.AddListener(OnValueChanged);

            interactionTypeTogglePush.onValueChanged.AddListener(OnValueChanged);
            interactionTypeTogglePinch.onValueChanged.AddListener(OnValueChanged);
            interactionTypeToggleHover.onValueChanged.AddListener(OnValueChanged);
        }

        protected override void RemoveValueChangedListeners()
        {
            scrollingOrDraggingTog.onValueChanged.RemoveListener(OnValueChanged);
            cursorDeadzoneSlider.onValueChanged.RemoveListener(OnValueChanged);

            HoverStartTime.onEndEdit.RemoveListener(OnValueChanged);
            HoverStartTimeSlider.onValueChanged.RemoveListener(OnValueChanged);
            HoverCompleteTime.onEndEdit.RemoveListener(OnValueChanged);
            HoverCompleteTimeSlider.onValueChanged.RemoveListener(OnValueChanged);

            interactionTypeTogglePush.onValueChanged.RemoveListener(OnValueChanged);
            interactionTypeTogglePinch.onValueChanged.RemoveListener(OnValueChanged);
            interactionTypeToggleHover.onValueChanged.RemoveListener(OnValueChanged);
        }

        protected override void LoadConfigValuesIntoFields()
        {
            resolutionWidth.text = Screen.currentResolution.width.ToString();
            resolutionHeight.text = Screen.currentResolution.height.ToString();

            cursorDeadzoneSlider.SetValueWithoutNotify(ConfigManager.InteractionConfig.DeadzoneRadius);

            scrollingOrDraggingTog.SetIsOnWithoutNotify(ConfigManager.InteractionConfig.UseScrollingOrDragging);

            HoverStartTime.SetTextWithoutNotify(ConfigManager.InteractionConfig.HoverAndHold.HoverStartTimeS.ToString("#0.00#"));
            HoverStartTimeSlider.SetValueWithoutNotify(ConfigManager.InteractionConfig.HoverAndHold.HoverStartTimeS);
            HoverCompleteTime.SetTextWithoutNotify(ConfigManager.InteractionConfig.HoverAndHold.HoverCompleteTimeS.ToString("#0.00#"));
            HoverCompleteTimeSlider.SetValueWithoutNotify(ConfigManager.InteractionConfig.HoverAndHold.HoverCompleteTimeS);

            // TODO: set the currently selected interaction here
            interactionTypeTogglePush.SetIsOnWithoutNotify(false);
            interactionTypeTogglePinch.SetIsOnWithoutNotify(false);
            interactionTypeToggleHover.SetIsOnWithoutNotify(false);

            DisplayIntractionPreview();
        }

        protected override void ValidateValues()
        {
            var deadzoneRadius = Mathf.Clamp(cursorDeadzoneSlider.value, CursorDeadzone_Min, CursorDeadzone_Max);
            cursorDeadzoneSlider.SetValueWithoutNotify(deadzoneRadius);

            var hoverStartTime = Mathf.Clamp(HoverStartTimeSlider.value, HoverCursorStartTime_Min, HoverCursorStartTime_Max);
            HoverStartTime.SetTextWithoutNotify(hoverStartTime.ToString("#0.00#"));
            HoverStartTimeSlider.SetValueWithoutNotify(hoverStartTime);

            var hoverCompleteTime = Mathf.Clamp(HoverCompleteTimeSlider.value, HoverCursorCompleteTime_Min, HoverCursorCompleteTime_Max);
            HoverCompleteTime.SetTextWithoutNotify(hoverCompleteTime.ToString("#0.00#"));
            HoverCompleteTimeSlider.SetValueWithoutNotify(hoverCompleteTime);

            SaveValuesToConfig();
        }

        void DisplayIntractionPreview()
        {
            pushPreview.SetActive(false);
            grabPreview.SetActive(false);
            hoverPreview.SetActive(false);

            if(interactionTypeTogglePush.isOn)
            {
                pushPreview.SetActive(true);
                HandleSpecificElements("Push");
            }
            else if(interactionTypeTogglePinch.isOn)
            {
                grabPreview.SetActive(true);
                HandleSpecificElements("Grab");
            }
            else if(interactionTypeToggleHover.isOn)
            {
                hoverPreview.SetActive(true);
                HandleSpecificElements("Hover");
            }
        }

        void HandleSpecificElements(string _name)
        {
            InteractionTypeElements matchingGroup = null;

            foreach (var group in interactionTypeElements)
            {
                if (group.name == _name)
                {
                    matchingGroup = group;
                }
                else
                {
                    foreach (var element in group.typeSpecificElements)
                    {
                        element.SetActive(false);
                    }
                }
            }

            if(matchingGroup != null)
            {
                foreach (var element in matchingGroup.typeSpecificElements)
                {
                    element.SetActive(true);
                }
            }
        }

        protected override void SaveValuesToConfig()
        {
            ConfigManager.InteractionConfig.DeadzoneRadius = cursorDeadzoneSlider.value;
            ConfigManager.InteractionConfig.UseScrollingOrDragging = scrollingOrDraggingTog.isOn;
            ConfigManager.InteractionConfig.HoverAndHold.HoverStartTimeS = HoverStartTimeSlider.value;
            ConfigManager.InteractionConfig.HoverAndHold.HoverCompleteTimeS = HoverCompleteTimeSlider.value;

            // TODO: Set the current interaction here

            ConfigManager.InteractionConfig.ConfigWasUpdated();
            RestartSaveConfigTimer();
            DisplayIntractionPreview();
        }

        protected override void CommitValuesToFile()
        {
            ConfigManager.SaveAllConfigs();
        }
    }
}
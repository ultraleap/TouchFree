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
        public CanvasGroup[] typeSpecificElements;
    }

    public class GlobalSettingsConfigUI : ConfigUI
    {
        #region Bounds
        public const float CursorVerticalOffset_Min = -0.1f;
        public const float CursorVerticalOffset_Max = 0.1f;

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
            cursorDeadzoneSlider.minValue = CursorDeadzone_Min;
            cursorDeadzoneSlider.maxValue = CursorDeadzone_Max;

            cursorVerticalOffsetSlider.minValue = ScreenControlUtility.ToDisplayUnits(CursorVerticalOffset_Min);
            cursorVerticalOffsetSlider.maxValue = ScreenControlUtility.ToDisplayUnits(CursorVerticalOffset_Max);

            HoverCursorStartTimeSlider.minValue = HoverCursorStartTime_Min;
            HoverCursorStartTimeSlider.maxValue = HoverCursorStartTime_Max;
            HoverCursorCompleteTimeSlider.minValue = HoverCursorCompleteTime_Min;
            HoverCursorCompleteTimeSlider.maxValue = HoverCursorCompleteTime_Max;
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
        }

        protected override void RemoveValueChangedListeners()
        {
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
        }

        protected override void LoadConfigValuesIntoFields()
        {
            resolutionWidth.text = Screen.currentResolution.width.ToString();
            resolutionHeight.text = Screen.currentResolution.height.ToString();

            cursorDeadzoneSlider.SetValueWithoutNotify(ConfigManager.InteractionConfig.Generic.DeadzoneRadius);
            cursorVerticalOffset.SetTextWithoutNotify(ScreenControlUtility.ToDisplayUnits(ConfigManager.InteractionConfig.Generic.CursorVerticalOffset).ToString("#0.00#"));
            cursorVerticalOffsetSlider.SetValueWithoutNotify(ScreenControlUtility.ToDisplayUnits(ConfigManager.InteractionConfig.Generic.CursorVerticalOffset));

            HoverCursorStartTime.SetTextWithoutNotify(ConfigManager.InteractionConfig.HoverAndHold.HoverCursorStartTimeS.ToString("#0.00#"));
            HoverCursorStartTimeSlider.SetValueWithoutNotify(ConfigManager.InteractionConfig.HoverAndHold.HoverCursorStartTimeS);
            HoverCursorCompleteTime.SetTextWithoutNotify(ConfigManager.InteractionConfig.HoverAndHold.HoverCursorCompleteTimeS.ToString("#0.00#"));
            HoverCursorCompleteTimeSlider.SetValueWithoutNotify(ConfigManager.InteractionConfig.HoverAndHold.HoverCursorCompleteTimeS);

            interactionTypeTogglePush.SetIsOnWithoutNotify(false);
            interactionTypeTogglePoke.SetIsOnWithoutNotify(false);
            interactionTypeTogglePinch.SetIsOnWithoutNotify(false);
            interactionTypeToggleHover.SetIsOnWithoutNotify(false);

            hoverEventsWarning.SetActive(sendHoverEventsTog.isOn);

            DisplayCursorPreview();
            DisplayIntractionPreview();
        }

        protected override void ValidateValues()
        {
            var deadzoneRadius = Mathf.Clamp(cursorDeadzoneSlider.value, CursorDeadzone_Min, CursorDeadzone_Max);
            cursorDeadzoneSlider.SetValueWithoutNotify(deadzoneRadius);

            var verticalOffset = Mathf.Clamp(cursorVerticalOffsetSlider.value,
                ScreenControlUtility.ToDisplayUnits(CursorVerticalOffset_Min),
                ScreenControlUtility.ToDisplayUnits(CursorVerticalOffset_Max));

            cursorVerticalOffsetSlider.SetValueWithoutNotify(verticalOffset);
            cursorVerticalOffset.SetTextWithoutNotify(verticalOffset.ToString("#0.00#"));

            var hoverStartTime = Mathf.Clamp(HoverCursorStartTimeSlider.value, HoverCursorStartTime_Min, HoverCursorStartTime_Max);
            HoverCursorStartTime.SetTextWithoutNotify(hoverStartTime.ToString("#0.00#"));
            HoverCursorStartTimeSlider.SetValueWithoutNotify(hoverStartTime);

            var hoverCompleteTime = Mathf.Clamp(HoverCursorCompleteTimeSlider.value, HoverCursorCompleteTime_Min, HoverCursorCompleteTime_Max);
            HoverCursorCompleteTime.SetTextWithoutNotify(hoverCompleteTime.ToString("#0.00#"));
            HoverCursorCompleteTimeSlider.SetValueWithoutNotify(hoverCompleteTime);

            hoverEventsWarning.SetActive(sendHoverEventsTog.isOn);
        }

        string customCursorDotFillColour;
        string customCursorDotBorderColour;
        string customCursorRingColour;

        float customCursorDotFillOpacity;
        float customCursorDotBorderOpacity;
        float customCursorRingOpacity;

        void DisplayCursorPreview()
        {
            pushCursorPreview.SetActive(false);
            grabCursorPreview.SetActive(false);
            hoverCursorPreview.SetActive(false);
        }

        void DisplayIntractionPreview()
        {
            pushPreview.SetActive(false);
            pokePreview.SetActive(false);
            grabPreview.SetActive(false);
            hoverPreview.SetActive(false);
        }

        protected override void SaveValuesToConfig()
        {
            ConfigManager.InteractionConfig.ConfigWasUpdated();
            RestartSaveConfigTimer();

            DisplayCursorPreview();
            DisplayIntractionPreview();
        }

        protected override void CommitValuesToFile()
        {
            ConfigFileUtils.SaveAllConfigFiles();
        }
    }
}
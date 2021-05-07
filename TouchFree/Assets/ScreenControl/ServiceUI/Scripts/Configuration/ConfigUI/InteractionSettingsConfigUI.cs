using System;
using System.Globalization;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace Ultraleap.ScreenControl.Core
{
    public class InteractionSettingsConfigUI : ConfigUI
    {
        #region Bounds
        public const float CursorDeadzone_Min = 0f;
        public const float CursorDeadzone_Max = 0.015f;

        public const float HoverCursorStartTime_Min = 0.1f;
        public const float HoverCursorStartTime_Max = 2f;
        #endregion

        // UI elements
        [Header("Misc")]
        public Toggle scrollingOrDraggingTog;

        public Slider cursorDeadzoneSlider;

        public InputField HoverStartTime;
        public Slider HoverStartTimeSlider;

        [Header("Interaction Type")]
        public Toggle interactionTypeTogglePush;
        public Toggle interactionTypeToggleHover;

        [Header("Interaction Preview")]
        public GameObject pushPreview;
        public GameObject hoverPreview;

        [Space]
        public GameObject resetToDefaultWarning;

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
        }

        public void ResetToDefaultValues()
        {
            //ConfigManager.InteractionConfig.SetAllValuesToDefault();
            //ConfigManager.InteractionConfig.ConfigWasUpdated();
            //ConfigManager.InteractionConfig.SaveConfig();
            LoadConfigValuesIntoFields();
        }

        protected override void AddValueChangedListeners()
        {
            scrollingOrDraggingTog.onValueChanged.AddListener(OnValueChanged);
            cursorDeadzoneSlider.onValueChanged.AddListener(OnValueChanged);

            HoverStartTime.onEndEdit.AddListener(OnValueChanged);
            HoverStartTimeSlider.onValueChanged.AddListener(OnValueChanged);

            interactionTypeTogglePush.onValueChanged.AddListener(OnValueChanged);
            interactionTypeToggleHover.onValueChanged.AddListener(OnValueChanged);
        }

        protected override void RemoveValueChangedListeners()
        {
            scrollingOrDraggingTog.onValueChanged.RemoveListener(OnValueChanged);
            cursorDeadzoneSlider.onValueChanged.RemoveListener(OnValueChanged);

            HoverStartTime.onEndEdit.RemoveListener(OnValueChanged);
            HoverStartTimeSlider.onValueChanged.RemoveListener(OnValueChanged);

            interactionTypeTogglePush.onValueChanged.RemoveListener(OnValueChanged);
            interactionTypeToggleHover.onValueChanged.RemoveListener(OnValueChanged);
        }

        protected override void LoadConfigValuesIntoFields()
        {
            //cursorDeadzoneSlider.SetValueWithoutNotify(ConfigManager.InteractionConfig.DeadzoneRadius);

            //scrollingOrDraggingTog.SetIsOnWithoutNotify(ConfigManager.InteractionConfig.UseScrollingOrDragging);

            //HoverStartTime.SetTextWithoutNotify(ConfigManager.InteractionConfig.HoverAndHold.HoverStartTimeS.ToString("#0.00#"));
            //HoverStartTimeSlider.SetValueWithoutNotify(ConfigManager.InteractionConfig.HoverAndHold.HoverStartTimeS);

            interactionTypeTogglePush.SetIsOnWithoutNotify(false);
            interactionTypeToggleHover.SetIsOnWithoutNotify(false);
            //switch (ConfigManager.InteractionConfig.InteractionType)
            //{
            //    case ScreenControlTypes.InteractionType.HOVER:
            //        interactionTypeToggleHover.SetIsOnWithoutNotify(true);
            //        break;
            //    case ScreenControlTypes.InteractionType.PUSH:
            //        interactionTypeTogglePush.SetIsOnWithoutNotify(true);
            //        break;
            //}

            DisplayIntractionPreview();
        }

        protected override void ValidateValues()
        {
            var deadzoneRadius = Mathf.Clamp(cursorDeadzoneSlider.value, CursorDeadzone_Min, CursorDeadzone_Max);
            cursorDeadzoneSlider.SetValueWithoutNotify(deadzoneRadius);

            var hoverStartTime = Mathf.Clamp(HoverStartTimeSlider.value, HoverCursorStartTime_Min, HoverCursorStartTime_Max);
            HoverStartTime.SetTextWithoutNotify(hoverStartTime.ToString("#0.00#"));
            HoverStartTimeSlider.SetValueWithoutNotify(hoverStartTime);

            //if (interactionTypeTogglePush.isOn)
            //{
            //    ConfigManager.InteractionConfig.InteractionType = ScreenControlTypes.InteractionType.PUSH;
            //}
            //else if (interactionTypeToggleHover.isOn)
            //{
            //    ConfigManager.InteractionConfig.InteractionType = ScreenControlTypes.InteractionType.HOVER;
            //}
        }

        void DisplayIntractionPreview()
        {
            pushPreview.SetActive(false);
            hoverPreview.SetActive(false);

            //switch (ConfigManager.InteractionConfig.InteractionType)
            //{
            //    case ScreenControlTypes.InteractionType.HOVER:
            //        hoverPreview.SetActive(true);
            //        break;
            //    case ScreenControlTypes.InteractionType.PUSH:
            //        pushPreview.SetActive(true);
            //        break;
            //}

            //HandleSpecificElements(ConfigManager.InteractionConfig.InteractionType);
        }

        void HandleSpecificElements(/*ScreenControlTypes.InteractionType _interactionType*/)
        {
            //InteractionTypeElements matchingGroup = null;

            //foreach (var group in interactionTypeElements)
            //{
            //    if (group.interactionType == _interactionType)
            //    {
            //        matchingGroup = group;
            //    }
            //    else
            //    {
            //        foreach (var element in group.typeSpecificElements)
            //        {
            //            element.SetActive(false);
            //        }
            //    }
            //}

            //if (matchingGroup != null)
            //{
            //    foreach (var element in matchingGroup.typeSpecificElements)
            //    {
            //        element.SetActive(true);
            //    }
            //}
        }

        protected override void SaveValuesToConfig()
        {
            //ConfigManager.InteractionConfig.DeadzoneRadius = cursorDeadzoneSlider.value;
            //ConfigManager.InteractionConfig.UseScrollingOrDragging = scrollingOrDraggingTog.isOn;
            //ConfigManager.InteractionConfig.HoverAndHold.HoverStartTimeS = HoverStartTimeSlider.value;

            //if (interactionTypeTogglePush.isOn)
            //{
            //    ConfigManager.InteractionConfig.InteractionType = ScreenControlTypes.InteractionType.PUSH;
            //}
            //else if (interactionTypeToggleHover.isOn)
            //{
            //    ConfigManager.InteractionConfig.InteractionType = ScreenControlTypes.InteractionType.HOVER;
            //}

            //ConfigManager.InteractionConfig.ConfigWasUpdated();
            //ConfigManager.InteractionConfig.SaveConfig();
            //DisplayIntractionPreview();
        }

        protected override void CommitValuesToFile()
        {
            //ConfigManager.SaveAllConfigs();
        }
    }
}
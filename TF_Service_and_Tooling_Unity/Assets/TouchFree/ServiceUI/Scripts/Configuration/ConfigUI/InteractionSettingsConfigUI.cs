using System.Globalization;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Ultraleap.TouchFree.ServiceShared;

namespace Ultraleap.TouchFree.ServiceUI
{
    [System.Serializable]
    public class InteractionTypeElements
    {
        public InteractionType interactionType;
        public GameObject[] typeSpecificElements;
    }

    public class InteractionSettingsConfigUI : ConfigUI
    {
        #region Bounds
        public const float CursorDeadzone_Min = 0f;
        public const float CursorDeadzone_Max = 0.015f;

        public const float HoverCursorStartTime_Min = 0.1f;
        public const float HoverCursorStartTime_Max = 2f;
        public const float HoverCursorCompleteTime_Min = 0.1f;
        public const float HoverCursorCompleteTime_Max = 2f;

        public const float TouchPlaneDistance_Min = 0f;
        public const float TouchPlaneDistance_Max = 20f;
        public const float TouchPlaneStartDistance_Min = 0f;
        public const float TouchPlaneStartDistance_Max = 20f;
        #endregion

        // UI elements
        [Header("Misc")]
        public Toggle scrollingOrDraggingTog;

        public Slider cursorDeadzoneSlider;

        [Header("Hover&Hold")]
        public InputField HoverStartTime;
        public Slider HoverStartTimeSlider;
        public InputField HoverCompleteTime;
        public Slider HoverCompleteTimeSlider;

        [Header("TouchPlane")]
        public InputField TouchPlaneDistance;
        public Slider TouchPlaneDistanceSlider;
        public InputField TouchPlaneStartDistance;
        public Slider TouchPlaneStartDistanceSlider;

        [Header("Interaction Type")]
        public Toggle interactionTypeTogglePush;
        public Toggle interactionTypeTogglePinch;
        public Toggle interactionTypeToggleHover;
        public Toggle interactionTypeToggleTouchPlane;

        [Header("Interaction Preview")]
        public GameObject pushPreview;
        public GameObject grabPreview;
        public GameObject hoverPreview;

        [Space]
        public GameObject resetToDefaultWarning;

        [Space, Tooltip("List all Settings elements that relate to the interactionType.")]
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

            TouchPlaneDistanceSlider.minValue = TouchPlaneDistance_Min;
            TouchPlaneDistanceSlider.maxValue = TouchPlaneDistance_Max;
            TouchPlaneStartDistanceSlider.minValue = TouchPlaneStartDistance_Min;
            TouchPlaneStartDistanceSlider.maxValue = TouchPlaneStartDistance_Max;
        }

        public void ResetToDefaultValues()
        {
            ConfigManager.InteractionConfig.SetAllValuesToDefault();
            ConfigManager.InteractionConfig.ConfigWasUpdated();
            ConfigManager.InteractionConfig.SaveConfig();
            LoadConfigValuesIntoFields();
        }

        protected override void AddValueChangedListeners()
        {
            scrollingOrDraggingTog.onValueChanged.AddListener(OnValueChanged);
            cursorDeadzoneSlider.onValueChanged.AddListener(OnValueChanged);

            HoverStartTime.onEndEdit.AddListener(OnValueChanged);
            HoverStartTimeSlider.onValueChanged.AddListener(OnValueChanged);
            HoverCompleteTime.onEndEdit.AddListener(OnValueChanged);
            HoverCompleteTimeSlider.onValueChanged.AddListener(OnValueChanged);

            TouchPlaneDistance.onEndEdit.AddListener(OnValueChanged);
            TouchPlaneDistanceSlider.onValueChanged.AddListener(OnValueChanged);
            TouchPlaneStartDistance.onEndEdit.AddListener(OnValueChanged);
            TouchPlaneStartDistanceSlider.onValueChanged.AddListener(OnValueChanged);

            interactionTypeTogglePush.onValueChanged.AddListener(OnValueChanged);
            interactionTypeTogglePinch.onValueChanged.AddListener(OnValueChanged);
            interactionTypeToggleHover.onValueChanged.AddListener(OnValueChanged);
            interactionTypeToggleTouchPlane.onValueChanged.AddListener(OnValueChanged);
        }

        protected override void RemoveValueChangedListeners()
        {
            scrollingOrDraggingTog.onValueChanged.RemoveListener(OnValueChanged);
            cursorDeadzoneSlider.onValueChanged.RemoveListener(OnValueChanged);

            HoverStartTime.onEndEdit.RemoveListener(OnValueChanged);
            HoverStartTimeSlider.onValueChanged.RemoveListener(OnValueChanged);
            HoverCompleteTime.onEndEdit.RemoveListener(OnValueChanged);
            HoverCompleteTimeSlider.onValueChanged.RemoveListener(OnValueChanged);

            TouchPlaneDistance.onEndEdit.RemoveListener(OnValueChanged);
            TouchPlaneDistanceSlider.onValueChanged.RemoveListener(OnValueChanged);
            TouchPlaneStartDistance.onEndEdit.RemoveListener(OnValueChanged);
            TouchPlaneStartDistanceSlider.onValueChanged.RemoveListener(OnValueChanged);

            interactionTypeTogglePush.onValueChanged.RemoveListener(OnValueChanged);
            interactionTypeTogglePinch.onValueChanged.RemoveListener(OnValueChanged);
            interactionTypeToggleHover.onValueChanged.RemoveListener(OnValueChanged);
            interactionTypeToggleTouchPlane.onValueChanged.RemoveListener(OnValueChanged);
        }

        protected override void LoadConfigValuesIntoFields()
        {
            cursorDeadzoneSlider.SetValueWithoutNotify(ConfigManager.InteractionConfig.DeadzoneRadius);

            scrollingOrDraggingTog.SetIsOnWithoutNotify(ConfigManager.InteractionConfig.UseScrollingOrDragging);

            HoverStartTime.SetTextWithoutNotify(ConfigManager.InteractionConfig.HoverAndHold.HoverStartTimeS.ToString("#0.00#"));
            HoverStartTimeSlider.SetValueWithoutNotify(ConfigManager.InteractionConfig.HoverAndHold.HoverStartTimeS);
            HoverCompleteTime.SetTextWithoutNotify(ConfigManager.InteractionConfig.HoverAndHold.HoverCompleteTimeS.ToString("#0.00#"));
            HoverCompleteTimeSlider.SetValueWithoutNotify(ConfigManager.InteractionConfig.HoverAndHold.HoverCompleteTimeS);

            TouchPlaneDistance.SetTextWithoutNotify(ConfigManager.InteractionConfig.TouchPlane.TouchPlaneActivationDistanceCM.ToString("#0.00#"));
            TouchPlaneDistanceSlider.SetValueWithoutNotify(ConfigManager.InteractionConfig.TouchPlane.TouchPlaneActivationDistanceCM);
            TouchPlaneStartDistance.SetTextWithoutNotify(ConfigManager.InteractionConfig.TouchPlane.TouchPlaneStartDistanceCM.ToString("#0.00#"));
            TouchPlaneStartDistanceSlider.SetValueWithoutNotify(ConfigManager.InteractionConfig.TouchPlane.TouchPlaneStartDistanceCM);

            interactionTypeTogglePush.SetIsOnWithoutNotify(false);
            interactionTypeTogglePinch.SetIsOnWithoutNotify(false);
            interactionTypeToggleHover.SetIsOnWithoutNotify(false);
            interactionTypeToggleTouchPlane.SetIsOnWithoutNotify(false);
            switch (ConfigManager.InteractionConfig.InteractionType)
            {
                case InteractionType.GRAB:
                    interactionTypeTogglePinch.SetIsOnWithoutNotify(true);
                    break;
                case InteractionType.HOVER:
                    interactionTypeToggleHover.SetIsOnWithoutNotify(true);
                    break;
                case InteractionType.PUSH:
                    interactionTypeTogglePush.SetIsOnWithoutNotify(true);
                    break;
                case InteractionType.TOUCHPLANE:
                    interactionTypeToggleTouchPlane.SetIsOnWithoutNotify(true);
                    break;
            }

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

            var touchPlaneDistance = Mathf.Clamp(TouchPlaneDistanceSlider.value, TouchPlaneDistance_Min, TouchPlaneDistance_Max);
            TouchPlaneDistance.SetTextWithoutNotify(touchPlaneDistance.ToString("#0.00#"));
            TouchPlaneDistanceSlider.SetValueWithoutNotify(touchPlaneDistance);

            var touchPlaneStartDistance = Mathf.Clamp(TouchPlaneStartDistanceSlider.value, TouchPlaneStartDistance_Min, TouchPlaneStartDistance_Max);
            TouchPlaneStartDistance.SetTextWithoutNotify(touchPlaneStartDistance.ToString("#0.00#"));
            TouchPlaneStartDistanceSlider.SetValueWithoutNotify(touchPlaneStartDistance);
        }

        void DisplayIntractionPreview()
        {
            pushPreview.SetActive(false);
            grabPreview.SetActive(false);
            hoverPreview.SetActive(false);

            switch (ConfigManager.InteractionConfig.InteractionType)
            {
                case InteractionType.GRAB:
                    grabPreview.SetActive(true);
                    break;
                case InteractionType.HOVER:
                    hoverPreview.SetActive(true);
                    break;
                case InteractionType.PUSH:
                case InteractionType.TOUCHPLANE:
                    pushPreview.SetActive(true);
                    break;
            }

            HandleSpecificElements(ConfigManager.InteractionConfig.InteractionType);
        }

        void HandleSpecificElements(InteractionType _interactionType)
        {
            InteractionTypeElements matchingGroup = null;

            foreach (var group in interactionTypeElements)
            {
                if (group.interactionType == _interactionType)
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

            if (matchingGroup != null)
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
            ConfigManager.InteractionConfig.TouchPlane.TouchPlaneActivationDistanceCM = TouchPlaneDistanceSlider.value;
            ConfigManager.InteractionConfig.TouchPlane.TouchPlaneStartDistanceCM = TouchPlaneStartDistanceSlider.value;

            if (interactionTypeTogglePush.isOn)
            {
                ConfigManager.InteractionConfig.InteractionType = InteractionType.PUSH;
            }
            else if (interactionTypeTogglePinch.isOn)
            {
                ConfigManager.InteractionConfig.InteractionType = InteractionType.GRAB;
            }
            else if (interactionTypeToggleHover.isOn)
            {
                ConfigManager.InteractionConfig.InteractionType = InteractionType.HOVER;
            }
            else if (interactionTypeToggleTouchPlane.isOn)
            {
                ConfigManager.InteractionConfig.InteractionType = InteractionType.TOUCHPLANE;
            }

            ConfigManager.InteractionConfig.ConfigWasUpdated();
            ConfigManager.InteractionConfig.SaveConfig();
            DisplayIntractionPreview();
        }

        protected override void CommitValuesToFile()
        {
            ConfigManager.SaveAllConfigs();
        }
    }
}
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
        #endregion

        // UI elements
        [Header("Misc")]
        public Toggle scrollingOrDraggingTog;

        public Slider cursorDeadzoneSlider;

        [Header("Hover&Hold")]
        public SliderInputFieldCombiner hoverStartInputSlider;
        public SliderInputFieldCombiner hoverCompleteInputSlider;

        [Header("TouchPlane")]
        public SliderInputFieldCombiner touchPlaneDistanceInputSlider;

        public Toggle trackingBoneNearestToggle;
        public Toggle trackingBoneIndexTipToggle;

        [Header("Interaction Type")]
        public Toggle interactionTypeTogglePush;
        public Toggle interactionTypeTogglePinch;
        public Toggle interactionTypeToggleHover;
        public Toggle interactionTypeToggleTouchPlane;

        [Header("Interaction Preview")]
        public GameObject airPushPreview;
        public GameObject touchPlanePreview;
        public GameObject hoverPreview;

        [Header("InteractionZone")]
        public Toggle EnableInteractionZoneToggle;
        public InputField InteractionMinDistanceField;
        public InputField InteractionMaxDistanceField;
        public GameObject[] InteractionZoneSettingsToHide;

        [Space]
        public GameObject resetToDefaultWarning;

        [Space, Tooltip("List all Settings elements that relate to the interactionType.")]
        public InteractionTypeElements[] interactionTypeElements;

        private void Awake()
        {
            InitialiseUI();
            resetToDefaultWarning.SetActive(false);
        }

        void InitialiseUI()
        {
            cursorDeadzoneSlider.minValue = CursorDeadzone_Min;
            cursorDeadzoneSlider.maxValue = CursorDeadzone_Max;
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

            hoverStartInputSlider.onValueChanged.AddListener(OnValueChanged);
            hoverCompleteInputSlider.onValueChanged.AddListener(OnValueChanged);

            touchPlaneDistanceInputSlider.onValueChanged.AddListener(OnValueChanged);

            trackingBoneNearestToggle.onValueChanged.AddListener(OnValueChanged);
            trackingBoneIndexTipToggle.onValueChanged.AddListener(OnValueChanged);

            interactionTypeTogglePush.onValueChanged.AddListener(OnValueChanged);
            interactionTypeTogglePinch.onValueChanged.AddListener(OnValueChanged);
            interactionTypeToggleHover.onValueChanged.AddListener(OnValueChanged);
            interactionTypeToggleTouchPlane.onValueChanged.AddListener(OnValueChanged);

            // Interaction Zone Events
            EnableInteractionZoneToggle.onValueChanged.AddListener(OnValueChanged);
            EnableInteractionZoneToggle.onValueChanged.AddListener(ShowHideInteractionZoneControls);
            InteractionMinDistanceField.onEndEdit.AddListener(OnValueChanged);
            InteractionMaxDistanceField.onEndEdit.AddListener(OnValueChanged);
        }

        protected override void RemoveValueChangedListeners()
        {
            scrollingOrDraggingTog.onValueChanged.RemoveListener(OnValueChanged);
            cursorDeadzoneSlider.onValueChanged.RemoveListener(OnValueChanged);

            hoverStartInputSlider.onValueChanged.RemoveListener(OnValueChanged);
            hoverCompleteInputSlider.onValueChanged.RemoveListener(OnValueChanged);

            touchPlaneDistanceInputSlider.onValueChanged.RemoveListener(OnValueChanged);

            trackingBoneNearestToggle.onValueChanged.RemoveListener(OnValueChanged);
            trackingBoneIndexTipToggle.onValueChanged.RemoveListener(OnValueChanged);

            interactionTypeTogglePush.onValueChanged.RemoveListener(OnValueChanged);
            interactionTypeTogglePinch.onValueChanged.RemoveListener(OnValueChanged);
            interactionTypeToggleHover.onValueChanged.RemoveListener(OnValueChanged);
            interactionTypeToggleTouchPlane.onValueChanged.RemoveListener(OnValueChanged);

            // Interaction Zone Events
            EnableInteractionZoneToggle.onValueChanged.RemoveListener(OnValueChanged);
            EnableInteractionZoneToggle.onValueChanged.RemoveListener(ShowHideInteractionZoneControls);
            InteractionMinDistanceField.onEndEdit.RemoveListener(OnValueChanged);
            InteractionMaxDistanceField.onEndEdit.RemoveListener(OnValueChanged);
        }

        protected override void LoadConfigValuesIntoFields()
        {
            cursorDeadzoneSlider.SetValueWithoutNotify(ConfigManager.InteractionConfig.DeadzoneRadius);

            scrollingOrDraggingTog.SetIsOnWithoutNotify(ConfigManager.InteractionConfig.UseScrollingOrDragging);

            hoverStartInputSlider.SetValueWithoutNotify(ConfigManager.InteractionConfig.HoverAndHold.HoverStartTimeS);
            hoverCompleteInputSlider.SetValueWithoutNotify(ConfigManager.InteractionConfig.HoverAndHold.HoverCompleteTimeS);

            touchPlaneDistanceInputSlider.SetValueWithoutNotify(ConfigManager.InteractionConfig.TouchPlane.TouchPlaneActivationDistanceCM);

            trackingBoneNearestToggle.SetIsOnWithoutNotify(false);
            trackingBoneIndexTipToggle.SetIsOnWithoutNotify(false);
            switch (ConfigManager.InteractionConfig.TouchPlane.TouchPlaneTrackedPosition)
            {
                case TrackedPosition.INDEX_TIP:
                    trackingBoneIndexTipToggle.SetIsOnWithoutNotify(true);
                    break;
                default:
                case TrackedPosition.NEAREST:
                    trackingBoneNearestToggle.SetIsOnWithoutNotify(true);
                    break;
            }

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

            // Interaction Zone settings
            EnableInteractionZoneToggle.SetIsOnWithoutNotify(ConfigManager.InteractionConfig.InteractionZoneEnabled);
            InteractionMinDistanceField.SetTextWithoutNotify(ConfigManager.InteractionConfig.InteractionMinDistanceCm.ToString());
            InteractionMaxDistanceField.SetTextWithoutNotify(ConfigManager.InteractionConfig.InteractionMaxDistanceCm.ToString());

            ShowHideInteractionZoneControls(ConfigManager.InteractionConfig.InteractionZoneEnabled);
            DisplayIntractionPreview();
        }

        protected override void ValidateValues()
        {
            var deadzoneRadius = Mathf.Clamp(cursorDeadzoneSlider.value, CursorDeadzone_Min, CursorDeadzone_Max);
            cursorDeadzoneSlider.SetValueWithoutNotify(deadzoneRadius);

            InteractionMinDistanceField.SetTextWithoutNotify(
                ServiceUtility.TryParseNewStringToFloat(ConfigManager.InteractionConfig.InteractionMinDistanceCm, InteractionMinDistanceField.text).ToString());
            InteractionMaxDistanceField.SetTextWithoutNotify(
                ServiceUtility.TryParseNewStringToFloat(ConfigManager.InteractionConfig.InteractionMaxDistanceCm, InteractionMaxDistanceField.text).ToString());
        }

        void DisplayIntractionPreview()
        {
            airPushPreview.SetActive(false);
            touchPlanePreview.SetActive(false);
            hoverPreview.SetActive(false);

            switch (ConfigManager.InteractionConfig.InteractionType)
            {
                case InteractionType.GRAB:
                    break;
                case InteractionType.HOVER:
                    hoverPreview.SetActive(true);
                    break;
                case InteractionType.PUSH:
                    airPushPreview.SetActive(true);
                    break;
                case InteractionType.TOUCHPLANE:
                    touchPlanePreview.SetActive(true);
                    break;
            }

            HandleSpecificElements(ConfigManager.InteractionConfig.InteractionType);
        }

        protected void ShowHideInteractionZoneControls(bool _state)
        {
            foreach (GameObject control in InteractionZoneSettingsToHide)
            {
                control.SetActive(_state);
            }
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
            ConfigManager.InteractionConfig.HoverAndHold.HoverStartTimeS = hoverStartInputSlider.Value;
            ConfigManager.InteractionConfig.HoverAndHold.HoverCompleteTimeS = hoverCompleteInputSlider.Value;
            ConfigManager.InteractionConfig.TouchPlane.TouchPlaneActivationDistanceCM = touchPlaneDistanceInputSlider.Value;

            if(trackingBoneIndexTipToggle.isOn)
            {
                ConfigManager.InteractionConfig.TouchPlane.TouchPlaneTrackedPosition = TrackedPosition.INDEX_TIP;
            }
            else
            {
                ConfigManager.InteractionConfig.TouchPlane.TouchPlaneTrackedPosition = TrackedPosition.NEAREST;
            }

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

            ConfigManager.InteractionConfig.InteractionZoneEnabled = EnableInteractionZoneToggle.isOn;

            ConfigManager.InteractionConfig.InteractionMinDistanceCm =
                ServiceUtility.TryParseNewStringToFloat(
                    ConfigManager.InteractionConfig.InteractionMinDistanceCm,
                    InteractionMinDistanceField.text);

            ConfigManager.InteractionConfig.InteractionMaxDistanceCm =
                ServiceUtility.TryParseNewStringToFloat(
                    ConfigManager.InteractionConfig.InteractionMaxDistanceCm,
                    InteractionMaxDistanceField.text);

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
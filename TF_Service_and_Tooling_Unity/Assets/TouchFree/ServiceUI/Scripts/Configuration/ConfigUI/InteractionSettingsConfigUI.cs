﻿using UnityEngine;
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

            HoverStartTimeSlider.minValue = HoverCursorStartTime_Min;
            HoverStartTimeSlider.maxValue = HoverCursorStartTime_Max;
            HoverCompleteTimeSlider.minValue = HoverCursorCompleteTime_Min;
            HoverCompleteTimeSlider.maxValue = HoverCursorCompleteTime_Max;

            TouchPlaneDistanceSlider.minValue = TouchPlaneDistance_Min;
            TouchPlaneDistanceSlider.maxValue = TouchPlaneDistance_Max;
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
            trackingBoneNearestToggle.onValueChanged.AddListener(OnValueChanged);
            trackingBoneIndexTipToggle.onValueChanged.AddListener(OnValueChanged);

            interactionTypeTogglePush.onValueChanged.AddListener(OnValueChanged);
            interactionTypeTogglePinch.onValueChanged.AddListener(OnValueChanged);
            interactionTypeToggleHover.onValueChanged.AddListener(OnValueChanged);
            interactionTypeToggleTouchPlane.onValueChanged.AddListener(OnValueChanged);

            // Interaction Zone Events
            EnableInteractionZoneToggle.onValueChanged.AddListener(OnValueChanged);
            EnableInteractionZoneToggle.onValueChanged.AddListener(ShowHideInteractionZoneControls);
            InteractionMinDistanceField.onValueChanged.AddListener(OnValueChanged);
            InteractionMaxDistanceField.onValueChanged.AddListener(OnValueChanged);
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
            trackingBoneNearestToggle.onValueChanged.RemoveListener(OnValueChanged);
            trackingBoneIndexTipToggle.onValueChanged.RemoveListener(OnValueChanged);

            interactionTypeTogglePush.onValueChanged.RemoveListener(OnValueChanged);
            interactionTypeTogglePinch.onValueChanged.RemoveListener(OnValueChanged);
            interactionTypeToggleHover.onValueChanged.RemoveListener(OnValueChanged);
            interactionTypeToggleTouchPlane.onValueChanged.RemoveListener(OnValueChanged);

            // Interaction Zone Events
            EnableInteractionZoneToggle.onValueChanged.RemoveListener(OnValueChanged);
            EnableInteractionZoneToggle.onValueChanged.RemoveListener(ShowHideInteractionZoneControls);
            InteractionMinDistanceField.onValueChanged.RemoveListener(OnValueChanged);
            InteractionMaxDistanceField.onValueChanged.RemoveListener(OnValueChanged);
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

            var hoverStartTime = Mathf.Clamp(HoverStartTimeSlider.value, HoverCursorStartTime_Min, HoverCursorStartTime_Max);
            HoverStartTime.SetTextWithoutNotify(hoverStartTime.ToString("#0.00#"));
            HoverStartTimeSlider.SetValueWithoutNotify(hoverStartTime);

            var hoverCompleteTime = Mathf.Clamp(HoverCompleteTimeSlider.value, HoverCursorCompleteTime_Min, HoverCursorCompleteTime_Max);
            HoverCompleteTime.SetTextWithoutNotify(hoverCompleteTime.ToString("#0.00#"));
            HoverCompleteTimeSlider.SetValueWithoutNotify(hoverCompleteTime);

            var touchPlaneDistance = Mathf.Clamp(TouchPlaneDistanceSlider.value, TouchPlaneDistance_Min, TouchPlaneDistance_Max);
            TouchPlaneDistance.SetTextWithoutNotify(touchPlaneDistance.ToString("#0.00#"));
            TouchPlaneDistanceSlider.SetValueWithoutNotify(touchPlaneDistance);
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
            ConfigManager.InteractionConfig.HoverAndHold.HoverStartTimeS = HoverStartTimeSlider.value;
            ConfigManager.InteractionConfig.HoverAndHold.HoverCompleteTimeS = HoverCompleteTimeSlider.value;
            ConfigManager.InteractionConfig.TouchPlane.TouchPlaneActivationDistanceCM = TouchPlaneDistanceSlider.value;

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
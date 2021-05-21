using UnityEngine;
using UnityEngine.UI;
using HSVPicker;

namespace Ultraleap.TouchFree
{
    public class ConfigUI : MonoBehaviour
    {
        #region CursorSettings
        public Toggle EnableCursorToggle;
        // public SliderAndInputFieldMirrorValues CursorSizeInputSlider;
        public GameObject CustomColorControlContainer;
        public ColorPicker ColorPicker;

        public Toggle LightColorPresetToggle;
        public Toggle DarkColorPresetToggle;
        public Toggle CustomColorPresetToggle;

        public Toggle PrimaryColorToggle;
        public Toggle SecondaryColorToggle;
        public Toggle TertiaryColorToggle;
        #endregion

        #region CTISettings
        public Toggle EnableCTIToggle;
        public Text CurrentCTIFilepath;
        public Toggle CTIHideOnInteractionToggle;
        public Toggle CTIHideOnPresenceToggle;
        public InputField CTIShowDelayField;
        #endregion

        #region CursorPreview
        public Image DotCursorPreviewCenter;
        public Image DotCursorPreviewRing;
        public Image DotCursorPreviewBorder;
        public Image FillCursorPreviewCenter;
        public Image FillCursorPreviewRing;
        public Image FillCursorPreviewBorder;
        #endregion

        #region Cursors
        public Image DotCursorCenter;
        public Image DotCursorRing;
        public Image DotCursorBorder;
        public Image FillCursorCenter;
        public Image FillCursorRing;
        public Image FillCursorBorder;
        #endregion

        #region InteractionZone
        // public Toggle InteractionZoneToggle;
        // public SliderAndInputFieldMirrorValues InteractionMinimumDistanceInputSlider;
        // public SliderAndInputFieldMirrorValues InteractionMaximumDistanceInputSlider;
        #endregion

        #region LocalStores
        public Color PrimaryColor;
        public Color SecondaryColor;
        public Color TertiaryColor;
        public Color CustomPrimaryColor;
        public Color CustomSecondaryColor;
        public Color CustomTertiaryColor;
        public string CTIFilePath;
        #endregion

        private void AddValueChangedListeners()
        {
            ColorPicker.onValueChanged.AddListener(UpdateAppropriateColor);

            EnableCursorToggle.onValueChanged.AddListener(SetCustomColorControlVisibility);
            CustomColorPresetToggle.onValueChanged.AddListener(SetCustomColorControlVisibility);

            // LightColorPresetToggle.onValueChanged.AddListener();
            // DarkColorPresetToggle.onValueChanged.AddListener();
            // CustomColorPresetToggle.onValueChanged.AddListener();

            // PrimaryColorToggle.onValueChanged.AddListener();
            // PrimaryColorToggle.onValueChanged.AddListener();
            // PrimaryColorToggle.onValueChanged.AddListener();

            // CursorSizeInputSlider.onValueChanged.AddListener();
            // CTIHideOnInteractionToggle.onValueChanged.AddListener();
            // CTIHideOnPresenceToggle.onValueChanged.AddListener();
        }

        private void SetColorsToLightPreset()
        {
            PrimaryColor = Color.white;
            SecondaryColor = Color.black;
            TertiaryColor = Color.white;
        }

        private void SetColorsToDarkPreset()
        {
            PrimaryColor = Color.black;
            SecondaryColor = Color.white;
            TertiaryColor = Color.black;
        }

        private void SetColorsToCustom()
        {
            PrimaryColor = CustomPrimaryColor;
            SecondaryColor = CustomSecondaryColor;
            TertiaryColor = CustomTertiaryColor;
        }

        private void UpdateCursorColors()
        {
            DotCursorCenter.color = PrimaryColor;
            DotCursorPreviewCenter.color = PrimaryColor;
            FillCursorCenter.color = PrimaryColor;
            FillCursorPreviewCenter.color = PrimaryColor;

            DotCursorRing.color = SecondaryColor;
            DotCursorPreviewRing.color = SecondaryColor;
            FillCursorRing.color = SecondaryColor;
            FillCursorPreviewRing.color = SecondaryColor;

            DotCursorBorder.color = TertiaryColor;
            DotCursorPreviewBorder.color = TertiaryColor;
            FillCursorBorder.color = TertiaryColor;
            FillCursorPreviewBorder.color = TertiaryColor;
        }

        private void SetCustomColorControlVisibility(bool _)
        {
            var shouldShow = EnableCursorToggle.IsActive() && CustomColorPresetToggle.IsActive();
            CustomColorControlContainer.SetActive(shouldShow);
        }

        private void UpdateAppropriateColor(Color newColor)
        {
            // CursorColorSelectToggles
            if (PrimaryColorToggle.IsActive())
            {
                CustomPrimaryColor = newColor;
            }
            else if (SecondaryColorToggle.IsActive())
            {
                CustomSecondaryColor = newColor;
            }
            else if (TertiaryColorToggle.IsActive())
            {
                CustomTertiaryColor = newColor;
            }

            UpdateCursorColors();
            SaveValuesToConfig();
        }

        private void SetColorPickerColor()
        {
            if (PrimaryColorToggle.isOn)
            {
                ColorPicker.CurrentColor = PrimaryColor;
            } else if (SecondaryColorToggle.isOn) {
                ColorPicker.CurrentColor = SecondaryColor;
            } else {
                ColorPicker.CurrentColor = TertiaryColor;
            }
        }

        private void SaveValuesToConfig()
        {
            // Local Values
            ConfigManager.Config.primaryCursorColor = PrimaryColor;
            ConfigManager.Config.secondaryCursorColor = SecondaryColor;
            ConfigManager.Config.tertiaryCursorColor = PrimaryColor;
            ConfigManager.Config.ctiFilePath = CTIFilePath;

            // Values pulled from UI elements
            ConfigManager.Config.ctiEnabled = EnableCTIToggle.IsActive();
            ConfigManager.Config.cursorEnabled = EnableCursorToggle.IsActive();

            // ConfigManager.Config.cursorSizeCm =
            //     ConfigDataUtilities.TryParseNewStringToFloat(
            //         ConfigManager.Config.cursorSizeCm,
            //         CursorSizeInputSlider.Value);
            ConfigManager.Config.ctiShowAfterTimer =
                ConfigDataUtilities.TryParseNewStringToFloat(
                    ConfigManager.Config.ctiShowAfterTimer,
                    CTIShowDelayField.text);

            if (CTIHideOnInteractionToggle)
            {
                ConfigManager.Config.ctiHideTrigger = CtiHideTrigger.INTERACTION;
            }
            else if (CTIHideOnPresenceToggle)
            {
                ConfigManager.Config.ctiHideTrigger = CtiHideTrigger.PRESENCE;
            }
        }
    }
}
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

using HSVPicker;
using SFB;
using System.Threading;
using System.Globalization;

namespace Ultraleap.TouchFree
{
    public class ConfigUI : MonoBehaviour
    {
        [Header("CursorSettings")]
        public Toggle EnableCursorToggle;
        public SliderInputFieldCombiner CursorSizeInputSlider;
        public SliderInputFieldCombiner CursorRingThicknessInputSlider;

        public Toggle LightColorPresetToggle;
        public Toggle DarkColorPresetToggle;
        public Toggle CustomColorPresetToggle;
        public GameObject[] CursorSettingsToHide;

        [Header("CustomColorPicker")]
        public GameObject CustomColorControlContainer;
        public ColorPicker ColorPicker;
        public Toggle PrimaryColorToggle;
        public Toggle SecondaryColorToggle;
        public Toggle TertiaryColorToggle;

        public Toggle PrimaryColorAlphaToggle;
        public Toggle SecondaryColorAlphaToggle;
        public Toggle TertiaryColorAlphaToggle;

        [Header("CTISettings")]
        public Toggle EnableCTIToggle;
        public InputField CurrentCTIFilepath;
        public Toggle CTIHideOnInteractionToggle;
        public Toggle CTIHideOnPresenceToggle;
        public InputField CTIShowDelayField;
        public GameObject[] CTISettingsToHide;

        [Header("CursorPreview")]
        public GameObject RingCursorContainer;
        public Image RingCursorPreviewCenter;
        public Image RingCursorPreviewRing;
        public Image RingCursorPreviewBorder;
        public GameObject FillCursorContainer;
        public Image FillCursorPreviewCenter;
        public Image FillCursorPreviewRing;
        public Image FillCursorPreviewBorder;

        private Color PrimaryColor;
        private Color SecondaryColor;
        private Color TertiaryColor;
        private Color CustomPrimaryColor = Color.white;
        private Color CustomSecondaryColor = Color.white;
        private Color CustomTertiaryColor = Color.white;
        private string CTIFilePath;

        private void Awake()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-GB");
        }

        protected virtual void OnEnable()
        {
            LoadConfigValuesIntoFields();
            AddValueChangedListeners();
        }

        protected virtual void OnDisable()
        {
            RemoveValueChangedListeners();
        }

        private void SetPresetTogglesBasedOnColors(CursorColorPreset _activePreset)
        {
            switch (_activePreset)
            {
                case CursorColorPreset.LIGHT:
                    LightColorPresetToggle.SetIsOnWithoutNotify(true);
                    DarkColorPresetToggle.SetIsOnWithoutNotify(false);
                    CustomColorPresetToggle.SetIsOnWithoutNotify(false);
                    CustomColorControlContainer.SetActive(false);
                    break;
                case CursorColorPreset.DARK:
                    LightColorPresetToggle.SetIsOnWithoutNotify(false);
                    DarkColorPresetToggle.SetIsOnWithoutNotify(true);
                    CustomColorPresetToggle.SetIsOnWithoutNotify(false);
                    CustomColorControlContainer.SetActive(false);
                    break;
                case CursorColorPreset.CUSTOM:
                    LightColorPresetToggle.SetIsOnWithoutNotify(false);
                    DarkColorPresetToggle.SetIsOnWithoutNotify(false);
                    CustomColorPresetToggle.SetIsOnWithoutNotify(true);
                    CustomColorControlContainer.SetActive(true);
                    break;
            }
        }

        private void AddValueChangedListeners()
        {
            // Cursor Events
            ColorPicker.onValueChanged.AddListener(UpdateAppropriateColor);

            EnableCursorToggle.onValueChanged.AddListener(OnValueChanged);
            EnableCursorToggle.onValueChanged.AddListener(SetCustomColorControlVisibility);
            EnableCursorToggle.onValueChanged.AddListener(ShowHideCursorControls);
            CustomColorPresetToggle.onValueChanged.AddListener(SetCustomColorControlVisibility);

            LightColorPresetToggle.onValueChanged.AddListener(OnValueChanged);
            DarkColorPresetToggle.onValueChanged.AddListener(OnValueChanged);
            CustomColorPresetToggle.onValueChanged.AddListener(OnValueChanged);

            PrimaryColorToggle.onValueChanged.AddListener(SetColorPickerColor);
            SecondaryColorToggle.onValueChanged.AddListener(SetColorPickerColor);
            TertiaryColorToggle.onValueChanged.AddListener(SetColorPickerColor);

            PrimaryColorAlphaToggle.onValueChanged.AddListener(PrimaryAlphaToggled);
            SecondaryColorAlphaToggle.onValueChanged.AddListener(SecondaryAlphaToggled);
            TertiaryColorAlphaToggle.onValueChanged.AddListener(TertiaryAlphaToggled);

            CursorSizeInputSlider.onValueChanged.AddListener(OnValueChanged);
            CursorRingThicknessInputSlider.onValueChanged.AddListener(OnValueChanged);

            // CTI Events
            EnableCTIToggle.onValueChanged.AddListener(OnValueChanged);
            EnableCTIToggle.onValueChanged.AddListener(ShowHideCtiControls);
            CTIHideOnInteractionToggle.onValueChanged.AddListener(OnValueChanged);
            CTIHideOnPresenceToggle.onValueChanged.AddListener(OnValueChanged);
            CTIShowDelayField.onValueChanged.AddListener(OnValueChanged);

            TouchFreeCursorManager.CursorChanged += SetCurrentlyActiveCursor;
        }

        private void RemoveValueChangedListeners()
        {
            // Cursor Events
            ColorPicker.onValueChanged.RemoveListener(UpdateAppropriateColor);

            EnableCursorToggle.onValueChanged.RemoveListener(OnValueChanged);
            EnableCursorToggle.onValueChanged.RemoveListener(SetCustomColorControlVisibility);
            EnableCursorToggle.onValueChanged.RemoveListener(ShowHideCursorControls);
            CustomColorPresetToggle.onValueChanged.RemoveListener(SetCustomColorControlVisibility);

            LightColorPresetToggle.onValueChanged.RemoveListener(OnValueChanged);
            DarkColorPresetToggle.onValueChanged.RemoveListener(OnValueChanged);
            CustomColorPresetToggle.onValueChanged.RemoveListener(OnValueChanged);

            PrimaryColorToggle.onValueChanged.RemoveListener(SetColorPickerColor);
            SecondaryColorToggle.onValueChanged.RemoveListener(SetColorPickerColor);
            TertiaryColorToggle.onValueChanged.RemoveListener(SetColorPickerColor);

            PrimaryColorAlphaToggle.onValueChanged.RemoveListener(PrimaryAlphaToggled);
            SecondaryColorAlphaToggle.onValueChanged.RemoveListener(SecondaryAlphaToggled);
            TertiaryColorAlphaToggle.onValueChanged.RemoveListener(TertiaryAlphaToggled);

            CursorSizeInputSlider.onValueChanged.RemoveListener(OnValueChanged);
            CursorRingThicknessInputSlider.onValueChanged.RemoveListener(OnValueChanged);

            // CTI Events
            EnableCTIToggle.onValueChanged.RemoveListener(OnValueChanged);
            EnableCTIToggle.onValueChanged.RemoveListener(ShowHideCtiControls);
            CTIHideOnInteractionToggle.onValueChanged.RemoveListener(OnValueChanged);
            CTIHideOnPresenceToggle.onValueChanged.RemoveListener(OnValueChanged);
            CTIShowDelayField.onValueChanged.RemoveListener(OnValueChanged);

            TouchFreeCursorManager.CursorChanged -= SetCurrentlyActiveCursor;
        }

        public void SetFileLocation()
        {
            var extensions = new string[
                CallToInteractController.VIDEO_EXTENSIONS.Length +
                CallToInteractController.IMAGE_EXTENSIONS.Length];
            CallToInteractController.VIDEO_EXTENSIONS.CopyTo(extensions, 0);
            CallToInteractController.IMAGE_EXTENSIONS.CopyTo(extensions, CallToInteractController.VIDEO_EXTENSIONS.Length);

            for (int i = 0; i < extensions.Length; i++)
            {
                extensions[i] = extensions[i].Replace(".", "");
            }

            var extFilter = new ExtensionFilter[] { new ExtensionFilter("CTIFileExtensions", extensions) };
            string[] paths = StandaloneFileBrowser.OpenFilePanel("", Path.GetDirectoryName(CTIFilePath), extFilter, false);

            if (paths.Length > 0)
            {
                CTIFilePath = paths[0];
                CurrentCTIFilepath.SetTextWithoutNotify(CTIFilePath);
            }

            SaveValuesToConfig();
        }

        #region ShowHideRegionsBasedOnToggles
        protected void ShowHideCtiControls(bool _state)
        {
            foreach (GameObject control in CTISettingsToHide)
            {
                control.SetActive(_state);
            }
        }

        protected void ShowHideCursorControls(bool _state)
        {
            foreach (GameObject control in CursorSettingsToHide)
            {
                control.SetActive(_state);
            }
        }
        #endregion

        #region Color Picker/Toggles methods
        private void SetColorsToCorrectPreset()
        {
            ConfigManager.Config.GetCurrentColors(
                ref PrimaryColor,
                ref SecondaryColor,
                ref TertiaryColor);
            UpdatePreviewCursorColors();
        }

        private void UpdatePreviewCursorColors()
        {
            if (CustomColorPresetToggle.isOn)
            {
                PrimaryColor = CustomPrimaryColor;
                SecondaryColor = CustomSecondaryColor;
                TertiaryColor = CustomTertiaryColor;
            }

            RingCursorPreviewCenter.color = PrimaryColor;
            FillCursorPreviewCenter.color = PrimaryColor;
            RingCursorPreviewRing.color = SecondaryColor;
            FillCursorPreviewRing.color = SecondaryColor;
            RingCursorPreviewBorder.color = TertiaryColor;
            FillCursorPreviewBorder.color = TertiaryColor;
        }

        private void SetCustomColorControlVisibility(bool _)
        {
            var shouldShow = EnableCursorToggle.isOn && CustomColorPresetToggle.isOn;
            CustomColorControlContainer.SetActive(shouldShow);
        }

        private void UpdateAppropriateColor(Color newColor)
        {
            // CursorColorSelectToggles
            if (PrimaryColorToggle.isOn)
            {
                CustomPrimaryColor = newColor;
            }
            else if (SecondaryColorToggle.isOn)
            {
                CustomSecondaryColor = newColor;
            }
            else if (TertiaryColorToggle.isOn)
            {
                CustomTertiaryColor = newColor;
            }

            PrimaryColorAlphaToggle.SetIsOnWithoutNotify(CustomPrimaryColor.a != 0);
            SecondaryColorAlphaToggle.SetIsOnWithoutNotify(CustomSecondaryColor.a != 0);
            TertiaryColorAlphaToggle.SetIsOnWithoutNotify(CustomTertiaryColor.a != 0);

            if (CustomColorPresetToggle.isOn)
            {
                PrimaryColor = CustomPrimaryColor;
                SecondaryColor = CustomSecondaryColor;
                TertiaryColor = CustomTertiaryColor;

                SetColorsToCorrectPreset();
                UpdatePreviewCursorColors();
            }

            SaveValuesToConfig();
        }

        private void SetCurrentlyActiveCursor(CursorManager.CursorType _type)
        {
            switch (_type)
            {
                case CursorManager.CursorType.FILL:
                    FillCursorContainer.SetActive(true);
                    RingCursorContainer.SetActive(false);
                    break;
                case CursorManager.CursorType.RING:
                    FillCursorContainer.SetActive(false);
                    RingCursorContainer.SetActive(true);
                    break;
            }
        }

        private void SetColorPickerColor(bool _)
        {
            if (PrimaryColorToggle.isOn)
            {
                ColorPicker.CurrentColor = PrimaryColor;
            }
            else if (SecondaryColorToggle.isOn)
            {
                ColorPicker.CurrentColor = SecondaryColor;
            }
            else
            {
                ColorPicker.CurrentColor = TertiaryColor;
            }
        }

        private void PrimaryAlphaToggled(bool _toggledTo)
        {
            AlphaToggled(ref CustomPrimaryColor, _toggledTo);
        }

        private void SecondaryAlphaToggled(bool _toggledTo)
        {
            AlphaToggled(ref CustomSecondaryColor, _toggledTo);
        }

        private void TertiaryAlphaToggled(bool _toggledTo)
        {
            AlphaToggled(ref CustomTertiaryColor, _toggledTo);
        }

        void AlphaToggled(ref Color _colourToChange, bool _changeTo)
        {
            if (_changeTo)
            {
                _colourToChange.a = 1;
            }
            else
            {
                _colourToChange.a = 0;
            }

            PrimaryColor = CustomPrimaryColor;
            SecondaryColor = CustomSecondaryColor;
            TertiaryColor = CustomTertiaryColor;

            SetColorPickerColor(true);

            if (CustomColorPresetToggle.isOn)
            {
                SetColorsToCorrectPreset();
                UpdatePreviewCursorColors();
            }

            SaveValuesToConfig();
        }
        #endregion

        #region ConfigFile Methods
        private void LoadConfigValuesIntoFields()
        {
            // Cursor settings
            EnableCursorToggle.SetIsOnWithoutNotify(ConfigManager.Config.cursorEnabled);
            CursorSizeInputSlider.SetValueWithoutNotify(ConfigManager.Config.cursorSizeCm);
            CursorRingThicknessInputSlider.SetValueWithoutNotify(ConfigManager.Config.cursorRingThickness);
            CustomPrimaryColor = ConfigManager.Config.primaryCustomColor;
            ColorPicker.CurrentColor = ConfigManager.Config.primaryCustomColor;
            CustomSecondaryColor = ConfigManager.Config.secondaryCustomColor;
            CustomTertiaryColor = ConfigManager.Config.tertiaryCustomColor;

            PrimaryColorAlphaToggle.SetIsOnWithoutNotify(CustomPrimaryColor.a != 0);
            SecondaryColorAlphaToggle.SetIsOnWithoutNotify(CustomSecondaryColor.a != 0);
            TertiaryColorAlphaToggle.SetIsOnWithoutNotify(CustomTertiaryColor.a != 0);

            SetPresetTogglesBasedOnColors(ConfigManager.Config.activeCursorPreset);
            SetColorsToCorrectPreset();
            UpdatePreviewCursorColors();

            // CTI settings
            EnableCTIToggle.SetIsOnWithoutNotify(ConfigManager.Config.ctiEnabled);
            CTIFilePath = ConfigManager.Config.ctiFilePath;
            CTIShowDelayField.SetTextWithoutNotify(ConfigManager.Config.ctiShowAfterTimer.ToString());

            ShowHideCursorControls(ConfigManager.Config.cursorEnabled);
            ShowHideCtiControls(ConfigManager.Config.ctiEnabled);

            SetCustomColorControlVisibility(CustomColorPresetToggle.isOn);

            switch (ConfigManager.Config.ctiHideTrigger)
            {
                case CtiHideTrigger.PRESENCE:
                    CTIHideOnInteractionToggle.SetIsOnWithoutNotify(false);
                    CTIHideOnPresenceToggle.SetIsOnWithoutNotify(true);
                    break;
                case CtiHideTrigger.INTERACTION:
                    CTIHideOnInteractionToggle.SetIsOnWithoutNotify(true);
                    CTIHideOnPresenceToggle.SetIsOnWithoutNotify(false);
                    break;
                default:
                    break;
            }
        }

        private void SaveValuesToConfig()
        {
            // Local Values
            ConfigManager.Config.primaryCustomColor = CustomPrimaryColor;
            ConfigManager.Config.secondaryCustomColor = CustomSecondaryColor;
            ConfigManager.Config.tertiaryCustomColor = CustomTertiaryColor;
            ConfigManager.Config.ctiFilePath = CTIFilePath;

            // Values pulled from UI elements
            ConfigManager.Config.ctiEnabled = EnableCTIToggle.isOn;
            ConfigManager.Config.cursorEnabled = EnableCursorToggle.isOn;

            ConfigManager.Config.cursorSizeCm = CursorSizeInputSlider.Value;
            ConfigManager.Config.cursorRingThickness = CursorRingThicknessInputSlider.Value;
            ConfigManager.Config.ctiShowAfterTimer =
                ConfigDataUtilities.TryParseNewStringToFloat(
                    ConfigManager.Config.ctiShowAfterTimer,
                    CTIShowDelayField.text);

            // Toggles
            if (LightColorPresetToggle.isOn)
            {
                ConfigManager.Config.activeCursorPreset = CursorColorPreset.LIGHT;
            }
            else if (DarkColorPresetToggle.isOn)
            {
                ConfigManager.Config.activeCursorPreset = CursorColorPreset.DARK;
            }
            else
            {
                ConfigManager.Config.activeCursorPreset = CursorColorPreset.CUSTOM;
            }

            if (CTIHideOnInteractionToggle.isOn)
            {
                ConfigManager.Config.ctiHideTrigger = CtiHideTrigger.INTERACTION;
            }
            else if (CTIHideOnPresenceToggle.isOn)
            {
                ConfigManager.Config.ctiHideTrigger = CtiHideTrigger.PRESENCE;
            }

            ConfigManager.Config.ConfigWasUpdated();
            ConfigManager.Config.SaveConfig();
        }

        public void ResetToDefaults()
        {
            RemoveValueChangedListeners();
            ConfigManager.Config.SetAllValuesToDefault();
            ConfigManager.Config.ConfigWasUpdated();
            ConfigManager.Config.SaveConfig();
            LoadConfigValuesIntoFields();
            AddValueChangedListeners();
        }
        #endregion

        #region OnValueChanged Overrides
        protected void OnValueChanged(string _)
        {
            OnValueChanged();
        }

        protected void OnValueChanged(float _)
        {
            OnValueChanged();
        }

        protected void OnValueChanged(int _)
        {
            OnValueChanged();
        }

        protected void OnValueChanged(bool _)
        {
            OnValueChanged();
        }

        protected void OnValueChanged()
        {
            SaveValuesToConfig();
            SetColorsToCorrectPreset();
        }
        #endregion
    }
}
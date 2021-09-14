using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

using HSVPicker;
using SFB;
using System.Threading;
using System.Globalization;
using Ultraleap.TouchFree.ServiceShared;

namespace Ultraleap.TouchFree.ServiceUI
{
    public class TFAppConfigUI : MonoBehaviour
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

        private Color PrimaryColor;
        private Color SecondaryColor;
        private Color TertiaryColor;
        private Color CustomPrimaryColor = Color.white;
        private Color CustomSecondaryColor = Color.white;
        private Color CustomTertiaryColor = Color.white;
        private string CTIFilePath;

        public static readonly string[] VIDEO_EXTENSIONS = new string[] { ".webm", ".mp4" };
        public static readonly string[] IMAGE_EXTENSIONS = new string[] { ".png" };

        public GameObject[] permissionsBlockers;

        private void Awake()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-GB");
        }

        protected virtual void OnEnable()
        {
            if (!PermissionController.hasFilePermission)
            {
                foreach (var blocker in permissionsBlockers)
                {
                    blocker.SetActive(true);
                }
            }

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
        }

        public void SetFileLocation()
        {
            var extensions = new string[VIDEO_EXTENSIONS.Length + IMAGE_EXTENSIONS.Length];
            VIDEO_EXTENSIONS.CopyTo(extensions, 0);
            IMAGE_EXTENSIONS.CopyTo(extensions, VIDEO_EXTENSIONS.Length);

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
            TFAppConfig.Config.GetCurrentColors(
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
            RingCursorPreviewRing.color = SecondaryColor;
            RingCursorPreviewBorder.color = TertiaryColor;
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
            EnableCursorToggle.SetIsOnWithoutNotify(TFAppConfig.Config.cursorEnabled);
            CursorSizeInputSlider.SetValueWithoutNotify(TFAppConfig.Config.cursorSizeCm);
            CursorRingThicknessInputSlider.SetValueWithoutNotify(TFAppConfig.Config.cursorRingThickness);
            CustomPrimaryColor = TFAppConfig.Config.primaryCustomColor;
            ColorPicker.CurrentColor = TFAppConfig.Config.primaryCustomColor;
            CustomSecondaryColor = TFAppConfig.Config.secondaryCustomColor;
            CustomTertiaryColor = TFAppConfig.Config.tertiaryCustomColor;

            PrimaryColorAlphaToggle.SetIsOnWithoutNotify(CustomPrimaryColor.a != 0);
            SecondaryColorAlphaToggle.SetIsOnWithoutNotify(CustomSecondaryColor.a != 0);
            TertiaryColorAlphaToggle.SetIsOnWithoutNotify(CustomTertiaryColor.a != 0);

            SetPresetTogglesBasedOnColors(TFAppConfig.Config.activeCursorPreset);
            SetColorsToCorrectPreset();
            UpdatePreviewCursorColors();

            // CTI settings
            EnableCTIToggle.SetIsOnWithoutNotify(TFAppConfig.Config.ctiEnabled);
            CTIFilePath = TFAppConfig.Config.ctiFilePath;
            CTIShowDelayField.SetTextWithoutNotify(TFAppConfig.Config.ctiShowAfterTimer.ToString());

            ShowHideCursorControls(TFAppConfig.Config.cursorEnabled);
            ShowHideCtiControls(TFAppConfig.Config.ctiEnabled);

            SetCustomColorControlVisibility(CustomColorPresetToggle.isOn);

            switch (TFAppConfig.Config.ctiHideTrigger)
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
            TFAppConfig.Config.primaryCustomColor = CustomPrimaryColor;
            TFAppConfig.Config.secondaryCustomColor = CustomSecondaryColor;
            TFAppConfig.Config.tertiaryCustomColor = CustomTertiaryColor;
            TFAppConfig.Config.ctiFilePath = CTIFilePath;

            // Values pulled from UI elements
            TFAppConfig.Config.ctiEnabled = EnableCTIToggle.isOn;
            TFAppConfig.Config.cursorEnabled = EnableCursorToggle.isOn;

            TFAppConfig.Config.cursorSizeCm = CursorSizeInputSlider.Value;
            TFAppConfig.Config.cursorRingThickness = CursorRingThicknessInputSlider.Value;
            TFAppConfig.Config.ctiShowAfterTimer =
                ServiceShared.ServiceUtility.TryParseNewStringToFloat(
                    TFAppConfig.Config.ctiShowAfterTimer,
                    CTIShowDelayField.text);

            // Toggles
            if (LightColorPresetToggle.isOn)
            {
                TFAppConfig.Config.activeCursorPreset = CursorColorPreset.LIGHT;
            }
            else if (DarkColorPresetToggle.isOn)
            {
                TFAppConfig.Config.activeCursorPreset = CursorColorPreset.DARK;
            }
            else
            {
                TFAppConfig.Config.activeCursorPreset = CursorColorPreset.CUSTOM;
            }

            if (CTIHideOnInteractionToggle.isOn)
            {
                TFAppConfig.Config.ctiHideTrigger = CtiHideTrigger.INTERACTION;
            }
            else if (CTIHideOnPresenceToggle.isOn)
            {
                TFAppConfig.Config.ctiHideTrigger = CtiHideTrigger.PRESENCE;
            }

            TFAppConfig.Config.ConfigWasUpdated();
            TFAppConfig.Config.SaveConfig();
        }

        public void ResetToDefaults()
        {
            RemoveValueChangedListeners();
            TFAppConfig.Config.SetAllValuesToDefault();
            TFAppConfig.Config.ConfigWasUpdated();
            TFAppConfig.Config.SaveConfig();
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
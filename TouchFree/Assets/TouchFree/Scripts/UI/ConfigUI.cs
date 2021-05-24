using System.Collections;
using UnityEngine;
using UnityEngine.UI;

using HSVPicker;
using SFB;

using Ultraleap.ScreenControl.Client.Cursors;

namespace Ultraleap.TouchFree
{
    public class ConfigUI : MonoBehaviour
    {
        [Header("CursorSettings")]
        public Toggle EnableCursorToggle;
        public SliderInputFieldCombiner CursorSizeInputSlider;

        public Toggle LightColorPresetToggle;
        public Toggle DarkColorPresetToggle;
        public Toggle CustomColorPresetToggle;

        [Header("CustomColorPicker")]
        public GameObject CustomColorControlContainer;
        public ColorPicker ColorPicker;
        public Toggle PrimaryColorToggle;
        public Toggle SecondaryColorToggle;
        public Toggle TertiaryColorToggle;

        [Header("CTISettings")]
        public Toggle EnableCTIToggle;
        public InputField CurrentCTIFilepath;
        public Toggle CTIHideOnInteractionToggle;
        public Toggle CTIHideOnPresenceToggle;
        public InputField CTIShowDelayField;

        [Header("CursorPreview")]
        public GameObject RingCursorContainer;
        public Image RingCursorPreviewCenter;
        public Image RingCursorPreviewRing;
        public Image RingCursorPreviewBorder;
        public GameObject FillCursorContainer;
        public Image FillCursorPreviewCenter;
        public Image FillCursorPreviewRing;
        public Image FillCursorPreviewBorder;

        [Header("Cursors")]
        public TouchlessCursor[] cursors;
        // public Image RingCursorCenter;
        // public SpriteRenderer RingCursorRing;
        // public Image RingCursorBorder;
        // public Image FillCursorCenter;
        // public Image FillCursorRing;
        // public Image FillCursorBorder;

        // [Header("InteractionZone")]
        // public Toggle InteractionZoneToggle;
        // public SliderAndInputFieldMirrorValues InteractionMinimumDistanceInputSlider;
        // public SliderAndInputFieldMirrorValues InteractionMaximumDistanceInputSlider;

        private Color PrimaryColor;
        private Color SecondaryColor;
        private Color TertiaryColor;
        private Color CustomPrimaryColor = Color.white;
        private Color CustomSecondaryColor = Color.white;
        private Color CustomTertiaryColor = Color.white;
        private string CTIFilePath;

        protected virtual void OnEnable()
        {
            LoadConfigValuesIntoFields();
            AddValueChangedListeners();
        }

        private void LoadConfigValuesIntoFields()
        {
            // Cursor settings
            EnableCursorToggle.isOn = ConfigManager.Config.cursorEnabled;
            CursorSizeInputSlider.Value = ConfigManager.Config.cursorSizeCm;
            PrimaryColor = ConfigManager.Config.primaryCursorColor;
            SecondaryColor = ConfigManager.Config.secondaryCursorColor;
            TertiaryColor = ConfigManager.Config.tertiaryCursorColor;
            SetPresetTogglesBasedOnColors();
            UpdateCursorColors();

            // CTI settings
            EnableCTIToggle.isOn = ConfigManager.Config.ctiEnabled;
            CTIFilePath = ConfigManager.Config.ctiFilePath;
            CTIShowDelayField.text = ConfigManager.Config.ctiShowAfterTimer.ToString();

            switch (ConfigManager.Config.ctiHideTrigger)
            {
                case CtiHideTrigger.PRESENCE:
                    CTIHideOnInteractionToggle.isOn = false;
                    CTIHideOnPresenceToggle.isOn = true;
                    break;
                case CtiHideTrigger.INTERACTION:
                    CTIHideOnInteractionToggle.isOn = true;
                    CTIHideOnPresenceToggle.isOn = false;
                    break;
                default:
                    break;
            }
        }

        private void SetPresetTogglesBasedOnColors()
        {
            if (PrimaryColor == Color.white &&
                SecondaryColor == Color.white &&
                TertiaryColor == Color.black)
            {
                LightColorPresetToggle.isOn = true;
                DarkColorPresetToggle.isOn = false;
                CustomColorPresetToggle.isOn = false;
                CustomColorControlContainer.SetActive(false);
            }
            else if (PrimaryColor == Color.black &&
                SecondaryColor == Color.black &&
                TertiaryColor == Color.white)
            {
                LightColorPresetToggle.isOn = false;
                DarkColorPresetToggle.isOn = true;
                CustomColorPresetToggle.isOn = false;
                CustomColorControlContainer.SetActive(false);
            }
            else
            {
                LightColorPresetToggle.isOn = false;
                DarkColorPresetToggle.isOn = false;
                CustomColorPresetToggle.isOn = true;
                CustomColorControlContainer.SetActive(true);
            }
        }

        private void AddValueChangedListeners()
        {
            ColorPicker.onValueChanged.AddListener(UpdateAppropriateColor);

            EnableCursorToggle.onValueChanged.AddListener(SetCustomColorControlVisibility);
            CustomColorPresetToggle.onValueChanged.AddListener(SetCustomColorControlVisibility);

            LightColorPresetToggle.onValueChanged.AddListener(SetColorsToCorrectPreset);
            DarkColorPresetToggle.onValueChanged.AddListener(SetColorsToCorrectPreset);
            CustomColorPresetToggle.onValueChanged.AddListener(SetColorsToCorrectPreset);

            PrimaryColorToggle.onValueChanged.AddListener(SetColorPickerColor);
            SecondaryColorToggle.onValueChanged.AddListener(SetColorPickerColor);
            TertiaryColorToggle.onValueChanged.AddListener(SetColorPickerColor);

            EnableCursorToggle.onValueChanged.AddListener(OnValueChanged);
            CursorSizeInputSlider.onValueChanged.AddListener(OnValueChanged);

            EnableCTIToggle.onValueChanged.AddListener(OnValueChanged);
            CTIHideOnInteractionToggle.onValueChanged.AddListener(OnValueChanged);
            CTIHideOnPresenceToggle.onValueChanged.AddListener(OnValueChanged);
            CTIShowDelayField.onValueChanged.AddListener(OnValueChanged);

            TouchFreeCursorManager.CursorChanged += SetCurrentlyActiveCursor;
        }

        private void SetColorsToCorrectPreset(bool _)
        {
            if (LightColorPresetToggle.isOn)
            {
                PrimaryColor = Color.white;
                SecondaryColor = Color.white;
                TertiaryColor = Color.black;
            }
            else if (DarkColorPresetToggle.isOn)
            {
                PrimaryColor = Color.black;
                SecondaryColor = Color.black;
                TertiaryColor = Color.white;
            }
            else
            {
                PrimaryColor = CustomPrimaryColor;
                SecondaryColor = CustomSecondaryColor;
                TertiaryColor = CustomTertiaryColor;
            }

            UpdateCursorColors();
            OnValueChanged();
        }

        private void UpdateCursorColors()
        {
            // foreach(TouchlessCursor cursor in cursors) {
            //     cursor.SetColors(PrimaryColor, SecondaryColor, TertiaryColor);
            // }

            RingCursorPreviewCenter.color = PrimaryColor;
            // FillCursorPreviewCenter.color = PrimaryColor;
            RingCursorPreviewRing.color = SecondaryColor;
            // FillCursorPreviewRing.color = SecondaryColor;
            RingCursorPreviewBorder.color = TertiaryColor;
            // FillCursorPreviewBorder.color = TertiaryColor;
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

            if (CustomColorPresetToggle.isOn)
            {
                SetColorsToCorrectPreset(true);
                UpdateCursorColors();
            }

            SaveValuesToConfig();
        }

        private void SetCurrentlyActiveCursor(CursorManager.CursorType _type) {
            switch (_type) {
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
            string[] paths = StandaloneFileBrowser.OpenFilePanel("", CTIFilePath, extFilter, false);

            if (paths.Length > 0)
            {
                CTIFilePath = paths[0];
                CurrentCTIFilepath.text = CTIFilePath;
            }

            SaveValuesToConfig();
        }

        private void ValidateValues()
        {
            // CursorSizeInputSlider is validated by the SliderInputCombiner
            CTIShowDelayField.SetTextWithoutNotify(
                ConfigDataUtilities.TryParseNewStringToFloat(
                    ConfigManager.Config.ctiShowAfterTimer,
                    CTIShowDelayField.text).ToString("#0.#"));
        }

        private void SaveValuesToConfig()
        {
            // Local Values
            ConfigManager.Config.primaryCursorColor = PrimaryColor;
            ConfigManager.Config.secondaryCursorColor = SecondaryColor;
            ConfigManager.Config.tertiaryCursorColor = TertiaryColor;
            ConfigManager.Config.ctiFilePath = CTIFilePath;

            // Values pulled from UI elements
            ConfigManager.Config.ctiEnabled = EnableCTIToggle.isOn;
            ConfigManager.Config.cursorEnabled = EnableCursorToggle.isOn;

            ConfigManager.Config.cursorSizeCm = CursorSizeInputSlider.Value;

            ConfigManager.Config.ctiShowAfterTimer =
                ConfigDataUtilities.TryParseNewStringToFloat(
                    ConfigManager.Config.ctiShowAfterTimer,
                    CTIShowDelayField.text);

            if (CTIHideOnInteractionToggle.isOn)
            {
                ConfigManager.Config.ctiHideTrigger = CtiHideTrigger.INTERACTION;
            }
            else if (CTIHideOnPresenceToggle.isOn)
            {
                ConfigManager.Config.ctiHideTrigger = CtiHideTrigger.PRESENCE;
            }

            ConfigManager.Config.ConfigWasUpdated();
        }

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
            ValidateValues();
            SaveValuesToConfig();
        }
        #endregion
    }
}
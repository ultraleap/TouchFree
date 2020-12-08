using UnityEngine;
using UnityEngine.UI;

namespace Ultraleap.ScreenControl.Core
{
    public class ManualConfigUI : ConfigUI
    {
        #region Bounds
        public const float ScreenHeight_Min = 0.05f;
        public const float ScreenHeight_Max = 1f;

        public const float TrackingOriginX_Min = -0.25f;
        public const float TrackingOriginX_Max = 0.25f;

        public const float TrackingOriginY_Min = -1f;
        public const float TrackingOriginY_Max = 1f;

        public const float TrackingOriginZ_Min = -0.5f;
        public const float TrackingOriginZ_Max = 0.5f;

        public const float ScreenTilt_Min = -90f;
        public const float ScreenTilt_Max = 90f;

        public const float VirtualScreenDist_Min = 0.01f;
        public const float VirtualScreenDist_Max = 0.5f;

        public const float TrackingRoation_Min = -90f;
        public const float TrackingRoation_Max = 90f;
        #endregion

        public InputField PhysicalScreenTiltAngle;
        public InputField ScreenHeight;
        public InputField TrackingOriginX;
        public InputField TrackingOriginY;
        public InputField TrackingOriginZ;
        public InputField TrackingRotationX;

        public Slider PhysicalScreenTiltAngleSlider;
        public Slider ScreenHeightSlider;
        public Slider TrackingOriginXSlider;
        public Slider TrackingOriginYSlider;
        public Slider TrackingOriginZSlider;
        public Slider TrackingRotationXSlider;

        public GameObject resetToDefaultWarning;

        private void Awake()
        {
            InitialiseUI();
            resetToDefaultWarning.SetActive(false);
        }

        void InitialiseUI()
        {
            PhysicalScreenTiltAngleSlider.minValue = ScreenTilt_Min;
            ScreenHeightSlider.minValue = ScreenControlUtility.ToDisplayUnits(ScreenHeight_Min);
            TrackingOriginXSlider.minValue = ScreenControlUtility.ToDisplayUnits(TrackingOriginX_Min);
            TrackingOriginYSlider.minValue = ScreenControlUtility.ToDisplayUnits(TrackingOriginY_Min);
            TrackingOriginZSlider.minValue = ScreenControlUtility.ToDisplayUnits(TrackingOriginZ_Min);
            TrackingRotationXSlider.minValue = TrackingRoation_Min;

            PhysicalScreenTiltAngleSlider.maxValue = ScreenTilt_Max;
            ScreenHeightSlider.maxValue = ScreenControlUtility.ToDisplayUnits(ScreenHeight_Max);
            TrackingOriginXSlider.maxValue = ScreenControlUtility.ToDisplayUnits(TrackingOriginX_Max);
            TrackingOriginYSlider.maxValue = ScreenControlUtility.ToDisplayUnits(TrackingOriginY_Max);
            TrackingOriginZSlider.maxValue = ScreenControlUtility.ToDisplayUnits(TrackingOriginZ_Max);
            TrackingRotationXSlider.maxValue = TrackingRoation_Max;
        }

        protected override void AddValueChangedListeners()
        {
            PhysicalScreenTiltAngle.onEndEdit.AddListener(OnInputFieldChanged);
            ScreenHeight.onEndEdit.AddListener(OnInputFieldChanged);
            TrackingOriginX.onEndEdit.AddListener(OnInputFieldChanged);
            TrackingOriginY.onEndEdit.AddListener(OnInputFieldChanged);
            TrackingOriginZ.onEndEdit.AddListener(OnInputFieldChanged);
            TrackingRotationX.onEndEdit.AddListener(OnInputFieldChanged);

            PhysicalScreenTiltAngleSlider.onValueChanged.AddListener(OnValueChanged);
            ScreenHeightSlider.onValueChanged.AddListener(OnValueChanged);
            TrackingOriginXSlider.onValueChanged.AddListener(OnValueChanged);
            TrackingOriginYSlider.onValueChanged.AddListener(OnValueChanged);
            TrackingOriginZSlider.onValueChanged.AddListener(OnValueChanged);
            TrackingRotationXSlider.onValueChanged.AddListener(OnValueChanged);
        }

        protected override void RemoveValueChangedListeners()
        {
            PhysicalScreenTiltAngle.onEndEdit.RemoveListener(OnInputFieldChanged);
            ScreenHeight.onEndEdit.RemoveListener(OnInputFieldChanged);
            TrackingOriginX.onEndEdit.RemoveListener(OnInputFieldChanged);
            TrackingOriginY.onEndEdit.RemoveListener(OnInputFieldChanged);
            TrackingOriginZ.onEndEdit.RemoveListener(OnInputFieldChanged);
            TrackingRotationX.onEndEdit.RemoveListener(OnInputFieldChanged);

            PhysicalScreenTiltAngleSlider.onValueChanged.RemoveListener(OnValueChanged);
            ScreenHeightSlider.onValueChanged.RemoveListener(OnValueChanged);
            TrackingOriginXSlider.onValueChanged.RemoveListener(OnValueChanged);
            TrackingOriginYSlider.onValueChanged.RemoveListener(OnValueChanged);
            TrackingOriginZSlider.onValueChanged.RemoveListener(OnValueChanged);
            TrackingRotationXSlider.onValueChanged.RemoveListener(OnValueChanged);
        }

        protected override void LoadConfigValuesIntoFields()
        {
            PhysicalScreenTiltAngle.SetTextWithoutNotify(
                ConfigManager.PhysicalConfig.ScreenRotationD.ToString("##0.0"));

            ScreenHeight.SetTextWithoutNotify(
                ScreenControlUtility.ToDisplayUnits(
                    ConfigManager.PhysicalConfig.ScreenHeightM
                ).ToString("#0.00#"));

            TrackingOriginX.SetTextWithoutNotify(
                ScreenControlUtility.ToDisplayUnits(
                    ConfigManager.PhysicalConfig.LeapPositionRelativeToScreenBottomM.x
                ).ToString("#0.00#"));

            TrackingOriginY.SetTextWithoutNotify(
                ScreenControlUtility.ToDisplayUnits(
                    ConfigManager.PhysicalConfig.LeapPositionRelativeToScreenBottomM.y
                ).ToString("#0.00#"));

            TrackingOriginZ.SetTextWithoutNotify(
                ScreenControlUtility.ToDisplayUnits(-
                    ConfigManager.PhysicalConfig.LeapPositionRelativeToScreenBottomM.z
                ).ToString("#0.00#"));

            TrackingRotationX.SetTextWithoutNotify(
                ConfigManager.PhysicalConfig.LeapRotationD.x.ToString("##0.0"));

            var bottomMount = Mathf.Approximately(
                ConfigManager.PhysicalConfig.LeapRotationD.z, 0f);

            PhysicalScreenTiltAngleSlider.SetValueWithoutNotify(
                ConfigManager.PhysicalConfig.ScreenRotationD);

            ScreenHeightSlider.SetValueWithoutNotify(
                ScreenControlUtility.ToDisplayUnits(
                    ConfigManager.PhysicalConfig.ScreenHeightM));

            TrackingOriginXSlider.SetValueWithoutNotify(
                ScreenControlUtility.ToDisplayUnits(
                        ConfigManager.PhysicalConfig.LeapPositionRelativeToScreenBottomM.x));

            TrackingOriginYSlider.SetValueWithoutNotify(
                ScreenControlUtility.ToDisplayUnits(
                    ConfigManager.PhysicalConfig.LeapPositionRelativeToScreenBottomM.y));

            TrackingOriginZSlider.SetValueWithoutNotify(
                ScreenControlUtility.ToDisplayUnits(-
                    ConfigManager.PhysicalConfig.LeapPositionRelativeToScreenBottomM.z));

            TrackingRotationXSlider.SetValueWithoutNotify(
                ConfigManager.PhysicalConfig.LeapRotationD.x);

        }

        protected override void CommitValuesToFile()
        {
            ConfigManager.PhysicalConfig.SaveConfig();
        }

        protected override void ValidateValues()
        {
            PhysicalScreenTiltAngle.SetTextWithoutNotify(PhysicalScreenTiltAngleSlider.value.ToString("##0.0"));
            ScreenHeight.SetTextWithoutNotify(ScreenHeightSlider.value.ToString("#0.00#"));
            TrackingOriginX.SetTextWithoutNotify(TrackingOriginXSlider.value.ToString("#0.00#"));
            TrackingOriginY.SetTextWithoutNotify(TrackingOriginYSlider.value.ToString("#0.00#"));
            TrackingOriginZ.SetTextWithoutNotify(TrackingOriginZSlider.value.ToString("#0.00#"));
            TrackingRotationX.SetTextWithoutNotify(TrackingRotationXSlider.value.ToString("##0.0"));
        }

        protected override void SaveValuesToConfig()
        {
            ConfigManager.PhysicalConfig.ScreenRotationD = TryParseNewStringToFloat(ref ConfigManager.PhysicalConfig.ScreenRotationD, PhysicalScreenTiltAngle.text);
            ConfigManager.PhysicalConfig.ScreenHeightM = TryParseNewStringToFloat(ref ConfigManager.PhysicalConfig.ScreenHeightM, ScreenHeight.text, true);
            ConfigManager.PhysicalConfig.LeapPositionRelativeToScreenBottomM = new Vector3(
                TryParseNewStringToFloat(ref ConfigManager.PhysicalConfig.LeapPositionRelativeToScreenBottomM.x, TrackingOriginX.text, true),
                TryParseNewStringToFloat(ref ConfigManager.PhysicalConfig.LeapPositionRelativeToScreenBottomM.y, TrackingOriginY.text, true),
                -TryParseNewStringToFloat(ref ConfigManager.PhysicalConfig.LeapPositionRelativeToScreenBottomM.z, TrackingOriginZ.text, true)
            );
            ConfigManager.PhysicalConfig.LeapRotationD = new Vector3(
                TryParseNewStringToFloat(ref ConfigManager.PhysicalConfig.LeapRotationD.x, TrackingRotationX.text),
                ConfigManager.PhysicalConfig.LeapRotationD.y,
                ConfigManager.PhysicalConfig.LeapRotationD.z
            );

            ConfigManager.PhysicalConfig.ConfigWasUpdated();

            RestartSaveConfigTimer();
        }

        public void ResetToDefaultValues()
        {
            ConfigManager.PhysicalConfig.SetAllValuesToDefault();
            ConfigManager.PhysicalConfig.SaveConfig();
            LoadConfigValuesIntoFields();
            SaveValuesToConfig();
        }

        private void OnInputFieldChanged(string _)
        {
            SaveValuesToConfig();
        }
    }
}
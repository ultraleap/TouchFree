using UnityEngine;
using UnityEngine.UI;

namespace Ultraleap.ScreenControl.Core
{
    public class ManualConfigUI : ConfigUI
    {
        public ConfigurationSetupController configController;

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
            PhysicalScreenTiltAngleSlider.minValue = PhysicalConfigurable.ScreenTilt_Min;
            ScreenHeightSlider.minValue = ScreenControlUtility.ToDisplayUnits(PhysicalConfigurable.ScreenHeight_Min);
            TrackingOriginXSlider.minValue = ScreenControlUtility.ToDisplayUnits(PhysicalConfigurable.TrackingOriginX_Min);
            TrackingOriginYSlider.minValue = ScreenControlUtility.ToDisplayUnits(PhysicalConfigurable.TrackingOriginY_Min);
            TrackingOriginZSlider.minValue = ScreenControlUtility.ToDisplayUnits(PhysicalConfigurable.TrackingOriginZ_Min);
            TrackingRotationXSlider.minValue = PhysicalConfigurable.TrackingRoation_Min;

            PhysicalScreenTiltAngleSlider.maxValue = PhysicalConfigurable.ScreenTilt_Max;
            ScreenHeightSlider.maxValue = ScreenControlUtility.ToDisplayUnits(PhysicalConfigurable.ScreenHeight_Max);
            TrackingOriginXSlider.maxValue = ScreenControlUtility.ToDisplayUnits(PhysicalConfigurable.TrackingOriginX_Max);
            TrackingOriginYSlider.maxValue = ScreenControlUtility.ToDisplayUnits(PhysicalConfigurable.TrackingOriginY_Max);
            TrackingOriginZSlider.maxValue = ScreenControlUtility.ToDisplayUnits(PhysicalConfigurable.TrackingOriginZ_Max);
            TrackingRotationXSlider.maxValue = PhysicalConfigurable.TrackingRoation_Max;
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
            PhysicalScreenTiltAngle.SetTextWithoutNotify(PhysicalConfigurable.Config.ScreenRotationD.ToString("##0.0"));
            ScreenHeight.SetTextWithoutNotify(ScreenControlUtility.ToDisplayUnits(PhysicalConfigurable.Config.ScreenHeightM).ToString("#0.00#"));
            TrackingOriginX.SetTextWithoutNotify(ScreenControlUtility.ToDisplayUnits(PhysicalConfigurable.Config.LeapPositionRelativeToScreenBottomM.x).ToString("#0.00#"));
            TrackingOriginY.SetTextWithoutNotify(ScreenControlUtility.ToDisplayUnits(PhysicalConfigurable.Config.LeapPositionRelativeToScreenBottomM.y).ToString("#0.00#"));
            TrackingOriginZ.SetTextWithoutNotify(ScreenControlUtility.ToDisplayUnits(-PhysicalConfigurable.Config.LeapPositionRelativeToScreenBottomM.z).ToString("#0.00#"));
            TrackingRotationX.SetTextWithoutNotify(PhysicalConfigurable.Config.LeapRotationD.x.ToString("##0.0"));

            var bottomMount = Mathf.Approximately(PhysicalConfigurable.Config.LeapRotationD.z, 0f);
            PhysicalScreenTiltAngleSlider.SetValueWithoutNotify(PhysicalConfigurable.Config.ScreenRotationD);
            ScreenHeightSlider.SetValueWithoutNotify(ScreenControlUtility.ToDisplayUnits(PhysicalConfigurable.Config.ScreenHeightM));
            TrackingOriginXSlider.SetValueWithoutNotify(ScreenControlUtility.ToDisplayUnits(PhysicalConfigurable.Config.LeapPositionRelativeToScreenBottomM.x));
            TrackingOriginYSlider.SetValueWithoutNotify(ScreenControlUtility.ToDisplayUnits(PhysicalConfigurable.Config.LeapPositionRelativeToScreenBottomM.y));
            TrackingOriginZSlider.SetValueWithoutNotify(ScreenControlUtility.ToDisplayUnits(-PhysicalConfigurable.Config.LeapPositionRelativeToScreenBottomM.z));
            TrackingRotationXSlider.SetValueWithoutNotify(PhysicalConfigurable.Config.LeapRotationD.x);
        }

        protected override void CommitValuesToFile()
        {
            PhysicalConfigurable.SaveConfig();
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
            var setup = PhysicalConfigurable.Config;

            setup.ScreenRotationD = TryParseNewStringToFloat(ref setup.ScreenRotationD, PhysicalScreenTiltAngle.text);
            setup.ScreenHeightM = TryParseNewStringToFloat(ref setup.ScreenHeightM, ScreenHeight.text, true);
            setup.LeapPositionRelativeToScreenBottomM = new Vector3(
                TryParseNewStringToFloat(ref setup.LeapPositionRelativeToScreenBottomM.x, TrackingOriginX.text, true),
                TryParseNewStringToFloat(ref setup.LeapPositionRelativeToScreenBottomM.y, TrackingOriginY.text, true),
                -TryParseNewStringToFloat(ref setup.LeapPositionRelativeToScreenBottomM.z, TrackingOriginZ.text, true)
            );
            setup.LeapRotationD = new Vector3(
                TryParseNewStringToFloat(ref setup.LeapRotationD.x, TrackingRotationX.text),
                setup.LeapRotationD.y,
                setup.LeapRotationD.z
            );

            PhysicalConfigurable.UpdateConfig(setup);
            RestartSaveConfigTimer();
        }

        public void ResetToDefaultValues()
        {
            PhysicalConfigurable.SetAllValuesToDefault();
            PhysicalConfigurable.SaveConfig();
            LoadConfigValuesIntoFields();
            SaveValuesToConfig();
        }

        private void OnInputFieldChanged(string _)
        {
            SaveValuesToConfig();
        }
    }
}
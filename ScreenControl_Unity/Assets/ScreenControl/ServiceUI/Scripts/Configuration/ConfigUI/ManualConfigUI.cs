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

        public GameObject resetToDefaultWarning;

        private void Awake()
        {
            resetToDefaultWarning.SetActive(false);
        }

        protected override void AddValueChangedListeners()
        {
            PhysicalScreenTiltAngle.onEndEdit.AddListener(OnInputFieldChanged);
            ScreenHeight.onEndEdit.AddListener(OnInputFieldChanged);
            TrackingOriginX.onEndEdit.AddListener(OnInputFieldChanged);
            TrackingOriginY.onEndEdit.AddListener(OnInputFieldChanged);
            TrackingOriginZ.onEndEdit.AddListener(OnInputFieldChanged);
            TrackingRotationX.onEndEdit.AddListener(OnInputFieldChanged);
        }

        protected override void RemoveValueChangedListeners()
        {
            PhysicalScreenTiltAngle.onEndEdit.RemoveListener(OnInputFieldChanged);
            ScreenHeight.onEndEdit.RemoveListener(OnInputFieldChanged);
            TrackingOriginX.onEndEdit.RemoveListener(OnInputFieldChanged);
            TrackingOriginY.onEndEdit.RemoveListener(OnInputFieldChanged);
            TrackingOriginZ.onEndEdit.RemoveListener(OnInputFieldChanged);
            TrackingRotationX.onEndEdit.RemoveListener(OnInputFieldChanged);
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
        }

        protected override void CommitValuesToFile()
        {
            ConfigManager.PhysicalConfig.SaveConfig();
        }

        protected override void ValidateValues()
        {
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
            ConfigManager.PhysicalConfig.SaveConfig();
        }

        public void ResetToDefaultValues()
        {
            ConfigManager.PhysicalConfig.SetAllValuesToDefault();
            ConfigManager.PhysicalConfig.ConfigWasUpdated();
            ConfigManager.PhysicalConfig.SaveConfig();
            LoadConfigValuesIntoFields();
        }

        private void OnInputFieldChanged(string _)
        {
            SaveValuesToConfig();
        }
    }
}
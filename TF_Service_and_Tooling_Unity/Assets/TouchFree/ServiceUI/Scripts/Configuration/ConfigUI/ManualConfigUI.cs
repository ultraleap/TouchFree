using UnityEngine;
using UnityEngine.UI;
using Ultraleap.TouchFree.ServiceShared;

namespace Ultraleap.TouchFree.ServiceUI
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

        protected override void OnEnable()
        {
            base.OnEnable();
            VirtualScreen.CaptureCurrentResolution();
            SaveValuesToConfig();
        }

        protected override void AddValueChangedListeners()
        {
            PhysicalScreenTiltAngle.onEndEdit.AddListener(OnValueChanged);
            ScreenHeight.onEndEdit.AddListener(OnValueChanged);
            TrackingOriginX.onEndEdit.AddListener(OnValueChanged);
            TrackingOriginY.onEndEdit.AddListener(OnValueChanged);
            TrackingOriginZ.onEndEdit.AddListener(OnValueChanged);
            TrackingRotationX.onEndEdit.AddListener(OnValueChanged);
        }

        protected override void RemoveValueChangedListeners()
        {
            PhysicalScreenTiltAngle.onEndEdit.RemoveListener(OnValueChanged);
            ScreenHeight.onEndEdit.RemoveListener(OnValueChanged);
            TrackingOriginX.onEndEdit.RemoveListener(OnValueChanged);
            TrackingOriginY.onEndEdit.RemoveListener(OnValueChanged);
            TrackingOriginZ.onEndEdit.RemoveListener(OnValueChanged);
            TrackingRotationX.onEndEdit.RemoveListener(OnValueChanged);
        }

        protected override void LoadConfigValuesIntoFields()
        {
            PhysicalScreenTiltAngle.SetTextWithoutNotify(
                ConfigManager.PhysicalConfig.ScreenRotationD.ToString("##0.0"));

            ScreenHeight.SetTextWithoutNotify(
                ServiceUtility.ToDisplayUnits(
                    ConfigManager.PhysicalConfig.ScreenHeightM
                ).ToString("#0.00#"));

            TrackingOriginX.SetTextWithoutNotify(
                ServiceUtility.ToDisplayUnits(
                    ConfigManager.PhysicalConfig.LeapPositionRelativeToScreenBottomM.x
                ).ToString("#0.00#"));

            TrackingOriginY.SetTextWithoutNotify(
                ServiceUtility.ToDisplayUnits(
                    ConfigManager.PhysicalConfig.LeapPositionRelativeToScreenBottomM.y
                ).ToString("#0.00#"));

            TrackingOriginZ.SetTextWithoutNotify(
                ServiceUtility.ToDisplayUnits(-
                    ConfigManager.PhysicalConfig.LeapPositionRelativeToScreenBottomM.z
                ).ToString("#0.00#"));


            // Convert from above screen rotaitons to a readable format
            if(Mathf.Approximately(ConfigManager.PhysicalConfig.LeapRotationD.z, 180))
            {
                TrackingRotationX.SetTextWithoutNotify(ServiceUtility.CentreRotationAroundZero((-ConfigManager.PhysicalConfig.LeapRotationD.x) -180).ToString("##0.0"));
            }
            else
            {
                TrackingRotationX.SetTextWithoutNotify(ConfigManager.PhysicalConfig.LeapRotationD.x.ToString("##0.0"));
            }
        }

        protected override void CommitValuesToFile()
        {
            ConfigManager.PhysicalConfig.SaveConfig();
        }

        protected override void ValidateValues()
        {
            PhysicalScreenTiltAngle.SetTextWithoutNotify(
                TryParseNewStringToFloat(ref ConfigManager.PhysicalConfig.ScreenRotationD,
                PhysicalScreenTiltAngle.text).ToString("##0.0"));

            ScreenHeight.SetTextWithoutNotify(
                ServiceUtility.ToDisplayUnits(
                    TryParseNewStringToFloat(ref ConfigManager.PhysicalConfig.ScreenHeightM, ScreenHeight.text, true)
                ).ToString("#0.00#"));

            TrackingOriginX.SetTextWithoutNotify(
                ServiceUtility.ToDisplayUnits(
                    TryParseNewStringToFloat(ref ConfigManager.PhysicalConfig.LeapPositionRelativeToScreenBottomM.x, TrackingOriginX.text, true)
                ).ToString("#0.00#"));

            TrackingOriginY.SetTextWithoutNotify(
                ServiceUtility.ToDisplayUnits(
                    TryParseNewStringToFloat(ref ConfigManager.PhysicalConfig.LeapPositionRelativeToScreenBottomM.y, TrackingOriginY.text, true)
                ).ToString("#0.00#"));

            TrackingOriginZ.SetTextWithoutNotify(
                ServiceUtility.ToDisplayUnits(
                    TryParseNewStringToFloat(ref ConfigManager.PhysicalConfig.LeapPositionRelativeToScreenBottomM.z, TrackingOriginZ.text, true)
                ).ToString("#0.00#"));


            // Convert from screen rotaitons to a readable format
            float backupLeapX = ConfigManager.PhysicalConfig.LeapRotationD.x;
            if (Mathf.Approximately(ConfigManager.PhysicalConfig.LeapRotationD.x, 180))
            {
                backupLeapX = (-ConfigManager.PhysicalConfig.LeapRotationD.x) - 180;
            }

            TrackingRotationX.SetTextWithoutNotify(ServiceUtility.CentreRotationAroundZero(
                TryParseNewStringToFloat(ref backupLeapX,
                TrackingRotationX.text)).ToString("##0.0"));
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

            // Convert from readable format to usable rotations
            float usableLeapX = TryParseNewStringToFloat(ref ConfigManager.PhysicalConfig.LeapRotationD.x, TrackingRotationX.text);
            float usableLeapZ = ConfigManager.PhysicalConfig.LeapRotationD.z;

            if(Mathf.Abs(usableLeapX) > 90)
            {
                // Above
                usableLeapZ = 180;
                usableLeapX = ServiceUtility.CentreRotationAroundZero((-usableLeapX) + 180);
            }
            else
            {
                // Below
                usableLeapZ = 0;
            }

            ConfigManager.PhysicalConfig.LeapRotationD = new Vector3(usableLeapX,
                ConfigManager.PhysicalConfig.LeapRotationD.y,
                usableLeapZ
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
    }
}
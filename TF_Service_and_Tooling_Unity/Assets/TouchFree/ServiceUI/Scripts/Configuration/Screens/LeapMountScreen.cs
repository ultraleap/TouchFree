using System.Collections;
using UnityEngine;
using Ultraleap.TouchFree.ServiceShared;

namespace Ultraleap.TouchFree.ServiceUI
{
    public class LeapMountScreen : ConfigScreen
    {
        public GameObject trackingServiceWarning;

        [Space]
        public GameObject guideWarning;

        [Space]
        public GameObject aboveFacingScreenCurrent;
        public GameObject belowMountedCurrent;
        public GameObject aboveFacingUserCurrent;

        [Space]
        public GameObject nextScreen;

        protected override void OnEnable()
        {
            base.OnEnable();

            ShowCurrentMount();

            // Check to see if we need to warn that no tracking service is connected
            if (HandManager.Instance.IsLeapServiceConnected())
            {
                trackingServiceWarning.SetActive(false);
            }
            else
            {
                trackingServiceWarning.SetActive(true);
            }

            // Check if the physicalconfig is set to default and guide the users if it is
            var defaultConfig = PhysicalConfigFile.GetDefaultValues();

            if (ConfigManager.PhysicalConfig.ScreenHeightM == defaultConfig.ScreenHeightM &&
                ConfigManager.PhysicalConfig.LeapPositionRelativeToScreenBottomM == defaultConfig.LeapPositionRelativeToScreenBottomM)
            {
                StartCoroutine(EnableWarningAfterWait());
            }
            else
            {
                guideWarning.SetActive(false);
            }
        }

        void ShowCurrentMount()
        {
            belowMountedCurrent.SetActive(false);
            aboveFacingScreenCurrent.SetActive(false);
            aboveFacingUserCurrent.SetActive(false);

            // leap is looking down
            if (Mathf.Abs(ConfigManager.PhysicalConfig.LeapRotationD.z) > 90f)
            {
                if (ConfigManager.PhysicalConfig.LeapRotationD.x <= 0f)
                {
                    //Screentop
                    aboveFacingUserCurrent.SetActive(true);
                }
                else
                {
                    //HMD
                    aboveFacingScreenCurrent.SetActive(true);
                }
            }
            else
            {
                //Desktop
                belowMountedCurrent.SetActive(true);
            }
        }

        IEnumerator EnableWarningAfterWait(float _wait = 0.5f)
        {
            yield return new WaitForSeconds(_wait);

            guideWarning.SetActive(true);
        }

        public void SetMode_AboveFacingUser()
        {
            ScreenManager.Instance.selectedMountType = MountingType.ABOVE_FACING_USER;
            SetTrackingModeAndContinue();
        }

        public void SetMode_AboveFacingScreen()
        {
            ScreenManager.Instance.selectedMountType = MountingType.ABOVE_FACING_SCREEN;
            SetTrackingModeAndContinue();
        }

        public void SetMode_Below()
        {
            ScreenManager.Instance.selectedMountType = MountingType.BELOW;
            SetTrackingModeAndContinue();
        }

        void SetTrackingModeAndContinue()
        {
            HandManager.Instance.SetLeapTrackingMode(ScreenManager.Instance.selectedMountType);
            ScreenManager.Instance.ChangeScreen(nextScreen);
        }

        public void CloseTrackingWarning()
        {
            trackingServiceWarning.SetActive(false);
        }
    }
}
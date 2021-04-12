using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace Ultraleap.ScreenControl.Core
{
    public class LeapMountScreen : MonoBehaviour
    {
        public GameObject guideWarning;

        public GameObject HMDMountedCurrent;
        public GameObject bottomMountedCurrent;
        public GameObject screenTopMountedCurrent;

        public GameObject screenTopOption;

        [Space]
        public GameObject quickOrManualScreen;

        private void OnEnable()
        {
            // only show users the screentop option if they have the correct leap service
            if (HandManager.Instance.screenTopAvailable)
            {
                screenTopOption.SetActive(true);
            }
            else
            {
                screenTopOption.SetActive(false);
            }

            ShowCurrentMount();

            // find the leap config path to look for auto orientation
            string appdatapath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
            string leapConfigPath = Path.Combine(appdatapath, "Leap Motion", "Config.json");

            if (File.Exists(leapConfigPath))
            {
                foreach (var line in File.ReadAllLines(leapConfigPath))
                {
                    if (line.Contains("image_processing_auto_flip"))
                    {
                        // check if auto orientation is true and warn against it
                        if (line.Contains("true"))
                        {
                            StartCoroutine(EnableWarningAfterWait());
                            return;
                        }
                        else
                        {
                            guideWarning.SetActive(false);
                        }

                        break;
                    }
                }
            }

            //Check if the physicalconfig is set to default and guide the users if it is
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
            bottomMountedCurrent.SetActive(false);
            HMDMountedCurrent.SetActive(false);
            screenTopMountedCurrent.SetActive(false);

            // leap is looking down
            if (Mathf.Abs(ConfigManager.PhysicalConfig.LeapRotationD.z) > 90f)
            {
                if (HandManager.Instance.screenTopAvailable && ConfigManager.PhysicalConfig.LeapRotationD.x <= 0f)
                {
                    //Screentop
                    screenTopMountedCurrent.SetActive(true);
                }
                else
                {
                    //HMD
                    HMDMountedCurrent.SetActive(true);
                }

            }
            else
            {
                //Desktop
                bottomMountedCurrent.SetActive(true);
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
            ScreenManager.Instance.ChangeScreen(quickOrManualScreen);
        }
    }
}
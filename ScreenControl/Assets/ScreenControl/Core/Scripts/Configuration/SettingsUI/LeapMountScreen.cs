using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace Ultraleap.ScreenControl.Core
{
    public class LeapMountScreen : MonoBehaviour
    {
        public GameObject guideWarning;

        public GameObject topMountedCurrent;
        public GameObject bottomMountedCurrent;

        private void OnEnable()
        {
            // find the leap config path to look for auto orientation
            string appdatapath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
            string leapConfigPath = Path.Combine(appdatapath, "Leap Motion", "Config.json");

            bool enabling = false;

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
                            enabling = true;
                        }
                        else
                        {
                            guideWarning.SetActive(false);
                        }

                        break;
                    }
                }
            }

            if (!enabling)
            {
                //Check if the physicalconfig is set to default and guide the users if it is
                var defaultConfig = PhysicalConfigFile.GetDefaultValues();

                if (ConfigManager.PhysicalConfig.ScreenHeightM == defaultConfig.ScreenHeightM &&
                    ConfigManager.PhysicalConfig.LeapPositionRelativeToScreenBottomM == defaultConfig.LeapPositionRelativeToScreenBottomM)
                {
                    StartCoroutine(EnableWarningAfterWait());
                    enabling = true;
                }
                else
                {
                    guideWarning.SetActive(false);
                }
            }

            bottomMountedCurrent.SetActive(false);
            topMountedCurrent.SetActive(false);

            if (!enabling)
            {
                // show the user their currently selected mounting mode
                if (Mathf.Abs(ConfigManager.PhysicalConfig.LeapRotationD.z) > 90f)
                {
                    //top
                    topMountedCurrent.SetActive(true);
                }
                else
                {
                    //bottom
                    bottomMountedCurrent.SetActive(true);
                }
            }
        }

        IEnumerator EnableWarningAfterWait(float _wait = 0.5f)
        {
            yield return new WaitForSeconds(_wait);

            guideWarning.SetActive(true);
        }
    }
}
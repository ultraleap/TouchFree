using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Ultraleap.TouchFree.ServiceShared;

namespace Ultraleap.ScreenControl.Core
{
    public class HomeScreen : MonoBehaviour
    {
        bool lastFrameServiceConnected = true;

        public GameObject leapConnectedNotification;
        public GameObject leapDisconnectedNotification;

        [Space]
        public GameObject setupCameraScreen;
        public GameObject interactionSettingsScreen;
        public GameObject advancedSettingsScreen;

        public Text versionText;
        string versionPath;

        private void Awake()
        {
            versionPath = Path.Combine(Application.dataPath, "../Version.txt");
            PopulateVersion();
        }

        private void OnEnable()
        {
            lastFrameServiceConnected = true;

            leapConnectedNotification.SetActive(true);
            leapDisconnectedNotification.SetActive(false);
        }

        private void Update()
        {
            if (HandManager.Instance.IsLeapServiceConnected())
            {
                // show service is connected
                if (!lastFrameServiceConnected)
                {
                    leapConnectedNotification.SetActive(true);
                    leapDisconnectedNotification.SetActive(false);
                    lastFrameServiceConnected = true;
                }
            }
            else
            {
                if (lastFrameServiceConnected)
                {
                    leapConnectedNotification.SetActive(false);
                    leapDisconnectedNotification.SetActive(true);
                    lastFrameServiceConnected = false;
                }
            }
        }

        void PopulateVersion()
        {
            string version = "N/A";

            if (File.Exists(versionPath))
            {
                var fileLines = File.ReadAllLines(versionPath);
                foreach (var line in fileLines)
                {
                    if (line.Contains("ScreenControl Service Version"))
                    {
                        version = line.Replace("ScreenControl Service Version: ", "");
                        break;
                    }
                }
            }
            versionText.text = "Version " + version;
        }

        public void ChangeToSetupCamera()
        {
            ScreenManager.Instance.ChangeScreen(setupCameraScreen);
        }

        public void ChangeToInteractionSettings()
        {
            ScreenManager.Instance.ChangeScreen(interactionSettingsScreen);
        }

        public void ChangeToAdvancedSettings()
        {
            ScreenManager.Instance.ChangeScreen(advancedSettingsScreen);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Ultraleap.ScreenControl.Core
{
    public class HomeScreen : MonoBehaviour
    {
        public GameObject trackingServiceWarning;
        bool serviceWarningForceClosed = false;
        bool lastFrameServiceConnected = true;

        public GameObject leapConnectedNotification;
        public GameObject leapDisconnectedNotification;

        bool delayingUpdates = false;

        [Space]
        public GameObject setupCameraScreen;
        public GameObject interactionSettingsScreen;
        public GameObject advancedSettingsScreen;

        private void OnEnable()
        {
            serviceWarningForceClosed = false;
            lastFrameServiceConnected = true;

            trackingServiceWarning.SetActive(false);
            leapConnectedNotification.SetActive(true);
            leapDisconnectedNotification.SetActive(false);

            StartCoroutine(DelayUpdates());
        }

        IEnumerator DelayUpdates()
        {
            delayingUpdates = true;
            yield return new WaitForSeconds(0.5f);
            delayingUpdates = false;
        }

        private void Update()
        {
            if (delayingUpdates)
            {
                return;
            }

            if (HandManager.Instance.IsLeapServiceConnected())
            {
                // show service is connected
                if (!lastFrameServiceConnected)
                {
                    trackingServiceWarning.SetActive(false);
                    leapConnectedNotification.SetActive(true);
                    leapDisconnectedNotification.SetActive(false);
                    lastFrameServiceConnected = true;
                }
            }
            else
            {
                if (lastFrameServiceConnected)
                {
                    // show service is not connected
                    if (!serviceWarningForceClosed)
                    {
                        trackingServiceWarning.SetActive(true);
                    }

                    leapConnectedNotification.SetActive(false);
                    leapDisconnectedNotification.SetActive(true);
                    lastFrameServiceConnected = false;
                }
            }
        }

        public void ForceCloseWarning()
        {
            trackingServiceWarning.SetActive(false);
            serviceWarningForceClosed = true;
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
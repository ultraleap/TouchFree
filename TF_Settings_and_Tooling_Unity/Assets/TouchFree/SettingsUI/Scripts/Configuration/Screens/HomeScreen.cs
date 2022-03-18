using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Ultraleap.TouchFree.ServiceShared;

namespace Ultraleap.TouchFree.ServiceUI
{
    public class HomeScreen : ConfigScreen
    {
        bool lastFrameServiceConnected = true;

        public GameObject leapConnectedNotification;
        public GameObject leapDisconnectedNotification;

        [Space]
        public GameObject setupCameraScreen;
        public GameObject interactionSettingsScreen;
        public GameObject overlayVisualsScreen;
        public GameObject advancedSettingsScreen;

        private void Awake()
        {
            if (DiagnosticAPIManager.diagnosticAPI == null)
            {
                DiagnosticAPIManager.diagnosticAPI = new DiagnosticAPI(this);
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
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

        public void ChangeToOverlayVisuals()
        {
            ScreenManager.Instance.ChangeScreen(overlayVisualsScreen);
        }

        public void ChangeToAdvancedSettings()
        {
            ScreenManager.Instance.ChangeScreen(advancedSettingsScreen);
        }
    }
}
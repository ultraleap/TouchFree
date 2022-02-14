﻿using UnityEngine;
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

        public Text versionText;
        string versionPath;

        private void Awake()
        {
            versionPath = Path.Combine(Application.dataPath, "../Version.txt");
            PopulateVersion();
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

        void PopulateVersion()
        {
            string version = "N/A";

            if (File.Exists(versionPath))
            {
                var fileLines = File.ReadAllLines(versionPath);
                foreach (var line in fileLines)
                {
                    if (line.Contains("TouchFree Service Version"))
                    {
                        version = line.Replace("TouchFree Service Version: ", "");
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
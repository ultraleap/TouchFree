using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Ultraleap.ScreenControl.Core
{
    public enum ConfigState
    {
        WELCOME,
        LEAP_MOUNT,
        AUTO_OR_MANUAL,
        AUTO,
        AUTO_COMPLETE,
        MANUAL,
        SETTINGS,
        FILE_SCREEN,
        TEST_CALIBRATION
    }

    public enum MountingType
    {
        NONE,
        BELOW,
        ABOVE_FACING_USER,
        ABOVE_FACING_SCREEN
    }

    public class ConfigurationSetupController : MonoBehaviour
    {
        public static ConfigurationSetupController Instance;
        public static ConfigState currentState;
        ConfigState previousState;

        public GameObject clientRootObj;
        public GameObject[] stateRoots;

        public static MountingType selectedMountType = MountingType.NONE;

        public void ChangeState(ConfigState _newState)
        {
            previousState = currentState;
            currentState = _newState;
            EnableCurrentState();
        }

        void EnableCurrentState()
        {
            HandManager.Instance.useTrackingTransform = true;
            foreach (var root in stateRoots)
            {
                root.SetActive(false);
            }

            stateRoots[(int)currentState].SetActive(true);
        }

        string manualConfigKeyEntered;
        public void ManualKeyEntry(string _keyEntered)
        {
            manualConfigKeyEntered = _keyEntered.ToUpper();
        }

        private void Start()
        {
            Instance = this;
        }

        private void Update()
        {
            if (manualConfigKeyEntered == "C" || Input.GetKeyDown(KeyCode.Escape))
            {
                if (currentState == ConfigState.WELCOME)
                {
                    OnMinimizeButtonClick();
                }
                else
                {
                    ChangeState(ConfigState.WELCOME);
                    HandManager.Instance.UpdateLeapTrackingMode();
                }
            }

            switch (currentState)
            {
                case ConfigState.WELCOME:
                    RunWelcomeScreen();
                    break;
                case ConfigState.LEAP_MOUNT:
                    RunLeapMount();
                    break;
                case ConfigState.AUTO_OR_MANUAL:
                    RunAutoOrManual();
                    break;
                case ConfigState.AUTO_COMPLETE:
                    RunAutoConfigComplete();
                    break;
                case ConfigState.MANUAL:
                    RunManualConfig();
                    break;
                case ConfigState.SETTINGS:
                    RunSettings();
                    break;
                case ConfigState.FILE_SCREEN:
                    RunFileScreen();
                    break;
                case ConfigState.TEST_CALIBRATION:
                    RunCalibrationTest();
                    break;
                case ConfigState.AUTO:
                    RunAutoConfig();
                    break;
            }

            manualConfigKeyEntered = "";
        }

        void RunWelcomeScreen()
        {
            if (manualConfigKeyEntered == "S")
            {
                ChangeState(ConfigState.LEAP_MOUNT);
            }
            else if (manualConfigKeyEntered == "U")
            {
                ChangeState(ConfigState.SETTINGS);
            }

            if (manualConfigKeyEntered == "F")
            {
                ChangeState(ConfigState.FILE_SCREEN);
            }

            if (manualConfigKeyEntered == "SUPPORT")
            {
                Application.OpenURL("http://rebrand.ly/ul-contact-us");
            }

            if (manualConfigKeyEntered == "SETUPGUIDE")
            {
                Application.OpenURL("http://rebrand.ly/ul-camera-setup");
            }
        }

        void RunLeapMount()
        {
            if (manualConfigKeyEntered == "ABOVE_FACING_USER")
            {
                // To help with Auto and manual, we set the tracking mode here. This way the user is in the correct mode for running a Setup
                selectedMountType = MountingType.ABOVE_FACING_USER;
                HandManager.Instance.SetLeapTrackingMode(selectedMountType);
                ChangeState(ConfigState.AUTO_OR_MANUAL);
            }
            else if (manualConfigKeyEntered == "BELOW")
            {
                // To help with Auto and manual, we set the tracking mode here. This way the user is in the correct mode for running a Setup
                selectedMountType = MountingType.BELOW;
                HandManager.Instance.SetLeapTrackingMode(selectedMountType);
                ChangeState(ConfigState.AUTO_OR_MANUAL);
            }
            else if (manualConfigKeyEntered == "ABOVE_FACING_SCREEN")
            {
                // To help with Auto and manual, we set the tracking mode here. This way the user is in the correct mode for running a Setup
                selectedMountType = MountingType.ABOVE_FACING_SCREEN;
                HandManager.Instance.SetLeapTrackingMode(selectedMountType);
                ChangeState(ConfigState.AUTO_OR_MANUAL);
            }

            if (manualConfigKeyEntered == "SETUPGUIDE")
            {
                Application.OpenURL("http://rebrand.ly/ul-camera-setup");
            }
        }

        void RunManualConfig()
        {
            // this is handled on the manual config screen object

            if (manualConfigKeyEntered == "T")
            {
                ChangeState(ConfigState.TEST_CALIBRATION);
            }
        }

        void RunAutoOrManual()
        {
            if (manualConfigKeyEntered == "A")
            {
                ChangeState(ConfigState.AUTO);
            }
            else if (manualConfigKeyEntered == "M")
            {
                // immediately use the selected mount type for manual setup
                bool wasBottomMounted = Mathf.Approximately(0, ConfigManager.PhysicalConfig.LeapRotationD.z);

                if (wasBottomMounted && (selectedMountType == MountingType.ABOVE_FACING_SCREEN || selectedMountType == MountingType.ABOVE_FACING_USER))
                {
                    ConfigManager.PhysicalConfig.LeapRotationD = new Vector3(
                        -ConfigManager.PhysicalConfig.LeapRotationD.x,
                        ConfigManager.PhysicalConfig.LeapRotationD.y,
                        180f);
                }
                else if (!wasBottomMounted && selectedMountType == MountingType.BELOW)
                {
                    ConfigManager.PhysicalConfig.LeapRotationD = new Vector3(
                        -ConfigManager.PhysicalConfig.LeapRotationD.x,
                        ConfigManager.PhysicalConfig.LeapRotationD.y, 0f);
                }

                ConfigManager.PhysicalConfig.ConfigWasUpdated();

                // dont allow manual to flip the axes again
                selectedMountType = MountingType.NONE;
                ChangeState(ConfigState.MANUAL);
            }
        }

        void RunAutoConfig()
        {
            if (manualConfigKeyEntered == "SETUPGUIDE")
            {
                Application.OpenURL("http://rebrand.ly/ul-camera-setup");
            }
        }

        void RunAutoConfigComplete()
        {
            if (manualConfigKeyEntered == "M")
            {
                selectedMountType = MountingType.NONE;
                ChangeState(ConfigState.MANUAL);
            }
            else if (manualConfigKeyEntered == "A")
            {
                ChangeState(ConfigState.AUTO);
            }
            else if (manualConfigKeyEntered == "T")
            {
                ChangeState(ConfigState.TEST_CALIBRATION);
            }
        }

        void RunSettings()
        {
            // this is handled on the settings config screen object
            if (manualConfigKeyEntered == "T")
            {
                ChangeState(ConfigState.TEST_CALIBRATION);
            }

            if (manualConfigKeyEntered == "DESIGNGUIDE")
            {
                Application.OpenURL("http://rebrand.ly/ul-design-guidelines");
            }
        }

        void RunFileScreen()
        {
            // this is run on the file screen object
            if (manualConfigKeyEntered == "SUPPORT")
            {
                Application.OpenURL("http://rebrand.ly/ul-contact-us");
            }

            if (manualConfigKeyEntered == "SETUPGUIDE")
            {
                Application.OpenURL("http://rebrand.ly/ul-touchfree-setup");
            }
        }

        void RunCalibrationTest()
        {
            if (manualConfigKeyEntered == "B")
            {
                GoToPreviousState();
            }

            if (manualConfigKeyEntered == "R")
            {
                ChangeState(ConfigState.LEAP_MOUNT);
            }
        }

        void GoToPreviousState()
        {
            ChangeState(previousState);
        }

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);
        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        public void OnMinimizeButtonClick()
        {
            clientRootObj.SetActive(false);
            ShowWindow(GetActiveWindow(), 2);
        }
    }
}
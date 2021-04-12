using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Ultraleap.ScreenControl.Core
{
    public enum ConfigScreenState
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
        public static ConfigScreenState currentState;
        ConfigScreenState previousState;

        public GameObject clientRootObj;
        public GameObject[] stateRoots;

        public static MountingType selectedMountType = MountingType.NONE;

        private PhysicalConfig defaultConfig = null;

        public void ChangeState(ConfigScreenState _newState)
        {
            previousState = currentState;
            currentState = _newState;
            EnableCurrentState();
            UpdateCursorState();
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
            UpdateCursorState();
        }

        public void SetCursorState(bool _state)
        {
            clientRootObj.SetActive(_state);
        }

        void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                UpdateCursorState();
            }
            else
            {
                clientRootObj.SetActive(hasFocus);
            }
        }

        private void UpdateCursorState()
        {
            if (defaultConfig == null)
            {
                defaultConfig = PhysicalConfigFile.GetDefaultValues();
            }

            //Check if the physicalconfig is set to default and guide the users if it is
            if (ConfigManager.PhysicalConfig.ScreenHeightM == defaultConfig.ScreenHeightM &&
                ConfigManager.PhysicalConfig.LeapPositionRelativeToScreenBottomM == defaultConfig.LeapPositionRelativeToScreenBottomM)
            {
                clientRootObj.SetActive(false);
            }
            else if(currentState != ConfigState.AUTO)
            {
                clientRootObj.SetActive(true);
            }
        }

        private void Update()
        {
            if (manualConfigKeyEntered == "C" || Input.GetKeyDown(KeyCode.Escape))
            {
                if (currentState == ConfigScreenState.WELCOME)
                {
                    OnMinimizeButtonClick();
                }
                else
                {
                    ChangeState(ConfigScreenState.WELCOME);
                    HandManager.Instance.UpdateLeapTrackingMode();
                }
            }

            switch (currentState)
            {
                case ConfigScreenState.WELCOME:
                    RunWelcomeScreen();
                    break;
                case ConfigScreenState.LEAP_MOUNT:
                    RunLeapMount();
                    break;
                case ConfigScreenState.AUTO_OR_MANUAL:
                    RunAutoOrManual();
                    break;
                case ConfigScreenState.AUTO_COMPLETE:
                    RunAutoConfigComplete();
                    break;
                case ConfigScreenState.MANUAL:
                    RunManualConfig();
                    break;
                case ConfigScreenState.SETTINGS:
                    RunSettings();
                    break;
                case ConfigScreenState.FILE_SCREEN:
                    RunFileScreen();
                    break;
                case ConfigScreenState.TEST_CALIBRATION:
                    RunCalibrationTest();
                    break;
                case ConfigScreenState.AUTO:
                    RunAutoConfig();
                    break;
            }

            manualConfigKeyEntered = "";
        }

        void RunWelcomeScreen()
        {
            if (manualConfigKeyEntered == "S")
            {
                ChangeState(ConfigScreenState.LEAP_MOUNT);
            }
            else if (manualConfigKeyEntered == "U")
            {
                ChangeState(ConfigScreenState.SETTINGS);
            }

            if (manualConfigKeyEntered == "F")
            {
                ChangeState(ConfigScreenState.FILE_SCREEN);
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
                ChangeState(ConfigScreenState.AUTO_OR_MANUAL);
            }
            else if (manualConfigKeyEntered == "BELOW")
            {
                // To help with Auto and manual, we set the tracking mode here. This way the user is in the correct mode for running a Setup
                selectedMountType = MountingType.BELOW;
                HandManager.Instance.SetLeapTrackingMode(selectedMountType);
                ChangeState(ConfigScreenState.AUTO_OR_MANUAL);
            }
            else if (manualConfigKeyEntered == "ABOVE_FACING_SCREEN")
            {
                // To help with Auto and manual, we set the tracking mode here. This way the user is in the correct mode for running a Setup
                selectedMountType = MountingType.ABOVE_FACING_SCREEN;
                HandManager.Instance.SetLeapTrackingMode(selectedMountType);
                ChangeState(ConfigScreenState.AUTO_OR_MANUAL);
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
                ChangeState(ConfigScreenState.TEST_CALIBRATION);
            }
        }

        void RunAutoOrManual()
        {
            if (manualConfigKeyEntered == "A")
            {
                ChangeState(ConfigScreenState.AUTO);
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
                ChangeState(ConfigScreenState.MANUAL);
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
                ChangeState(ConfigScreenState.MANUAL);
            }
            else if (manualConfigKeyEntered == "A")
            {
                ChangeState(ConfigScreenState.AUTO);
            }
            else if (manualConfigKeyEntered == "T")
            {
                ChangeState(ConfigScreenState.TEST_CALIBRATION);
            }
        }

        void RunSettings()
        {
            // this is handled on the settings config screen object
            if (manualConfigKeyEntered == "T")
            {
                ChangeState(ConfigScreenState.TEST_CALIBRATION);
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
                ChangeState(ConfigScreenState.LEAP_MOUNT);
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
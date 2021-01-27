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
        BOTTOM,
        TOP_DOWN,
        OVERHEAD
    }

    public class ConfigurationSetupController : MonoBehaviour
    {
        public static ConfigurationSetupController Instance;

        public static ConfigState currentState;
        ConfigState previousState;

        public static event Action OnConfigActive;
        public static event Action OnConfigInactive;

        public static event Action EnableInteractions;
        public static event Action DisableInteractions;

        private bool setupScreenActive = false;

        public GameObject[] stateRoots;
        public GameObject configCanvas;

        [HideInInspector] public bool closeConfig = false;

        public static MountingType selectedMountType = MountingType.NONE;

        bool openedOnStart = false;

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
                root.SetActive(false);

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
            if (!setupScreenActive)
            {
                if (Input.GetKeyDown(KeyCode.C) || !openedOnStart)
                {
                    setupScreenActive = true;
                    DisableInteractions?.Invoke();
                    // display default autoconfig screen
                    configCanvas.SetActive(true);

                    ChangeState(ConfigState.WELCOME);
                    OnConfigActive?.Invoke();
                    openedOnStart = true;
                }
            }
            else
            {
                if (closeConfig || manualConfigKeyEntered == "C" || Input.GetKeyDown(KeyCode.Escape))
                {
                    if (currentState == ConfigState.WELCOME)
                    {
                        OnMinimizeButtonClick();
                        //setupScreenActive = false;
                        //EnableInteractions?.Invoke();
                        //HandManager.Instance.useTrackingTransform = true;
                        //configCanvas.SetActive(false);
                        //OnConfigInactive?.Invoke();
                        //closeConfig = false;
                    }
                    else
                    {
                        ChangeState(ConfigState.WELCOME);
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
                        break;
                }
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
            if (manualConfigKeyEntered == "T")
            {
                selectedMountType = MountingType.OVERHEAD;
                ChangeState(ConfigState.AUTO_OR_MANUAL);
            }
            else if (manualConfigKeyEntered == "B")
            {
                selectedMountType = MountingType.BOTTOM;
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
                // immediately use the selecte dmount type for manual setup
                bool wasBottomMounted = Mathf.Approximately(0, ConfigManager.PhysicalConfig.LeapRotationD.z);

                if (wasBottomMounted && selectedMountType == MountingType.OVERHEAD)
                {
                    ConfigManager.PhysicalConfig.LeapRotationD = new Vector3(
                        -ConfigManager.PhysicalConfig.LeapRotationD.x,
                        ConfigManager.PhysicalConfig.LeapRotationD.y,
                        180f);
                }
                else if (!wasBottomMounted && selectedMountType == MountingType.BOTTOM)
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

        public void RefreshConfigActive()
        {
            OnConfigInactive?.Invoke();
            OnConfigActive?.Invoke();
        }

        public void RefreshConfigInactive()
        {
            OnConfigActive?.Invoke();
            OnConfigInactive?.Invoke();
        }

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);
        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        public void OnMinimizeButtonClick()
        {
            ShowWindow(GetActiveWindow(), 2);
        }
    }
}
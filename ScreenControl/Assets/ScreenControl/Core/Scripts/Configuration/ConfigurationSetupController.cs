using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;

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

    public static bool isActive = false;

    public static event Action OnConfigActive;
    public static event Action OnConfigInactive;

    public static event Action EnableInteractions;
    public static event Action DisableInteractions;

    public static event Action EnableCursorVisuals;
    public static event Action DisableCursorVisuals;

    private bool setupScreenActive = false;

    public GameObject[] stateRoots;
    public GameObject configCanvas;
    public bool startWithConfig = true;

    [HideInInspector] public bool closeConfig = false;

    public static MountingType selectedMountType = MountingType.NONE;

    bool isFocussed = false;

    public static void SetCursorVisual(bool _enabled)
    {
        if(_enabled)
        {
            EnableCursorVisuals?.Invoke();
        }
        else
        {
            DisableCursorVisuals?.Invoke();
        }
    }

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
        startWithConfig = SettingsConfig.Config.ShowSetupScreenOnStartup;
    }

    private void Update()
    {
        if (!setupScreenActive)
        {
            if (Input.GetKeyDown(KeyCode.C) || startWithConfig)
            {
                setupScreenActive = true;
                DisableInteractions?.Invoke();
                // display default autoconfig screen
                configCanvas.SetActive(true);

                startWithConfig = false;
                ChangeState(ConfigState.WELCOME);
                OnConfigActive?.Invoke();
                isActive = true;
            }
        }
        else
        {
            if (closeConfig || manualConfigKeyEntered == "C" || Input.GetKeyDown(KeyCode.Escape))
            {
                if (currentState == ConfigState.WELCOME)
                {
                    setupScreenActive = false;
                    EnableInteractions?.Invoke();
                    HandManager.Instance.useTrackingTransform = true;
                    configCanvas.SetActive(false);
                    OnConfigInactive?.Invoke();
                    closeConfig = false;
                    isActive = false;
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
                case ConfigState.AUTO:
                    RunAutoConfig();
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

        if(manualConfigKeyEntered == "SUPPORT")
        {
            Application.OpenURL("http://rebrand.ly/ul-contact-us");
            //closeConfig = true;
        }

        if (manualConfigKeyEntered == "SETUPGUIDE")
        {
            Application.OpenURL("http://rebrand.ly/ul-camera-setup");
            //closeConfig = true;
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
            //closeConfig = true;
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
            var setup = PhysicalConfigurable.Config;
            bool wasBottomMounted = Mathf.Approximately(0, setup.LeapRotationD.z);

            if(wasBottomMounted && selectedMountType == MountingType.OVERHEAD)
            {
                setup.LeapRotationD = new Vector3(-setup.LeapRotationD.x, setup.LeapRotationD.y, 180f);
                PhysicalConfigurable.UpdateConfig(setup);
            }
            else if(!wasBottomMounted && selectedMountType == MountingType.BOTTOM)
            {
                setup.LeapRotationD = new Vector3(-setup.LeapRotationD.x, setup.LeapRotationD.y, 0f);
                PhysicalConfigurable.UpdateConfig(setup);
            }

            // dont alloe manual to flip the axes again
            selectedMountType = MountingType.NONE;
            ChangeState(ConfigState.MANUAL);
        }
    }

    void RunAutoConfig()
    {
        // this is handled on the auto config screen object
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
            //closeConfig = true;
        }        
    }

    void RunFileScreen()
    {
        // this is run on the file screen object

        if (manualConfigKeyEntered == "SUPPORT")
        {
            Application.OpenURL("http://rebrand.ly/ul-contact-us");
            //closeConfig = true;
        }

        if (manualConfigKeyEntered == "SETUPGUIDE")
        {
            Application.OpenURL("http://rebrand.ly/ul-touchfree-setup");
            //closeConfig = true;
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
}
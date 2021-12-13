using System.Collections.Generic;
using System.Globalization;
using System.Threading;

using UnityEngine;
using Ultraleap.TouchFree.ServiceShared;

namespace Ultraleap.TouchFree.ServiceUI
{
    public class ScreenManager : MonoBehaviour
    {
        public static ScreenManager Instance;

        public GameObject clientRootObj;
        public GameObject[] stateRoots;
        public GameObject homeScreen;
        GameObject currentScreen;
        List<GameObject> previousScreens = new List<GameObject>();

        [HideInInspector] public MountingType selectedMountType = MountingType.NONE;

        private PhysicalConfig defaultConfig = null;
        bool cursorStateOverridden = false;

        [RuntimeInitializeOnLoadMethod]
        void EnsureCorrectLanguageCulture()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-GB");
        }

        public void ChangeScreen(GameObject _newScreenRoot, bool _movingBack = false)
        {
            if (currentScreen == null)
            {
                currentScreen = homeScreen;
            }

            if (!_movingBack)
            {
                previousScreens.Add(currentScreen);
            }

            foreach (var root in stateRoots)
            {
                root.SetActive(false);
            }

            HandManager.Instance.lockTrackingMode = false;
            _newScreenRoot.SetActive(true);
            currentScreen = _newScreenRoot;
        }

        private void Start()
        {
            Instance = this;
            UpdateCursorState();

            // Never use TrackingTransform in UI scene, tracking is only used for
            // Quick Setup here
            HandManager.Instance.useTrackingTransform = false;
            EnsureCorrectLanguageCulture();
        }

        public void SetCursorState(bool _state)
        {
            clientRootObj.SetActive(_state);
            cursorStateOverridden = !_state;
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
            if (cursorStateOverridden)
            {
                return;
            }

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
            else
            {
                clientRootObj.SetActive(true);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (homeScreen.activeSelf)
                {
                    CloseApplication();
                }
                else
                {
                    ReturnToHome();
                }
            }
        }

        public void SupportPressed()
        {
            Application.OpenURL("http://rebrand.ly/ul-contact-us");
        }

        public void DesignGuidePressed()
        {
            Application.OpenURL("http://rebrand.ly/ul-design-guidelines");
        }

        public void SetupGuidePressed()
        {
            Application.OpenURL("http://rebrand.ly/ul-camera-setup");
        }

        public void ReturnToHome()
        {
            ChangeScreen(homeScreen);
        }

        public void PreviousScreen()
        {
            ChangeScreen(previousScreens[previousScreens.Count-1], true);
            previousScreens.RemoveAt(previousScreens.Count-1);
        }

        public void CloseApplication()
        {
            Application.Quit();
        }
    }
}
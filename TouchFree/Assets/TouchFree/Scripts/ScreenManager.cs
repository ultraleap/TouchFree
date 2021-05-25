using System.Collections.Generic;
using UnityEngine;
using System;

namespace Ultraleap.TouchFree
{
    [DefaultExecutionOrder(-1)]
    public class ScreenManager : MonoBehaviour
    {
        public static ScreenManager Instance;

        public static event Action UIActivated;
        public static event Action UIDeactivated;

        public bool isActive;

        public GameObject[] stateRoots;
        public GameObject homeScreen;
        GameObject currentScreen;
        List<GameObject> previousScreens = new List<GameObject>();

        private void Start()
        {
            Instance = this;
            Application.focusChanged += Application_focusChanged;
        }

        private void OnDestroy()
        {
            Application.focusChanged -= Application_focusChanged;
        }

        private void Application_focusChanged(bool _focussed)
        {
            if (_focussed)
            {
                SetUIActive(true);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (homeScreen.activeSelf)
                {
                    SetUIActive(false);
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

            CloseAllScreens();

            _newScreenRoot.SetActive(true);
            currentScreen = _newScreenRoot;
        }

        void CloseAllScreens()
        {
            foreach (var root in stateRoots)
            {
                root.SetActive(false);
            }
        }

        public void ReturnToHome()
        {
            ChangeScreen(homeScreen);
        }

        public void PreviousScreen()
        {
            ChangeScreen(previousScreens[previousScreens.Count - 1], true);
            previousScreens.RemoveAt(previousScreens.Count - 1);
        }

        private void SetUIActive(bool _setTo = true)
        {
            isActive = _setTo;

            if (isActive)
            {
                ReturnToHome();
                UIActivated?.Invoke();
            }
            else
            {
                CloseAllScreens();
                UIDeactivated?.Invoke();
            }
        }

        public void CloseApplication()
        {
            SetUIActive(false);
        }
    }
}
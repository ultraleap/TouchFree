using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Ultraleap.TouchFree
{
    [DefaultExecutionOrder(-1)]
    public class TouchFreeMain : MonoBehaviour
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        public static int CursorWindowSize = 200;
        public static Vector3 CursorWindowMiddle = new Vector2(100, 100);

        static int minCursorWindowSize = 50;
        static int maxCursorWindowSize = 500;

        bool settingsAppOpen = false;

        public GameObject toolingClientGameobject;
        public GameObject[] visibleCanvasses;

        void Awake()
        {
            Application.targetFrameRate = 60;
            StartCoroutine(CheckForSettingsAppState());
        }

        void OnEnable()
        {
            ConfigManager.OnConfigUpdated += ConfigUpdated;
            ConfigUpdated();
        }

        void OnDisable()
        {
            ConfigManager.OnConfigUpdated -= ConfigUpdated;
        }

        void ConfigUpdated()
        {
            if(ConfigManager.Config.cursorEnabled)
            {
                UpdateCursorWindowSize(ConfigManager.Config.cursorSizeCm);
            }
            else
            {
                // If we do not need to render a cursor, make it very small so reduce GPU load
                UpdateCursorWindowSize(2);
            }
        }

        public static void UpdateCursorWindowSize(float _cursorSize)
        {
            int newWindowSize = (int)Tooling.Utilities.MapRangeToRange(
                _cursorSize, 0.1f, 1f, minCursorWindowSize, maxCursorWindowSize);

            CursorWindowSize = newWindowSize;
            CursorWindowMiddle = Vector2.one * (CursorWindowSize / 2);
        }

        IEnumerator CheckForSettingsAppState()
        {
            WaitForSeconds wait = new WaitForSeconds(1);

            yield return wait;

            while (true)
            {
                IntPtr hwnd = FindWindow(null, "TouchFreeSettings");

                if ((int)hwnd != 0)
                {
                    if (!settingsAppOpen)
                    {
                        HandleSettingsAppOpened();
                    }

                    settingsAppOpen = true;
                }
                else
                {
                    if (settingsAppOpen)
                    {
                        HandleSettingsAppClosed();
                    }

                    settingsAppOpen = false;
                }

                yield return wait;
            }
        }

        void HandleSettingsAppOpened()
        {
            SetToolingClientActiveState(false);
            SetVisibleCanvassesActveState(false);
        }

        void HandleSettingsAppClosed()
        {
            SetToolingClientActiveState(true);
            SetVisibleCanvassesActveState(true);
            TouchFreeConfigFile.LoadConfig();
        }

        void SetToolingClientActiveState(bool _activate)
        {
            toolingClientGameobject.SetActive(_activate);
        }

        void SetVisibleCanvassesActveState(bool _activate)
        {
            foreach (var canvas in visibleCanvasses)
            {
                canvas.SetActive(_activate);
            }
        }
    }
}
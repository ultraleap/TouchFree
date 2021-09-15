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

        bool settingsOpen = false;
        public GameObject disableWhenSettingsOpen;

        static int minCursorWindowSize = 50;
        static int maxCursorWindowSize = 500;

        void Awake()
        {
            Application.targetFrameRate = 60;
            StartCoroutine(CheckForSettingsOpen());
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

        IEnumerator CheckForSettingsOpen()
        {
            WaitForSeconds wait = new WaitForSeconds(1);

            yield return wait;

            while (true)
            {
                IntPtr hwnd = FindWindow(null, "TouchFreeService");

                if ((int)hwnd != 0)
                {
                    if (!settingsOpen)
                    {
                        disableWhenSettingsOpen.SetActive(false);
                    }

                    settingsOpen = true;
                }
                else
                {
                    if (settingsOpen)
                    {
                        disableWhenSettingsOpen.SetActive(true);
                        TouchFreeConfigFile.LoadConfig();
                    }

                    settingsOpen = false;
                }

                yield return wait;
            }
        }
    }
}
using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Ultraleap.TouchFree
{
    [DefaultExecutionOrder(-10)]
    public class TransparentWindow : MonoBehaviour
    {
        private struct MARGINS
        {
            public int cxLeftWidth;
            public int cxRightWidth;
            public int cyTopHeight;
            public int cyBottomHeight;
        }

        [DllImport("user32.dll")]
        private static extern uint SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

        [DllImport("user32.dll")]
        private static extern uint SetWindowLong(IntPtr hWnd, int nIndex, long dwNewLong);

        [DllImport("user32.dll")]
        private static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetLayeredWindowAttributes")]
        static extern int SetLayeredWindowAttributes(IntPtr hwnd, int crKey, byte bAlpha, int dwFlags);

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        private static extern int SetWindowPos(IntPtr hwnd, int hwndInsertAfter, int x, int y, int cx, int cy, int uFlags);

        [DllImport("Dwmapi.dll")]
        private static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        const int GWL_STYLE = -16;
        const uint WS_POPUP = 0x80000000;
        const uint WS_VISIBLE = 0x10000000;
        const int HWND_TOPMOST = -1;

        const uint WS_EX_LAYERED = 0x00080000;
        const uint WS_EX_TRANSPARENT = 0x00000020;

        const int SWP_NOACTIVATE = 16;
        const int SWP_FRAMECHANGED = 32;
        const int SWP_SHOWWINDOW = 64;

        const int LWA_ALPHA = 2;

        public IntPtr hwnd;

        private Vector2 position;
        bool ctiActive = false;

        [HideInInspector] public static bool clickThroughEnabled = false;

        void Start()
        {
#if !UNITY_EDITOR // You really don't want to enable this in the editor..
            hwnd = FindWindow(null, "TouchFree");
            SetWindowLong(hwnd, GWL_STYLE, WS_POPUP | WS_VISIBLE);
#endif

            clickThroughEnabled = false;
            CTIDeactivated();
        }

        public void CTIActivated()
        {
            ctiActive = true;
#if !UNITY_EDITOR
            if (!clickThroughEnabled)
            {
                return;
            }

            clickThroughEnabled = false;
            SetConfigWindow(true);
#endif
        }

        public void CTIDeactivated()
        {
            ctiActive = false;
#if !UNITY_EDITOR
            if (clickThroughEnabled || ctiActive)
            {
                return;
            }

            clickThroughEnabled = true;
            SetCursorWindow(true);
#endif
        }

        private void OnEnable()
        {
            CallToInteractController.OnCTIActive += CTIActivated;
            CallToInteractController.OnCTIInactive += CTIDeactivated;
        }

        private void OnDisable()
        {
            CallToInteractController.OnCTIActive -= CTIActivated;
            CallToInteractController.OnCTIInactive -= CTIDeactivated;
        }

        void SetCursorWindow(bool setResolution)
        {
            if (setResolution)
            {
                Screen.SetResolution(TouchFreeMain.CursorWindowSize, TouchFreeMain.CursorWindowSize, FullScreenMode.Windowed);
            }

            SetWindowLong(hwnd, -20, WS_EX_LAYERED | WS_EX_TRANSPARENT);

            SetWindowPos(hwnd, HWND_TOPMOST, Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y),
                TouchFreeMain.CursorWindowSize, TouchFreeMain.CursorWindowSize, SWP_FRAMECHANGED | SWP_SHOWWINDOW | SWP_NOACTIVATE);

            var margins = new MARGINS() { cxLeftWidth = -1 };
            DwmExtendFrameIntoClientArea(hwnd, ref margins);
        }

        void SetConfigWindow(bool setResolution)
        {
            if (setResolution)
            {
                Screen.SetResolution(Display.main.systemWidth, Display.main.systemHeight, FullScreenMode.FullScreenWindow);
            }

            // get the current -20 window and remove the layerd and transparent parts
            long style = GetWindowLong(hwnd, -20);
            style &= ~WS_EX_TRANSPARENT;
            style &= ~WS_EX_LAYERED;
            SetWindowLong(hwnd, -20, style);

            SetWindowPos(hwnd, -2, 0, 0, Display.main.systemWidth, Display.main.systemHeight, SWP_FRAMECHANGED | SWP_SHOWWINDOW | SWP_NOACTIVATE);

            var margins = new MARGINS();
            DwmExtendFrameIntoClientArea(hwnd, ref margins);
        }

        void Update()
        {
#if !UNITY_EDITOR
            if (clickThroughEnabled)
            {
                SetWindowPos(hwnd,
                    HWND_TOPMOST,
                    Mathf.RoundToInt(position.x),
                    Mathf.RoundToInt(position.y),
                    TouchFreeMain.CursorWindowSize,
                    TouchFreeMain.CursorWindowSize,
                    SWP_NOACTIVATE | SWP_FRAMECHANGED | SWP_SHOWWINDOW);

                long style = GetWindowLong(hwnd, -20);
                if((style & WS_EX_TRANSPARENT) != WS_EX_TRANSPARENT ||
                    (style & WS_EX_LAYERED) != WS_EX_LAYERED)
                {
                    SetWindowLong(hwnd, -20, WS_EX_LAYERED | WS_EX_TRANSPARENT);
                }

                style = GetWindowLong(hwnd, GWL_STYLE);
                if((style & WS_POPUP) != WS_POPUP ||
                    (style & WS_VISIBLE) != WS_VISIBLE)
                {
                    SetWindowLong(hwnd, GWL_STYLE, WS_POPUP | WS_VISIBLE);
                }
            }
#endif
        }

        public void SetPosition(Vector2 value)
        {
            position = value;

            position.x = position.x - (TouchFreeMain.CursorWindowSize / 2);
            position.y = Display.main.systemHeight - position.y - (TouchFreeMain.CursorWindowSize / 2);
        }

        private void OnApplicationQuit()
        {
            SetConfigWindow(true);
        }
    }
}
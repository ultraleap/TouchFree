using UnityEngine;
using System.Runtime.InteropServices;
using System;

namespace Ultraleap.ScreenControl.Core
{
    [DefaultExecutionOrder(-1000)]
    public class ScreenSizeInitializer : MonoBehaviour
    {
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool SetProcessDpiAwarenessContext(int dpiFlag);

        [DllImport("SHCore.dll", SetLastError = true)]
        internal static extern bool SetProcessDpiAwareness(PROCESS_DPI_AWARENESS awareness);

        [DllImport("user32.dll")]
        internal static extern bool SetProcessDPIAware();

        internal enum PROCESS_DPI_AWARENESS
        {
            Process_DPI_Unaware = 0,
            Process_System_DPI_Aware = 1,
            Process_Per_Monitor_DPI_Aware = 2
        }

        internal enum DPI_AWARENESS_CONTEXT
        {
            DPI_AWARENESS_CONTEXT_UNAWARE = 16,
            DPI_AWARENESS_CONTEXT_SYSTEM_AWARE = 17,
            DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE = 18,
            DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = 34
        }

        public void Awake()
        {
            if (ConfigManager.PhysicalConfig.ScreenWidthPX <= 1 ||
                ConfigManager.PhysicalConfig.ScreenHeightPX <= 1)
            {
                ConfigManager.PhysicalConfig.ScreenWidthPX = Display.main.systemWidth;
                ConfigManager.PhysicalConfig.ScreenHeightPX = Display.main.systemHeight;

                if(ConfigManager.PhysicalConfig.ScreenWidthPX <= 1 ||
                    ConfigManager.PhysicalConfig.ScreenHeightPX <= 1)
                {
                    ConfigManager.PhysicalConfig.ScreenWidthPX = 1;
                    ConfigManager.PhysicalConfig.ScreenHeightPX = 1;
                }

                ConfigManager.InteractionConfig.ConfigWasUpdated();
                ConfigManager.SaveAllConfigs();
            }

            HandleDPIAwareness();
        }

        void HandleDPIAwareness()
        {
            if (Environment.OSVersion.Version >= new Version(6, 3, 0)) // win 8.1 added support for per monitor dpi
            {
                if (Environment.OSVersion.Version >= new Version(10, 0, 15063)) // win 10 creators update added support for per monitor v2
                {
                    SetProcessDpiAwarenessContext((int)DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);
                }
                else
                {
                    SetProcessDpiAwareness(PROCESS_DPI_AWARENESS.Process_Per_Monitor_DPI_Aware);
                }
            }
            else
            {
                SetProcessDPIAware();
            }
        }
    }
}
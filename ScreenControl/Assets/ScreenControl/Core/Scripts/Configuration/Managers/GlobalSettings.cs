using System;

namespace Ultraleap.ScreenControl.Core
{
    public class GlobalSettings : BaseSettings
    {
        public int CursorWindowSize = 256;
        public int ScreenWidth;
        public int ScreenHeight;
        public VirtualScreen virtualScreen;

        // Store in M, display in CM
        public readonly float ConfigToDisplayMeasurementMultiplier = 100;

        public override void SetAllValuesToDefault()
        {
            var defaults = new GlobalSettings();

            CursorWindowSize = defaults.CursorWindowSize;
        }

        public void CreateVirtualScreen()
        {
            virtualScreen = new VirtualScreen(
                ScreenWidth,
                ScreenHeight,
                ConfigManager.PhysicalConfig.ScreenHeightM,
                ConfigManager.PhysicalConfig.ScreenRotationD);
        }
    }
}
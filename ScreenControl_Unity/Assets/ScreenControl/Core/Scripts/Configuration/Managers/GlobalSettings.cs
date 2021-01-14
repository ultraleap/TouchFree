using System;

namespace Ultraleap.ScreenControl.Core
{
    public class GlobalSettings : BaseSettings
    {
        public int ScreenWidth;
        public int ScreenHeight;
        public VirtualScreen virtualScreen;

        // Store in M, display in CM
        public readonly float ConfigToDisplayMeasurementMultiplier = 100;

        public override void SetAllValuesToDefault()
        {
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
namespace Ultraleap.ScreenControl.Core
{
    public class GlobalSettings : BaseSettings
    {
        public VirtualScreen virtualScreen;

        public override void SetAllValuesToDefault()
        {
        }

        public void CreateVirtualScreen()
        {
            virtualScreen = new VirtualScreen(
                ConfigManager.PhysicalConfig.ScreenWidthPX,
                ConfigManager.PhysicalConfig.ScreenHeightPX,
                ConfigManager.PhysicalConfig.ScreenHeightM,
                ConfigManager.PhysicalConfig.ScreenRotationD);
        }
    }
}
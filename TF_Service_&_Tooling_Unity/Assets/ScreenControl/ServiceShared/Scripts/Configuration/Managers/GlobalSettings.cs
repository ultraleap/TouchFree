namespace Ultraleap.ScreenControl.Core
{
    public class GlobalSettings : BaseSettings
    {
        public VirtualScreen virtualScreen
        { 
            get
            {
                if (_virtualScreen == null)
                {
                    CreateVirtualScreen();
                }

                return _virtualScreen;
            } 
        }
        VirtualScreen _virtualScreen;

        public override void SetAllValuesToDefault()
        {
        }

        public void CreateVirtualScreen()
        {
            _virtualScreen = new VirtualScreen(
                ConfigManager.PhysicalConfig.ScreenWidthPX,
                ConfigManager.PhysicalConfig.ScreenHeightPX,
                ConfigManager.PhysicalConfig.ScreenHeightM,
                ConfigManager.PhysicalConfig.ScreenRotationD);
        }
    }
}
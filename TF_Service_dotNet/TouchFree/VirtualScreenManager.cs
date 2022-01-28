using Ultraleap.TouchFree.Library.Configuration;

namespace Ultraleap.TouchFree.Library
{
    public class VirtualScreenManager : IVirtualScreenManager
    {
        private readonly IConfigManager configManager;

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
        private VirtualScreen _virtualScreen;

        public VirtualScreenManager(IConfigManager _configManager)
        {
            configManager = _configManager;
        }

        private void CreateVirtualScreen()
        {
            _virtualScreen = new VirtualScreen(
                configManager.PhysicalConfig.ScreenWidthPX,
                configManager.PhysicalConfig.ScreenHeightPX,
                configManager.PhysicalConfig.ScreenHeightM,
                configManager.PhysicalConfig.ScreenRotationD,
                configManager);
        }
    }
}

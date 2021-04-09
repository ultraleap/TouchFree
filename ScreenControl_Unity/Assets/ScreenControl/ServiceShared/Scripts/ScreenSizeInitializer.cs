using UnityEngine;

namespace Ultraleap.ScreenControl.Core
{
    [DefaultExecutionOrder(-1000)]
    public class ScreenSizeInitializer : MonoBehaviour
    {
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
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ultraleap.ScreenControl.Core
{
    public class QuickOrManualScreen : MonoBehaviour
    {
        public GameObject manualSetupScreen;
        public GameObject quickSetupScreen;

        public void ChangeToManualSetup()
        {
            // immediately use the selected mount type for manual setup
            bool wasBottomMounted = Mathf.Approximately(0, ConfigManager.PhysicalConfig.LeapRotationD.z);

            if (wasBottomMounted && (ScreenManager.Instance.selectedMountType == MountingType.ABOVE_FACING_SCREEN ||
                ScreenManager.Instance.selectedMountType == MountingType.ABOVE_FACING_USER))
            {
                ConfigManager.PhysicalConfig.LeapRotationD = new Vector3(
                    -ConfigManager.PhysicalConfig.LeapRotationD.x,
                    ConfigManager.PhysicalConfig.LeapRotationD.y,
                    180f);
            }
            else if (!wasBottomMounted && ScreenManager.Instance.selectedMountType == MountingType.BELOW)
            {
                ConfigManager.PhysicalConfig.LeapRotationD = new Vector3(
                    -ConfigManager.PhysicalConfig.LeapRotationD.x,
                    ConfigManager.PhysicalConfig.LeapRotationD.y, 0f);
            }

            ConfigManager.PhysicalConfig.ConfigWasUpdated();

            // dont allow manual to flip the axes again
            ScreenManager.Instance.selectedMountType = MountingType.NONE;
            ScreenManager.Instance.ChangeScreen(manualSetupScreen);
        }

        public void ChangeToQuickSetup()
        {
            ScreenManager.Instance.ChangeScreen(quickSetupScreen);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ultraleap.ScreenControl.Core
{
    public class QuickSetupCompleteScreen : MonoBehaviour
    {
        public GameObject quickSetupScreen;
        public GameObject manualSetupScreen;

        public void RetryQuickSetup()
        {
            ScreenManager.Instance.ChangeScreen(quickSetupScreen);
        }

        public void ChangeToManual()
        {
            ScreenManager.Instance.selectedMountType = MountingType.NONE;
            ScreenManager.Instance.ChangeScreen(manualSetupScreen);
        }
    }
}
using UnityEngine;

namespace Ultraleap.TouchFree.ServiceUI
{
    public class QuickSetupCompleteScreen : ConfigScreen
    {
        public GameObject quickSetupScreen;
        public GameObject manualSetupScreen;

        public void RetryQuickSetup()
        {
            ScreenManager.Instance.ChangeScreen(quickSetupScreen);
        }

        public void ChangeToManual()
        {
            ScreenManager.Instance.ChangeScreen(manualSetupScreen);
        }
    }
}
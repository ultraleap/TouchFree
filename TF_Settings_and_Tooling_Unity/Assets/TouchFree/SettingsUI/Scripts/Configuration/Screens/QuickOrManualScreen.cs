using UnityEngine;

namespace Ultraleap.TouchFree.ServiceUI
{
    public class QuickOrManualScreen : ConfigScreen
    {
        public GameObject manualSetupScreen;
        public GameObject quickSetupScreen;
        public GameObject cameraViewsScreen;

        public void ChangeToManualSetup()
        {
            ScreenManager.Instance.ChangeScreen(manualSetupScreen);
        }

        public void ChangeToQuickSetup()
        {
            ScreenManager.Instance.ChangeScreen(quickSetupScreen);
        }

        public void ChangeToCameraViews()
        {
            ScreenManager.Instance.ChangeScreen(cameraViewsScreen);
        }
    }
}
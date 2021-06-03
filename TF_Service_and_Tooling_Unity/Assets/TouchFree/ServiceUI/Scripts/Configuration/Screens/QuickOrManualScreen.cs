using UnityEngine;

namespace Ultraleap.TouchFree.ServiceUI
{
    public class QuickOrManualScreen : MonoBehaviour
    {
        public GameObject manualSetupScreen;
        public GameObject quickSetupScreen;

        public void ChangeToManualSetup()
        {
            ScreenManager.Instance.ChangeScreen(manualSetupScreen);
        }

        public void ChangeToQuickSetup()
        {
            ScreenManager.Instance.ChangeScreen(quickSetupScreen);
        }
    }
}
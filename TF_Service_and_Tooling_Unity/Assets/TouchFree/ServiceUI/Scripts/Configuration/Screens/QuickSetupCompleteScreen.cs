using UnityEngine;
using Ultraleap.TouchFree.ServiceShared;

namespace Ultraleap.TouchFree.ServiceUI
{
    public class QuickSetupCompleteScreen : MonoBehaviour
    {
        public GameObject quickSetupScreen;
        public GameObject manualSetupScreen;

        void OnEnable()
        {
           VirtualScreen.CaptureCurrentResolution();
        }

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
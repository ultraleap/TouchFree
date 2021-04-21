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
            ScreenManager.Instance.ChangeScreen(manualSetupScreen);
        }

        public void ChangeToQuickSetup()
        {
            ScreenManager.Instance.ChangeScreen(quickSetupScreen);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Ultraleap.ScreenControl.Core
{
    public class CalibrationTestScreen : MonoBehaviour
    {
        public Toggle[] toggles;

        private void OnEnable()
        {
            foreach (var tog in toggles)
            {
                tog.isOn = false;
            }
        }
    }
}
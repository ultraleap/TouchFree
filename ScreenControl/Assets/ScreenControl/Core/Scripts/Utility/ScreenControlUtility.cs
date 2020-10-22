using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ultraleap.ScreenControl.Core
{
    public static class ScreenControlUtility
    {
        public static float MapRangeToRange(float _value, float _oldMin, float _oldMax, float _newMin, float _newMax)
        {
            float oldRange = (_oldMax - _oldMin);
            float newValue;
            if (oldRange == 0)
            {
                newValue = _newMin;
            }
            else
            {
                float newRange = (_newMax - _newMin);
                newValue = (((_value - _oldMin) * newRange) / oldRange) + _newMin;
            }
            return newValue;
        }

        public static Color ParseColor(string _hexColor, float _alpha = 1)
        {
            Color defaultColor = Color.white;
            if (ColorUtility.TryParseHtmlString(_hexColor, out Color outColor))
            {
                outColor.a = _alpha;
                return outColor;
            }
            return defaultColor;
        }

        public static int ToDisplayUnits(int _value)
        {
            return (int)(_value * GlobalSettings.ConfigToDisplayMeasurementMultiplier);
        }

        public static float ToDisplayUnits(float _value)
        {
            return _value * GlobalSettings.ConfigToDisplayMeasurementMultiplier;
        }

        public static int FromDisplayUnits(int _value)
        {
            return (int)(_value / GlobalSettings.ConfigToDisplayMeasurementMultiplier);
        }

        public static float FromDisplayUnits(float _value)
        {
            return _value / GlobalSettings.ConfigToDisplayMeasurementMultiplier;
        }
    }
}
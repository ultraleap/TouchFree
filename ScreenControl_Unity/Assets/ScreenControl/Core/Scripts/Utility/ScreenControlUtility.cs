using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ultraleap.ScreenControl.Core
{
    public static class ScreenControlUtility
    {
        // Store in M, display in CM
        public static readonly float ConfigToDisplayMeasurementMultiplier = 100;

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
        public static int ToDisplayUnits(int _value)
        {
            return (int)(_value * ConfigToDisplayMeasurementMultiplier);
        }

        public static float ToDisplayUnits(float _value)
        {
            return _value * ConfigToDisplayMeasurementMultiplier;
        }

        public static int FromDisplayUnits(int _value)
        {
            return (int)(_value / ConfigToDisplayMeasurementMultiplier);
        }

        public static float FromDisplayUnits(float _value)
        {
            return _value / ConfigToDisplayMeasurementMultiplier;
        }
    }
}
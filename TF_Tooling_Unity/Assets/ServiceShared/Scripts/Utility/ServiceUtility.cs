using System.Globalization;
using System.Text.RegularExpressions;

namespace Ultraleap.TouchFree.ServiceShared
{
    public static class ServiceUtility
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

        public static float TryParseNewStringToFloat(float _original,
                                         string _newText,
                                         bool _convertToStorageUnits = false,
                                         bool _convertToDisplayUnits = false)
        {
            // ensure decimal commas are replaced with decimal points
            _newText = _newText.Replace(',', '.');

            // Match any character that is not period (.), hypen (-), or numbers 0 to 9, and strip them out.
            _newText = Regex.Replace(_newText, "[^.0-9-]", "");

            float val;

            if (!float.TryParse(_newText, NumberStyles.Number, CultureInfo.CurrentCulture, out val))
            {
                val = _original; // string was not compatible!
            }

            if (_convertToDisplayUnits)
            {
                val = ToDisplayUnits(val);
            }
            else if (_convertToStorageUnits)
            {
                val = FromDisplayUnits(val);
            }

            return val;
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

        /// <summary>
        ///    Ensure the calculated rotations make sense to the UI by avoiding large values.
        ///    Angles are centred around 0, with the smallest representation of the value
        /// </summary>
        public static float CentreRotationAroundZero(float angle)
        {
            angle = angle % 360;

            if (angle > 180)
            {
                return angle - 360;
            }
            else if (angle < -180)
            {
                return angle + 360;
            }
            else
            {
                return angle;
            }
        }
    }
}
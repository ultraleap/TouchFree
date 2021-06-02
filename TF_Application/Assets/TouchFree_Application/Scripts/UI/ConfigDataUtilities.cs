using System.Globalization;
using System.Text.RegularExpressions;

namespace Ultraleap.TouchFree
{
    public static class ConfigDataUtilities
    {
        public static readonly float ConfigToDisplayMeasurementMultiplier = 100;

        public static float TryParseNewStringToFloat(float _original,
                                                 string _newText,
                                                 bool _convertToStorageUnits = false,
                                                 bool _convertToDisplayUnits = false)
        {
            // Match any character that is not period (.), hypen (-), or numbers 0 to 9, and strip them out.
            _newText = Regex.Replace(_newText, "[^.0-9-]", "");

            float val;

            if (!float.TryParse(_newText, NumberStyles.Number, CultureInfo.CurrentCulture, out val))
                val = _original; // string was not compatible!

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
    }
}
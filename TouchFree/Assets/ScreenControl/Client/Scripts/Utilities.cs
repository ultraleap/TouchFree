namespace Ultraleap.ScreenControl.Client
{
    // Class: Utilities
    // This class contains any utility functions that ScreenControl uses.
    public static class Utilities
    {
        // Group: Functions

        // Function: MapRangeToRange
        // Map _value from a range of _oldMin to _oldMax to a new range of _newMin to _newMax.
        //
        // e.g. the result of MapRangeToRange(0.5f, 0f, 1f, 0f, 8f) is 4.
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
    }
}
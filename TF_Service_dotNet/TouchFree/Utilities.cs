namespace Ultraleap.TouchFree.Library
{
    public static class Utilities
    {
        internal static BitmaskFlags GetInteractionFlags(
            InteractionType _interactionType,
            HandType _handType,
            HandChirality _chirality,
            InputType _inputType)
        {
            BitmaskFlags returnVal = BitmaskFlags.NONE;

            switch (_handType)
            {
                case HandType.PRIMARY:
                    returnVal ^= BitmaskFlags.PRIMARY;
                    break;
                case HandType.SECONDARY:
                    returnVal ^= BitmaskFlags.SECONDARY;
                    break;
            }

            switch (_chirality)
            {
                case HandChirality.LEFT:
                    returnVal ^= BitmaskFlags.LEFT;
                    break;
                case HandChirality.RIGHT:
                    returnVal ^= BitmaskFlags.RIGHT;
                    break;
            }

            switch (_inputType)
            {
                case InputType.NONE:
                    returnVal ^= BitmaskFlags.NONE_INPUT;
                    break;
                case InputType.CANCEL:
                    returnVal ^= BitmaskFlags.CANCEL;
                    break;
                case InputType.MOVE:
                    returnVal ^= BitmaskFlags.MOVE;
                    break;
                case InputType.UP:
                    returnVal ^= BitmaskFlags.UP;
                    break;
                case InputType.DOWN:
                    returnVal ^= BitmaskFlags.DOWN;
                    break;
            }

            switch (_interactionType)
            {
                case InteractionType.PUSH:
                    returnVal ^= BitmaskFlags.PUSH;
                    break;
                case InteractionType.HOVER:
                    returnVal ^= BitmaskFlags.HOVER;
                    break;
                case InteractionType.GRAB:
                    returnVal ^= BitmaskFlags.GRAB;
                    break;
                case InteractionType.TOUCHPLANE:
                    returnVal ^= BitmaskFlags.TOUCHPLANE;
                    break;
            }

            return returnVal;
        }

        public static System.Numerics.Vector3 LeapVectorToNumerics(Leap.Vector _leap)
        {
            Leap.Vector scaledDown = _leap / 1000;
            return new System.Numerics.Vector3(scaledDown.x, scaledDown.y, -scaledDown.z);
        }

        public static float Lerp(float first, float second, float amount)
        {
            return (first * (1.0f - amount)) + (second * amount);
        }

        public static float InverseLerp(float first, float second, float value)
        {
            return (value - first) / (second - first);
        }

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
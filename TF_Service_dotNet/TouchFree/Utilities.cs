using System;

namespace Ultraleap.TouchFree.Library;

public static class Utilities
{
    public const float RADTODEG = (float)(180f / Math.PI);
    public const float DEGTORAD = (float)(Math.PI / 180f);

    public static float DegreesToRadians(float angle) => DEGTORAD * angle;

    internal static BitmaskFlags GetInteractionFlags(
        InteractionType interactionType,
        HandType handType,
        HandChirality chirality,
        InputType inputType) =>
        BitmaskFlags.NONE
        ^ handType switch
        {
            HandType.PRIMARY => BitmaskFlags.PRIMARY,
            HandType.SECONDARY => BitmaskFlags.SECONDARY,
        } ^ chirality switch
        {
            HandChirality.LEFT => BitmaskFlags.LEFT,
            HandChirality.RIGHT => BitmaskFlags.RIGHT,
        } ^ inputType switch
        {
            InputType.NONE => BitmaskFlags.NONE_INPUT,
            InputType.CANCEL => BitmaskFlags.CANCEL,
            InputType.MOVE => BitmaskFlags.MOVE,
            InputType.UP => BitmaskFlags.UP,
            InputType.DOWN => BitmaskFlags.DOWN,
        } ^ interactionType switch
        {
            InteractionType.PUSH => BitmaskFlags.PUSH,
            InteractionType.HOVER => BitmaskFlags.HOVER,
            InteractionType.GRAB => BitmaskFlags.GRAB,
            InteractionType.TOUCHPLANE => BitmaskFlags.TOUCHPLANE,
            InteractionType.VELOCITYSWIPE => BitmaskFlags.VELOCITYSWIPE,
            InteractionType.AIRCLICK => throw new NotImplementedException(),
        };

    public static System.Numerics.Vector3 LeapVectorToNumerics(Leap.Vector leap)
    {
        Leap.Vector scaledDown = leap / 1000;
        return new System.Numerics.Vector3(scaledDown.x, scaledDown.y, scaledDown.z);
    }

    public static float Lerp(float first, float second, float amount) => (first * (1.0f - amount)) + (second * amount);

    public static float InverseLerp(float first, float second, float value) => (value - first) / (second - first);

    // Function: MapRangeToRange
    // Map value from a range of oldMin to oldMax to a new range of newMin to newMax.
    //
    // e.g. the result of MapRangeToRange(0.5f, 0f, 1f, 0f, 8f) is 4.
    public static float MapRangeToRange(float value, float oldMin, float oldMax, float newMin, float newMax)
    {
        float oldRange = (oldMax - oldMin);
        float newValue;

        if (oldRange == 0)
        {
            newValue = newMin;
        }
        else
        {
            float newRange = (newMax - newMin);
            newValue = (((value - oldMin) * newRange) / oldRange) + newMin;
        }

        return newValue;
    }
}
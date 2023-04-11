using System;

namespace Ultraleap.TouchFree.Library.Interactions.GrabDetector;

public class GeneralisedGrabDetector
{
    public float GrabThreshold { get; set; } = 0.8f;
    public float UngrabThreshold { get; set; } = 0.7f;
    public bool Grabbing { get; private set; } = false;
    public float GeneralisedGrabStrength { get; private set; } = 0;

    public bool IsGrabbing(Leap.Hand hand)
    {
        ResolveClassicGrab(hand);
        return Grabbing;
    }

    private void ResolveClassicGrab(Leap.Hand hand)
    {
        float grabStrength = hand.GrabStrength;

        if (Grabbing)
        {
            bool classicGrabbing = (grabStrength >= UngrabThreshold);
            Grabbing = classicGrabbing;
        }
        else
        {
            bool classicGrabbing = (grabStrength >= GrabThreshold);
            Grabbing = classicGrabbing;
        }

        if (Grabbing)
        {
            GeneralisedGrabStrength = 1;
        }
        else
        {
            float normalisedGrabStrength = Utilities.MapRangeToRange(Math.Clamp(grabStrength, 0, GrabThreshold), 0, GrabThreshold, 0, 1);
            GeneralisedGrabStrength = normalisedGrabStrength;
        }
    }
}
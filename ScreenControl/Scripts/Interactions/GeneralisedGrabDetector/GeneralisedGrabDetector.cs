using System.Collections;
using System.ComponentModel;
using System.Collections.Generic;
using Leap.Unity;
using UnityEngine;

// Generalised-Grab Algorithms
public enum GrabAlgorithm
{
    [Description("Classic Pinch")]
    CLASSIC_PINCH,
    [Description("Classic Grab")]
    CLASSIC_GRAB,
    [Description("Classic Pinch or Grab")]
    CLASSIC_PINCH_OR_GRAB,
    [Description("Combined Pinch with Fist Strength")]
    COMBINED_PINCH_WITH_FIST,
    [Description("Safety Pinch")]
    SAFETY_PINCH,
    [Description("Thumbless Grab Algorithm")]
    THUMBLESS_GRAB,
    [Description("Thumbless Grab or Safety Pinch")]
    THUMBLESS_OR_SAFETY_PINCH,
    [Description("Duck Pinch")]
    DUCK_PINCH,
    [Description("Thumbless Grab or Duck Pinch")]
    THUMBLESS_OR_DUCK_PINCH,
    [Description("Thumbless Grab or Duck Pinch with PokePose Suppression")]
    THUMBLESS_OR_DUCK_PINCH_POKE_SUPPRESSED,
    [Description("Physics Grab")]
    PHYSICS_GRAB,
}

/**
 * The GeneralisedGrabDetector detects things that can be considered a 'grab'
 */
public class GeneralisedGrabDetector : MonoBehaviour
{
    [Header("General Params")]
    public GrabAlgorithm grabAlgorithm;
    [Range(0, 3)] public float minGrabDuration;

    [Header("Pinch Parameters")]
    [Range(0, 1)] public float pinchThreshold;
    [Range(0, 1)] public float unpinchThreshold;

    [Header("Grab Parameters")]
    [Range(0, 1)] public float grabThreshold;
    [Range(0, 1)] public float ungrabThreshold;

    [Header("Combined Parameters")]
    [Range(0, 2)] public float combinedGrabThreshold;
    [Range(0, 2)] public float combinedUngrabThreshold;

    [Header("Detectors")]
    public SafetyPinch safetyPinch;
    public ThumblessGrab thumblessGrab;
    public DuckPinch duckPinch;
    public PokePose pokePose;
    public PhysicsGrab physicsGrab;
    

    [Header("Debug Parameters")]
    public float pinchStrength;
    public float grabStrength;
    public float fistStrength;
    public float combinedStrength;
    public bool grabbing;
    public float GeneralisedGrabStrength = 0;


    private float grabStartTime;
    private bool requireUngrab = false;


    private void Start()
    {
        grabbing = false;
        requireUngrab = false;
    }

    // Check whether a Generalised-Grab has been detected
    //
    // The result depends on the GrabAlgorithm
    public bool IsGrabbing(Leap.Hand hand)
    {
        return IsGrabbing(0, hand, 0f);
    }

    public bool IsGrabbing(long timestamp, Leap.Hand hand, float cursorVelocity)
    {
        UpdateDebugParams(hand);
        UpdateGrab(timestamp, hand, cursorVelocity);

        if (grabbing && grabStartTime != -1.0f)
        {
            return (Time.time - grabStartTime) > minGrabDuration;
        }
        else
        {
            return false;
        }
    }

    // Update the data about the stored hand
    private void UpdateGrab(long timestamp, Leap.Hand hand, float cursorVelocity)
    {
        switch (grabAlgorithm)
        {
            case GrabAlgorithm.CLASSIC_PINCH:
                ResolveClassicPinch(hand);
                break;
            case GrabAlgorithm.CLASSIC_GRAB:
                ResolveClassicGrab(hand);
                break;
            case GrabAlgorithm.CLASSIC_PINCH_OR_GRAB:
                ResolveClassicPinchOrGrab(hand);
                break;
            case GrabAlgorithm.COMBINED_PINCH_WITH_FIST:
                ResolveCombinedPinchAndGrab(hand);
                break;
            case GrabAlgorithm.SAFETY_PINCH:
                ResolveSafetyPinch(hand);
                break;
            case GrabAlgorithm.THUMBLESS_GRAB:
                ResolveThumblessGrab(hand);
                break;
            case GrabAlgorithm.THUMBLESS_OR_SAFETY_PINCH:
                ResolveThumblessOrSafetyPinch(hand);
                break;
            case GrabAlgorithm.DUCK_PINCH:
                ResolveDuckPinch(hand);
                break;
            case GrabAlgorithm.THUMBLESS_OR_DUCK_PINCH:
                ResolveThumblessOrDuckPinch(hand);
                break;
            case GrabAlgorithm.THUMBLESS_OR_DUCK_PINCH_POKE_SUPPRESSED:
                ResolveThumblessOrDuckWithPokeSuppression(hand);
                break;
            case GrabAlgorithm.PHYSICS_GRAB:
                ResolvePhysicsGrab(timestamp, hand, cursorVelocity);
                break;
        }

        if (!grabbing)
        {
            grabStartTime = -1.0f;
        }
        if (grabbing && grabStartTime == -1.0f)
        {
            grabStartTime = Time.time;
        }
    }

    private void ResolveClassicPinch(Leap.Hand hand)
    {
        float pinchStrength = hand.PinchStrength;
        if (grabbing)
        {
            grabbing = (pinchStrength >= unpinchThreshold);
            GeneralisedGrabStrength = ScreenControlUtility.MapRangeToRange(Mathf.Clamp(pinchStrength, 0, unpinchThreshold), 0, unpinchThreshold, 0, 1);
        }
        else
        {
            grabbing = (pinchStrength >= pinchThreshold);
            GeneralisedGrabStrength = ScreenControlUtility.MapRangeToRange(Mathf.Clamp(pinchStrength, 0, pinchThreshold), 0, pinchThreshold, 0, 1);
        }
    }

    private void ResolveClassicGrab(Leap.Hand hand)
    {
        float grabStrength = hand.GrabStrength;
        if (grabbing)
        {
            grabbing = (grabStrength >= ungrabThreshold);
            GeneralisedGrabStrength = ScreenControlUtility.MapRangeToRange(Mathf.Clamp(grabStrength, 0, ungrabThreshold), 0, ungrabThreshold, 0, 1);
        }
        else
        {
            grabbing = (grabStrength >= grabThreshold);
            GeneralisedGrabStrength = ScreenControlUtility.MapRangeToRange(Mathf.Clamp(grabStrength, 0, grabThreshold), 0, grabThreshold, 0, 1);
        }
    }

    private void ResolveClassicPinchOrGrab(Leap.Hand hand)
    {
        float pinchStrength = hand.PinchStrength;
        float grabStrength = hand.GrabStrength;

        if (grabbing)
        {
            bool classicPinching = (pinchStrength >= unpinchThreshold);
            bool classicGrabbing = (grabStrength >= ungrabThreshold);
            grabbing = classicPinching | classicGrabbing;
        }
        else
        {
            bool classicPinching = (pinchStrength >= pinchThreshold);
            bool classicGrabbing = (grabStrength >= grabThreshold);
            grabbing = classicPinching | classicGrabbing;
        }

        if (grabbing)
        {
            GeneralisedGrabStrength = 1;
        }
        else
        {
            float normalisedPinchStrength = ScreenControlUtility.MapRangeToRange(Mathf.Clamp(pinchStrength, 0, pinchThreshold), 0, pinchThreshold, 0, 1);
            float normalisedGrabStrength = ScreenControlUtility.MapRangeToRange(Mathf.Clamp(grabStrength, 0, grabThreshold), 0, grabThreshold, 0, 1);

            GeneralisedGrabStrength = Mathf.Max(normalisedPinchStrength, normalisedGrabStrength);
        }
    }
    private void ResolveCombinedPinchAndGrab(Leap.Hand hand)
    {
        float pinchStrength = hand.PinchStrength;
        float fistStrength = hand.GetFistStrength();
        float combinedStrength = pinchStrength + fistStrength;

        if (grabbing)
        {
            grabbing = combinedStrength >= combinedUngrabThreshold;
            GeneralisedGrabStrength = ScreenControlUtility.MapRangeToRange(Mathf.Clamp(combinedStrength, 0, combinedUngrabThreshold), 0, combinedUngrabThreshold, 0, 1);
        }
        else
        {
            grabbing = (pinchStrength == pinchThreshold) && (combinedStrength >= combinedGrabThreshold);
            float clampedCombinedStrength = Mathf.Clamp(combinedStrength, 0, combinedGrabThreshold);

            GeneralisedGrabStrength = ScreenControlUtility.MapRangeToRange(Mathf.Clamp(clampedCombinedStrength + pinchStrength, 0, combinedGrabThreshold + pinchThreshold), 0, clampedCombinedStrength + pinchThreshold, 0, 1);
        }

    }

    private void ResolveSafetyPinch(Leap.Hand hand)
    {
        grabbing = safetyPinch.IsPinching(hand);
        GeneralisedGrabStrength = safetyPinch.PinchStrength;
    }

    private void ResolveThumblessGrab(Leap.Hand hand)
    {
        grabbing = thumblessGrab.IsGrabbing(hand);
        GeneralisedGrabStrength = thumblessGrab.GrabStrength;
    }

    private void ResolveThumblessOrSafetyPinch(Leap.Hand hand)
    {
        bool thumblessGrabbing = thumblessGrab.IsGrabbing(hand);
        bool safetyPinching = safetyPinch.IsPinching(hand);

        grabbing = thumblessGrabbing | safetyPinching;
        GeneralisedGrabStrength = Mathf.Max(thumblessGrab.GrabStrength, safetyPinch.PinchStrength);
    }

    private void ResolveDuckPinch(Leap.Hand hand)
    {
        grabbing = duckPinch.IsGrabbing(hand);
        GeneralisedGrabStrength = duckPinch.DuckPinchStrength;
    }

    private void ResolveThumblessOrDuckPinch(Leap.Hand hand)
    {
        bool thumblessGrabbing = thumblessGrab.IsGrabbing(hand);
        bool duckPinching = duckPinch.IsGrabbing(hand);

        grabbing = thumblessGrabbing | duckPinching;
        GeneralisedGrabStrength = Mathf.Max(thumblessGrab.GrabStrength, duckPinch.DuckPinchStrength);
    }

    private void ResolveThumblessOrDuckWithPokeSuppression(Leap.Hand hand)
    {
        bool thumblessGrabbing = thumblessGrab.IsGrabbing(hand);
        bool duckPinching = duckPinch.IsGrabbing(hand);

        if (pokePose.InPose(hand))
        {
            grabbing = false;
            requireUngrab = true;
            GeneralisedGrabStrength = 0f;
        }
        else if (thumblessGrabbing | duckPinching)
        {
            if (requireUngrab)
            {
                grabbing = false;
                GeneralisedGrabStrength = 0f;
            }
            else
            {
                grabbing = true;
                GeneralisedGrabStrength = Mathf.Max(thumblessGrab.GrabStrength, duckPinch.DuckPinchStrength);
            }
        }
        else
        {
            grabbing = false;
            requireUngrab = false;
            GeneralisedGrabStrength = Mathf.Max(thumblessGrab.GrabStrength, duckPinch.DuckPinchStrength);
        }
    }

    private void ResolvePhysicsGrab(long timestamp, Leap.Hand hand, float cursorVelocity)
    {
        physicsGrab.UpdateData(timestamp, hand, cursorVelocity);
        grabbing = physicsGrab.Grabbing;
        GeneralisedGrabStrength = physicsGrab.GrabStrength;
    }

    private void UpdateDebugParams(Leap.Hand hand)
    {
        pinchStrength = hand.PinchStrength;
        grabStrength = hand.GrabStrength;
        fistStrength = hand.GetFistStrength();
        combinedStrength = pinchStrength + fistStrength;
    }
}

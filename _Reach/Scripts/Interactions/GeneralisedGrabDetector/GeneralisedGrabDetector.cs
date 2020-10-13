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
    public bool IsGrabbing()
    {
        UpdateDebugParams();
        UpdateGrab();

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
    private void UpdateGrab()
    {
        switch (grabAlgorithm)
        {
            case GrabAlgorithm.CLASSIC_PINCH:
                ResolveClassicPinch();
                break;
            case GrabAlgorithm.CLASSIC_GRAB:
                ResolveClassicGrab();
                break;
            case GrabAlgorithm.CLASSIC_PINCH_OR_GRAB:
                ResolveClassicPinchOrGrab();
                break;
            case GrabAlgorithm.COMBINED_PINCH_WITH_FIST:
                ResolveCombinedPinchAndGrab();
                break;
            case GrabAlgorithm.SAFETY_PINCH:
                ResolveSafetyPinch();
                break;
            case GrabAlgorithm.THUMBLESS_GRAB:
                ResolveThumblessGrab();
                break;
            case GrabAlgorithm.THUMBLESS_OR_SAFETY_PINCH:
                ResolveThumblessOrSafetyPinch();
                break;
            case GrabAlgorithm.DUCK_PINCH:
                ResolveDuckPinch();
                break;
            case GrabAlgorithm.THUMBLESS_OR_DUCK_PINCH:
                ResolveThumblessOrDuckPinch();
                break;
            case GrabAlgorithm.THUMBLESS_OR_DUCK_PINCH_POKE_SUPPRESSED:
                ResolveThumblessOrDuckWithPokeSuppression();
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

    private void ResolveClassicPinch()
    {
        float pinchStrength = SingleHandManager.Instance.CurrentHand.PinchStrength;
        if (grabbing)
        {
            grabbing = (pinchStrength >= unpinchThreshold);
            GeneralisedGrabStrength = ReachUtility.MapRangeToRange(Mathf.Clamp(pinchStrength, 0, unpinchThreshold), 0, unpinchThreshold, 0, 1);
        }
        else
        {
            grabbing = (pinchStrength >= pinchThreshold);
            GeneralisedGrabStrength = ReachUtility.MapRangeToRange(Mathf.Clamp(pinchStrength, 0, pinchThreshold), 0, pinchThreshold, 0, 1);
        }
    }

    private void ResolveClassicGrab()
    {
        float grabStrength = SingleHandManager.Instance.CurrentHand.GrabStrength;
        if (grabbing)
        {
            grabbing = (grabStrength >= ungrabThreshold);
            GeneralisedGrabStrength = ReachUtility.MapRangeToRange(Mathf.Clamp(grabStrength, 0, ungrabThreshold), 0, ungrabThreshold, 0, 1);
        }
        else
        {
            grabbing = (grabStrength >= grabThreshold);
            GeneralisedGrabStrength = ReachUtility.MapRangeToRange(Mathf.Clamp(grabStrength, 0, grabThreshold), 0, grabThreshold, 0, 1);
        }
    }

    private void ResolveClassicPinchOrGrab()
    {
        float pinchStrength = SingleHandManager.Instance.CurrentHand.PinchStrength;
        float grabStrength = SingleHandManager.Instance.CurrentHand.GrabStrength;

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
            float normalisedPinchStrength = ReachUtility.MapRangeToRange(Mathf.Clamp(pinchStrength, 0, pinchThreshold), 0, pinchThreshold, 0, 1);
            float normalisedGrabStrength = ReachUtility.MapRangeToRange(Mathf.Clamp(grabStrength, 0, grabThreshold), 0, grabThreshold, 0, 1);

            GeneralisedGrabStrength = Mathf.Max(normalisedPinchStrength, normalisedGrabStrength);
        }
    }
    private void ResolveCombinedPinchAndGrab()
    {
        float pinchStrength = SingleHandManager.Instance.CurrentHand.PinchStrength;
        float fistStrength = SingleHandManager.Instance.CurrentHand.GetFistStrength();
        float combinedStrength = pinchStrength + fistStrength;

        if (grabbing)
        {
            grabbing = combinedStrength >= combinedUngrabThreshold;
            GeneralisedGrabStrength = ReachUtility.MapRangeToRange(Mathf.Clamp(combinedStrength, 0, combinedUngrabThreshold), 0, combinedUngrabThreshold, 0, 1);
        }
        else
        {
            grabbing = (pinchStrength == pinchThreshold) && (combinedStrength >= combinedGrabThreshold);
            float clampedCombinedStrength = Mathf.Clamp(combinedStrength, 0, combinedGrabThreshold);

            GeneralisedGrabStrength = ReachUtility.MapRangeToRange(Mathf.Clamp(clampedCombinedStrength + pinchStrength, 0, combinedGrabThreshold + pinchThreshold), 0, clampedCombinedStrength + pinchThreshold, 0, 1);
        }

    }

    private void ResolveSafetyPinch()
    {
        grabbing = safetyPinch.IsPinching(SingleHandManager.Instance.CurrentHand);
        GeneralisedGrabStrength = safetyPinch.PinchStrength;
    }

    private void ResolveThumblessGrab()
    {
        grabbing = thumblessGrab.IsGrabbing(SingleHandManager.Instance.CurrentHand);
        GeneralisedGrabStrength = thumblessGrab.GrabStrength;
    }

    private void ResolveThumblessOrSafetyPinch()
    {
        bool thumblessGrabbing = thumblessGrab.IsGrabbing(SingleHandManager.Instance.CurrentHand);
        bool safetyPinching = safetyPinch.IsPinching(SingleHandManager.Instance.CurrentHand);

        grabbing = thumblessGrabbing | safetyPinching;
        GeneralisedGrabStrength = Mathf.Max(thumblessGrab.GrabStrength, safetyPinch.PinchStrength);
    }

    private void ResolveDuckPinch()
    {
        grabbing = duckPinch.IsGrabbing(SingleHandManager.Instance.CurrentHand);
        GeneralisedGrabStrength = duckPinch.DuckPinchStrength;
    }

    private void ResolveThumblessOrDuckPinch()
    {
        bool thumblessGrabbing = thumblessGrab.IsGrabbing(SingleHandManager.Instance.CurrentHand);
        bool duckPinching = duckPinch.IsGrabbing(SingleHandManager.Instance.CurrentHand);

        grabbing = thumblessGrabbing | duckPinching;
        GeneralisedGrabStrength = Mathf.Max(thumblessGrab.GrabStrength, duckPinch.DuckPinchStrength);
    }

    private void ResolveThumblessOrDuckWithPokeSuppression()
    {
        bool thumblessGrabbing = thumblessGrab.IsGrabbing(SingleHandManager.Instance.CurrentHand);
        bool duckPinching = duckPinch.IsGrabbing(SingleHandManager.Instance.CurrentHand);

        if (pokePose.InPose(SingleHandManager.Instance.CurrentHand))
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

    private void UpdateDebugParams()
    {
        pinchStrength = SingleHandManager.Instance.CurrentHand.PinchStrength;
        grabStrength = SingleHandManager.Instance.CurrentHand.GrabStrength;
        fistStrength = SingleHandManager.Instance.CurrentHand.GetFistStrength();
        combinedStrength = pinchStrength + fistStrength;
    }
}

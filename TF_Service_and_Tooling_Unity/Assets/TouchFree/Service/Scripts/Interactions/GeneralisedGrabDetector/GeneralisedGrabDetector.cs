using System.Collections;
using System.ComponentModel;
using System.Collections.Generic;
using Leap.Unity;
using UnityEngine;
using Ultraleap.TouchFree.ServiceShared;

namespace Ultraleap.TouchFree.Service
{
    public class GeneralisedGrabDetector : MonoBehaviour
    {
        [Header("Pinch Parameters")]
        [Range(0, 1)] public float pinchThreshold;
        [Range(0, 1)] public float unpinchThreshold;

        [Header("Grab Parameters")]
        [Range(0, 1)] public float grabThreshold;
        [Range(0, 1)] public float ungrabThreshold;

        [Header("Debug Parameters")]
        public float pinchStrength;
        public float grabStrength;
        public bool grabbing;
        public float GeneralisedGrabStrength = 0;

        private void Start()
        {
            grabbing = false;
        }

        public bool IsGrabbing(Leap.Hand hand)
        {
            ResolveClassicPinchOrGrab(hand);

            if (grabbing)
            {
                return true;
            }
            else
            {
                return false;
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
                float normalisedPinchStrength = ServiceUtility.MapRangeToRange(Mathf.Clamp(pinchStrength, 0, pinchThreshold), 0, pinchThreshold, 0, 1);
                float normalisedGrabStrength = ServiceUtility.MapRangeToRange(Mathf.Clamp(grabStrength, 0, grabThreshold), 0, grabThreshold, 0, 1);

                GeneralisedGrabStrength = Mathf.Max(normalisedPinchStrength, normalisedGrabStrength);
            }
        }
    }
}
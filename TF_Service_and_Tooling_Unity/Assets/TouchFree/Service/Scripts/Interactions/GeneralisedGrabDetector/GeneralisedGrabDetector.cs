using UnityEngine;
using Ultraleap.TouchFree.ServiceShared;

namespace Ultraleap.TouchFree.Service
{
    public class GeneralisedGrabDetector : MonoBehaviour
    {
        [Header("Grab Parameters")]
        [Range(0, 1)] public float grabThreshold;
        [Range(0, 1)] public float ungrabThreshold;

        [Header("Debug Parameters")]
        public float grabStrength;
        public bool grabbing;
        public float GeneralisedGrabStrength = 0;

        private void Start()
        {
            grabbing = false;
        }

        public bool IsGrabbing(Leap.Hand hand)
        {
            ResolveClassicGrab(hand);
            return grabbing;
        }

        private void ResolveClassicGrab(Leap.Hand hand)
        {
            float grabStrength = hand.GrabStrength;

            if (grabbing)
            {
                bool classicGrabbing = (grabStrength >= ungrabThreshold);
                grabbing = classicGrabbing;
            }
            else
            {
                bool classicGrabbing = (grabStrength >= grabThreshold);
                grabbing = classicGrabbing;
            }

            if (grabbing)
            {
                GeneralisedGrabStrength = 1;
            }
            else
            {
                float normalisedGrabStrength = ServiceUtility.MapRangeToRange(Mathf.Clamp(grabStrength, 0, grabThreshold), 0, grabThreshold, 0, 1);
                GeneralisedGrabStrength = normalisedGrabStrength;
            }
        }
    }
}
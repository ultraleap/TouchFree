﻿using System;

using Ultraleap.TouchFree.Library;

namespace Ultraleap.TouchFree.Service
{
    public class GeneralisedGrabDetector
    {
        public float grabThreshold = 0.8f;
        public float ungrabThreshold = 0.7f;

        public bool grabbing;
        public float grabStrength;
        public float GeneralisedGrabStrength = 0;

        public GeneralisedGrabDetector()
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
                float normalisedGrabStrength = Utilities.MapRangeToRange(Utilities.Clamp(grabStrength, 0, grabThreshold), 0, grabThreshold, 0, 1);
                GeneralisedGrabStrength = normalisedGrabStrength;
            }
        }
    }
}
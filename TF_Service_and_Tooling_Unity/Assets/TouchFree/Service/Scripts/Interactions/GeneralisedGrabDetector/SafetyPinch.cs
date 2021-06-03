using System.Collections;
using System.Collections.Generic;
using Leap.Unity;
using UnityEngine;

namespace Ultraleap.TouchFree.Service
{
    public class SafetyPinch : MonoBehaviour
    {
        [Header("Activation Parameters")]
        [Range(0f, 0.04f)]
        public float pinchActivateDistance = 0.0075f;

        [Header("Deactivation Parameters")]
        [Range(0f, 0.04f)]
        public float pinchDeactivateDistance = 0.025f;
        [Range(0f, 0.04f)]
        public float failedPinchResetDistance = 0.010f;

        [Header("Safety-Pinch Parameters")]
        [Tooltip("This is the 'safety pinch' requirement, which only recognizes a pinch if "
               + "the middle and ring fingers are open.")]
        public bool requireMiddleAndRingSafetyPinch = false;
        [Range(0f, 90f)]
        public float minPalmMiddleAngle = 65f;
        [Range(0f, 90f)]
        public float minPalmRingAngle = 65f;

        [Header("Hysteresis Parameters")]
        [Range(0.6f, 1f)]
        public float ringMiddleSafetyHysteresisMult = 0.8f;

        [Header("Eligibility Params")]
        [Range(45f, 130f)]
        public float maxIndexAngleForEligibilityActivation = 98f;
        [Range(45f, 130f)]
        public float maxIndexAngleForEligibilityDeactivation = 110f;
        [Range(45f, 130f)]
        public float maxThumbAngleForEligibilityActivation = 85f;
        [Range(45f, 130f)]
        public float maxThumbAngleForEligibilityDeactivation = 100f;

        private const int MIN_REACTIVATE_TIME = 5;  // Number of frames
        private int minReactivateTimer = 0;

        private const int MIN_DEACTIVATE_TIME = 5;  // Number of frames
        private int minDeactivateTimer = 0;

        private bool grabbing;
        private bool requiresRepinch;

        private float _latestPinchStrength;

        private bool _isGestureEligible;

        public float PinchStrength
        {
            get
            {
                return grabbing ? 1f : 0f;
            }
        }

        private void Start()
        {
            grabbing = false;
            requiresRepinch = false;
            _isGestureEligible = false;
        }

        public bool IsPinching(Leap.Hand hand)
        {
            if (grabbing)
            {
                grabbing = !ShouldGestureDeactivate(hand);
            }
            else
            {
                grabbing = ShouldGestureActivate(hand);
            }
            return grabbing;
        }

        bool ShouldGestureActivate(Leap.Hand hand)
        {
            bool shouldActivate = false;

            _latestPinchStrength = 0f;

            bool wasEligibleLastCheck = _isGestureEligible;
            _isGestureEligible = false;

            // Can only activate a pinch if we haven't already activated a pinch very recently.
            if (minReactivateTimer > MIN_REACTIVATE_TIME)
            {
                shouldActivate = CheckHandForActivation(hand, wasEligibleLastCheck);
            }
            else
            {
                minReactivateTimer += 1;
            }

            if (shouldActivate)
            {
                minDeactivateTimer = 0;
            }
            return shouldActivate;
        }

        bool CheckHandForActivation(Leap.Hand hand, bool wasEligibleLastCheck)
        {
            bool shouldActivate = false;
            float latestPinchDistance = GetCustomPinchDistance(hand);

            Vector3 palmDir = hand.PalmarAxis();

            Vector3 middleDir = hand.GetMiddle().bones[1].Direction.ToVector3();
            float signedMiddlePalmAngle = Vector3.SignedAngle(palmDir, middleDir, hand.RadialAxis());
            if (hand.IsLeft)
            { signedMiddlePalmAngle *= -1f; }

            Vector3 ringDir = hand.GetRing().bones[1].Direction.ToVector3();
            float signedRingPalmAngle = Vector3.SignedAngle(palmDir, ringDir, hand.RadialAxis());
            if (hand.IsLeft)
            { signedRingPalmAngle *= -1f; }

            Vector3 indexDir = hand.GetIndex().bones[1].Direction.ToVector3();
            float indexPalmAngle = Vector3.Angle(indexDir, palmDir);

            Vector3 thumbDir = hand.GetThumb().bones[2].Direction.ToVector3();
            float thumbPalmAngle = Vector3.Angle(thumbDir, palmDir);

            // Eligibility checks-- necessary, but not sufficient conditions to start
            // a pinch, suitable for e.g. visual feedback on whether the gesture is
            // "able to occur" or "about to occur."
            if (

                    ((!wasEligibleLastCheck
                        && signedMiddlePalmAngle >= minPalmMiddleAngle)
                    || (wasEligibleLastCheck
                        && signedMiddlePalmAngle >= minPalmMiddleAngle
                                                    * ringMiddleSafetyHysteresisMult)
                    || !requireMiddleAndRingSafetyPinch)

                && ((!wasEligibleLastCheck
                        && signedRingPalmAngle >= minPalmRingAngle)
                    || (wasEligibleLastCheck
                        && signedRingPalmAngle >= minPalmRingAngle
                                                    * ringMiddleSafetyHysteresisMult)
                    || !requireMiddleAndRingSafetyPinch)

                // Index angle (eligibility state only)
                && ((!wasEligibleLastCheck
                        && indexPalmAngle < maxIndexAngleForEligibilityActivation)
                    || (wasEligibleLastCheck
                        && indexPalmAngle < maxIndexAngleForEligibilityDeactivation))

                // Thumb angle (eligibility state only)
                && ((!wasEligibleLastCheck
                        && thumbPalmAngle < maxThumbAngleForEligibilityActivation)
                    || (wasEligibleLastCheck
                        && thumbPalmAngle < maxThumbAngleForEligibilityDeactivation))

                // Must cross pinch threshold from a non-pinching / non-fist pose.
                && (!requiresRepinch)

                )
            {

                // Conceptually, this should be true when all but the most essential
                // parameters for the gesture are satisfied, so the user can be notified
                // that the gesture is imminent.
                _isGestureEligible = true;
            }

            #region Update Pinch Strength

            // Update global "pinch strength".
            // If the gesture is eligible, we'll have a non-zero pinch strength.
            if (_isGestureEligible)
            {
                _latestPinchStrength = latestPinchDistance.Map(0f, pinchActivateDistance,
                                                                1f, 0f);
            }
            else
            {
                _latestPinchStrength = 0f;
            }

            #endregion

            #region Check: Pinch Distance

            if (_isGestureEligible

                // Absolute pinch strength.
                && (latestPinchDistance < pinchActivateDistance)

                    )
            {
                shouldActivate = true;
            }

            #endregion

            #region Hysteresis for Failed Pinches

            // "requiresRepinch" prevents a closed-finger configuration from beginning
            // a pinch when the index and thumb never actually actively close from a
            // valid position -- think, closed-fist to safety-pinch, as opposed to
            // open-hand to safety-pinch -- without introducing any velocity-based
            // requirement.
            if (latestPinchDistance < pinchActivateDistance && !shouldActivate)
            {
                requiresRepinch = true;
            }
            if (requiresRepinch && latestPinchDistance > failedPinchResetDistance)
            {
                requiresRepinch = false;
            }

            #endregion

            return shouldActivate;
        }

        bool ShouldGestureDeactivate(Leap.Hand hand)
        {
            bool shouldDeactivate = false;
            _latestPinchStrength = 1f;

            if (minDeactivateTimer > MIN_DEACTIVATE_TIME)
            {
                var pinchDistance = GetCustomPinchDistance(hand);

                if (pinchDistance > pinchDeactivateDistance)
                {
                    shouldDeactivate = true;

                }
            }
            else
            {
                minDeactivateTimer += 1;
            }
            if (shouldDeactivate)
            {
                minReactivateTimer = 0;
            }
            return shouldDeactivate;
        }


        float GetCustomPinchDistance(Leap.Hand hand)
        {
            float pinchDistance = PinchSegmentToSegmentDisplacement(hand).magnitude;
            pinchDistance -= 0.01f;
            pinchDistance = Mathf.Max(0f, pinchDistance);
            return pinchDistance;
        }

        static Vector3 PinchSegmentToSegmentDisplacement(Leap.Hand hand)
        {
            Vector3 indexDistal = hand.GetIndex().bones[3].PrevJoint.ToVector3();
            Vector3 indexTip = hand.GetIndex().TipPosition.ToVector3();
            Vector3 thumbDistal = hand.GetThumb().bones[3].PrevJoint.ToVector3();
            Vector3 thumbTip = hand.GetThumb().TipPosition.ToVector3();
            return SegmentToSegmentDisplacement(indexDistal, indexTip, thumbDistal, thumbTip);
        }

        static Vector3 SegmentToSegmentDisplacement(Vector3 a1, Vector3 a2, Vector3 b1, Vector3 b2)
        {
            // SegmentToSegmentDisplacement

            Vector3 u = a2 - a1; //from a1 to a2
            Vector3 v = b2 - b1; //from b1 to b2
            Vector3 w = a1 - b1;
            float a = Vector3.Dot(u, u);         // always >= 0
            float b = Vector3.Dot(u, v);
            float c = Vector3.Dot(v, v);         // always >= 0
            float d = Vector3.Dot(u, w);
            float e = Vector3.Dot(v, w);
            float D = a * c - b * b;        // always >= 0
            float sc, sN, sD = D;       // sc = sN / sD, default sD = D >= 0
            float tc, tN, tD = D;       // tc = tN / tD, default tD = D >= 0

            // compute the line parameters of the two closest points
            if (D < Mathf.Epsilon)
            { // the lines are almost parallel
                sN = 0.0f;         // force using point P0 on segment S1
                sD = 1.0f;         // to prevent possible division by 0.0 later
                tN = e;
                tD = c;
            }
            else
            {                 // get the closest points on the infinite lines
                sN = (b * e - c * d);
                tN = (a * e - b * d);
                if (sN < 0.0f)
                {        // sc < 0 => the s=0 edge is visible
                    sN = 0.0f;
                    tN = e;
                    tD = c;
                }
                else if (sN > sD)
                {  // sc > 1  => the s=1 edge is visible
                    sN = sD;
                    tN = e + b;
                    tD = c;
                }
            }

            if (tN < 0.0)
            {            // tc < 0 => the t=0 edge is visible
                tN = 0.0f;
                // recompute sc for this edge
                if (-d < 0.0)
                    sN = 0.0f;
                else if (-d > a)
                    sN = sD;
                else
                {
                    sN = -d;
                    sD = a;
                }
            }
            else if (tN > tD)
            {      // tc > 1  => the t=1 edge is visible
                tN = tD;
                // recompute sc for this edge
                if ((-d + b) < 0.0)
                    sN = 0;
                else if ((-d + b) > a)
                    sN = sD;
                else
                {
                    sN = (-d + b);
                    sD = a;
                }
            }
            // finally do the division to get sc and tc
            sc = (Mathf.Abs(sN) < Mathf.Epsilon ? 0.0f : sN / sD);
            tc = (Mathf.Abs(tN) < Mathf.Epsilon ? 0.0f : tN / tD);

            // get the difference of the two closest points
            Vector3 dP = w + (sc * u) - (tc * v);  // =  S1(sc) - S2(tc)
            return dP;   // return the closest distance
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;

namespace Ultraleap.ScreenControl.Core
{
    public class ThumblessGrab : MonoBehaviour
    {
        [Header("Strength Params")]
        public float clickAngle = 70.0f;
        public float unclickAngle = 40.0f;

        public float GrabStrength { get; private set; }

        [Header("Private Params")]
        private bool grabbing;

        [Header("Debug Params")]
        public float angle;

        void Start()
        {
            grabbing = false;
        }

        public bool IsGrabbing(Leap.Hand hand)
        {
            if (grabbing)
            {
                grabbing = !ShouldTriggerUngrab(hand);
            }
            else
            {
                grabbing = ShouldTriggerGrab(hand);
            }
            return grabbing;
        }

        private float GetAngle(Leap.Hand hand)
        {
            Vector3 proximalAxis = hand.DistalAxis() * -1f;
            Vector3 radialAxis = hand.RadialAxis();

            if (hand.IsLeft)
            {
                radialAxis *= -1f;
            }

            List<float> fingerAngles = new List<float>
        {
            Vector3.SignedAngle(proximalAxis, hand.GetIndex().Direction.ToVector3(), radialAxis),
            Vector3.SignedAngle(proximalAxis, hand.GetMiddle().Direction.ToVector3(), radialAxis),
            Vector3.SignedAngle(proximalAxis, hand.GetRing().Direction.ToVector3(), radialAxis),
            Vector3.SignedAngle(proximalAxis, hand.GetPinky().Direction.ToVector3(), radialAxis)
        };

            List<float> fingerAnglesShifted = new List<float>();

            foreach (float angle in fingerAngles)
            {
                float shiftedAngle = angle;
                if (angle < -90f)
                {
                    shiftedAngle += 360f;
                }
                fingerAnglesShifted.Add(shiftedAngle);
            }


            angle = 0.25f * (fingerAnglesShifted[0] + fingerAnglesShifted[1] + fingerAnglesShifted[2] + fingerAnglesShifted[3]);

            return angle;
        }

        private bool ShouldTriggerGrab(Leap.Hand hand)
        {
            float handAngle = GetAngle(hand);
            float clampedHandAngle = Mathf.Clamp(handAngle, clickAngle, 180);
            GrabStrength = ScreenControlUtility.MapRangeToRange(clampedHandAngle, clickAngle, 180, 1, 0);

            return handAngle < clickAngle;
        }

        private bool ShouldTriggerUngrab(Leap.Hand hand)
        {
            return GetAngle(hand) > unclickAngle;
        }
    }
}
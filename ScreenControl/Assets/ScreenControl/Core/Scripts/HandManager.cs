using UnityEngine;
using Leap;
using Leap.Unity;
using System.Collections;

namespace Ultraleap.ScreenControl.Core
{
    [DefaultExecutionOrder(-1)]
    public class HandManager : MonoBehaviour
    {
        public static HandManager Instance;

        public long Timestamp { get; private set; }

        // The PrimaryHand is the hand that appeared first. It does not change until tracking on it is lost.
        public Hand PrimaryHand { get; private set; }

        // The SecondaryHand is the second hand that appears. It may be promoted to the PrimaryHand if the
        // PrimaryHand is lost.
        public Hand SecondaryHand { get; private set; }

        bool PrimaryIsLeft => PrimaryHand != null && PrimaryHand.IsLeft;
        bool PrimaryIsRight => PrimaryHand != null && !PrimaryHand.IsLeft;
        bool SecondaryIsLeft => SecondaryHand != null && SecondaryHand.IsLeft;
        bool SecondaryIsRight => SecondaryHand != null && !SecondaryHand.IsLeft;

        public Hand LeftHand
        {
            get
            {
                if (PrimaryIsLeft)
                {
                    return PrimaryHand;
                }
                else if (SecondaryIsLeft)
                {
                    return SecondaryHand;
                }
                else
                {
                    return null;
                }
            }
        }

        public Hand RightHand
        {
            get
            {
                if (PrimaryIsRight)
                {
                    return PrimaryHand;
                }
                else if (SecondaryIsRight)
                {
                    return SecondaryHand;
                }
                else
                {
                    return null;
                }
            }
        }

        [HideInInspector] public bool useTrackingTransform = true;
        LeapTransform TrackingTransform;

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }
            Instance = this;

            PhysicalConfigurable.OnConfigUpdated += UpdateTrackingTransform;
        }

        private void Start()
        {
            UpdateTrackingTransform();
        }

        void OnDestroy()
        {
            PhysicalConfigurable.OnConfigUpdated -= UpdateTrackingTransform;
        }

        IEnumerator UpdateTrackingAfterLeapInit()
        {
            while (((LeapServiceProvider)Hands.Provider).GetLeapController() == null)
            {
                yield return null;
            }

            // To simplify the configuration values, positive X angles tilt the Leap towards the screen no matter how its mounted.
            // Therefore, we must convert to the real values before using them.
            // If top mounted, the X rotation should be negative if tilted towards the screen so we must negate the X rotation in this instance.
            var isTopMounted = Mathf.Approximately(PhysicalConfigurable.Config.LeapRotationD.z, 180f);
            float xAngleDegree = isTopMounted ? -PhysicalConfigurable.Config.LeapRotationD.x : PhysicalConfigurable.Config.LeapRotationD.x;

            SetLeapTrackingMode();
            TrackingTransform = new LeapTransform(
                PhysicalConfigurable.Config.LeapPositionRelativeToScreenBottomM.ToVector(),
                Quaternion.Euler(xAngleDegree, PhysicalConfigurable.Config.LeapRotationD.y, PhysicalConfigurable.Config.LeapRotationD.z).ToLeapQuaternion()
            );
        }

        private void UpdateTrackingTransform()
        {
            StartCoroutine(UpdateTrackingAfterLeapInit());
        }

        void SetLeapTrackingMode()
        {
            if (Mathf.Abs(PhysicalConfigurable.Config.LeapRotationD.z) > 90f)
            {
                ((LeapServiceProvider)Hands.Provider).GetLeapController().SetPolicy(Leap.Controller.PolicyFlag.POLICY_OPTIMIZE_HMD);
            }
            else
            {
                ((LeapServiceProvider)Hands.Provider).GetLeapController().ClearPolicy(Leap.Controller.PolicyFlag.POLICY_OPTIMIZE_HMD);
            }
        }

        private void Update()
        {
            if (useTrackingTransform)
            {
                Hands.Provider.CurrentFrame.Transform(TrackingTransform);
            }

            Timestamp = Hands.Provider.CurrentFrame.Timestamp;

            int nHands = Hands.Provider.CurrentFrame.Hands.Count;

            bool foundPrimary = nHands > 0;
            bool foundSecondary = nHands > 1;

            if (foundPrimary)
            {
                PrimaryHand = Hands.Provider.CurrentFrame.Hands[0];
            }
            else
            {
                PrimaryHand = null;
            }

            if (foundSecondary)
            {
                SecondaryHand = Hands.Provider.CurrentFrame.Hands[1];
            }
            else
            {
                SecondaryHand = null;
            }
        }

        public bool IsLeapServiceConnected()
        {
            return ((LeapServiceProvider)Hands.Provider).IsConnected();
        }
    }
}
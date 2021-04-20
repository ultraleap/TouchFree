using System;
using System.Collections;
using System.Diagnostics;
using System.IO;

using UnityEngine;

using Leap;
using Leap.Unity;
using System.Linq;

namespace Ultraleap.ScreenControl.Core
{
    [DefaultExecutionOrder(-1)]
    public class HandManager : MonoBehaviour
    {
        public static HandManager Instance;

        public long Timestamp { get; private set; }

        // The PrimaryHand is the hand that appeared first. It does not change until tracking on it is lost.
        public Hand PrimaryHand;
        Chirality primaryChirality;

        // The SecondaryHand is the second hand that appears. It may be promoted to the PrimaryHand if the
        // PrimaryHand is lost.
        public Hand SecondaryHand;
        Chirality secondaryChirality;

        public delegate void HandPresenceEvent();
        public event HandPresenceEvent HandFound;
        public event HandPresenceEvent HandsLost;

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

        [HideInInspector] public bool screenTopAvailable = false;

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }
            Instance = this;

            PhysicalConfig.OnConfigUpdated += UpdateTrackingTransform;
            CheckLeapVersionForScreentop();
            UpdateTrackingTransform();
        }

        void OnDestroy()
        {
            PhysicalConfig.OnConfigUpdated -= UpdateTrackingTransform;
        }

        void CheckLeapVersionForScreentop()
        {
            // find the LeapSvc.exe as it has the current version
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Leap Motion", "Core Services", "LeapSvc.exe");

            if (File.Exists(path))
            {
                // get the version info from the service
                FileVersionInfo myFileVersionInfo = FileVersionInfo.GetVersionInfo(path);
                string versionString = new string(myFileVersionInfo.FileVersion.Where(c => c == '.' || char.IsDigit(c)).ToArray());

                // parse the version or use default (1.0.0)
                Version version = new Version();
                Version.TryParse(versionString, out version);
                // Virsion screentop is introduced
                Version screenTopVersionMin = new Version(4, 9, 0);

                if (version != null)
                {
                    if (version.CompareTo(screenTopVersionMin) >= 0)
                    {
                        screenTopAvailable = true;
                    }
                }
            }
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
            var isTopMounted = Mathf.Approximately(ConfigManager.PhysicalConfig.LeapRotationD.z, 180f);
            float xAngleDegree = isTopMounted ? -ConfigManager.PhysicalConfig.LeapRotationD.x : ConfigManager.PhysicalConfig.LeapRotationD.x;

            UpdateLeapTrackingMode();
            TrackingTransform = new LeapTransform(
                ConfigManager.PhysicalConfig.LeapPositionRelativeToScreenBottomM.ToVector(),
                Quaternion.Euler(xAngleDegree, ConfigManager.PhysicalConfig.LeapRotationD.y,
                ConfigManager.PhysicalConfig.LeapRotationD.z).ToLeapQuaternion()
            );
        }

        private void UpdateTrackingTransform()
        {
            StartCoroutine(UpdateTrackingAfterLeapInit());
        }

        public void UpdateLeapTrackingMode()
        {
            // leap is looking down
            if (Mathf.Abs(ConfigManager.PhysicalConfig.LeapRotationD.z) > 90f)
            {
                if (screenTopAvailable && ConfigManager.PhysicalConfig.LeapRotationD.x <= 0f)
                {
                    //Screentop
                    SetLeapTrackingMode(MountingType.ABOVE_FACING_USER);
                }
                else
                {
                    //HMD
                    SetLeapTrackingMode(MountingType.ABOVE_FACING_SCREEN);
                }
            }
            else
            {
                //Desktop
                SetLeapTrackingMode(MountingType.BELOW);
            }
        }

        public void SetLeapTrackingMode(MountingType _mount)
        {
            switch (_mount)
            {
                case MountingType.NONE:
                case MountingType.BELOW:
                    ((LeapServiceProvider)Hands.Provider).GetLeapController().ClearPolicy(Controller.PolicyFlag.POLICY_OPTIMIZE_HMD);

                    if (screenTopAvailable)
                    {
                        ((LeapServiceProvider)Hands.Provider).GetLeapController().ClearPolicy(Controller.PolicyFlag.POLICY_OPTIMIZE_SCREENTOP);
                    }
                    break;
                case MountingType.ABOVE_FACING_USER:
                    if (screenTopAvailable)
                    {
                        ((LeapServiceProvider)Hands.Provider).GetLeapController().SetPolicy(Controller.PolicyFlag.POLICY_OPTIMIZE_SCREENTOP);
                    }
                    ((LeapServiceProvider)Hands.Provider).GetLeapController().ClearPolicy(Controller.PolicyFlag.POLICY_OPTIMIZE_HMD);
                    break;
                case MountingType.ABOVE_FACING_SCREEN:
                    ((LeapServiceProvider)Hands.Provider).GetLeapController().SetPolicy(Controller.PolicyFlag.POLICY_OPTIMIZE_HMD);

                    if (screenTopAvailable)
                    {
                        ((LeapServiceProvider)Hands.Provider).GetLeapController().ClearPolicy(Controller.PolicyFlag.POLICY_OPTIMIZE_SCREENTOP);
                    }
                    break;
            }
        }

        private void Update()
        {
            var currentFrame = Hands.Provider.CurrentFrame;

            if (useTrackingTransform)
            {
                currentFrame.Transform(TrackingTransform);
            }

            Timestamp = currentFrame.Timestamp;

            Hand leftHand = null;
            Hand rightHand = null;

            foreach (Hand hand in currentFrame.Hands)
            {
                if (hand.IsLeft)
                    leftHand = hand;
                else
                    rightHand = hand;
            }

            if (currentFrame.Hands.Count == 0 && PrimaryHand != null)
            {
                HandsLost?.Invoke();
            }
            else if (currentFrame.Hands.Count > 0 && PrimaryHand == null)
            {
                HandFound?.Invoke();
            }

            UpdateHandStatus(ref PrimaryHand, leftHand, rightHand, ScreenControlTypes.HandType.PRIMARY);

            UpdateHandStatus(ref SecondaryHand, leftHand, rightHand, ScreenControlTypes.HandType.SECONDARY);
        }

        void UpdateHandStatus(ref Hand _hand, Hand _left, Hand _right, ScreenControlTypes.HandType _handType)
        {
            // We must use the cached Chirality to ensure persistence
            Chirality handChirality;

            if (_handType == ScreenControlTypes.HandType.PRIMARY)
            {
                handChirality = primaryChirality;
            }
            else
            {
                handChirality = secondaryChirality;
            }

            if (_hand == null)
            {
                // Look for a new hand

                if (_handType == ScreenControlTypes.HandType.PRIMARY)
                {
                    AssignNewPrimary(_left, _right);
                }
                else
                {
                    AssignNewSecondary(_left, _right);
                }
            }
            else
            {
                // Check hand is still active

                if (handChirality == Chirality.Left && _left != null)
                {
                    // Hand is still left
                    _hand = _left;
                    return;
                }
                else if (handChirality == Chirality.Right && _right != null)
                {
                    // Hand is still right
                    _hand = _right;
                    return;
                }

                // If we are here, the Hand has been lost. Assign a new Hand.
                if (_handType == ScreenControlTypes.HandType.PRIMARY)
                {
                    AssignNewPrimary(_left, _right);
                }
                else
                {
                    AssignNewSecondary(_left, _right);
                }
            }
        }

        void AssignNewPrimary(Hand _left, Hand _right)
        {
            // When assigning a new primary, we should force Secondary to be re-assigned too
            PrimaryHand = null;
            SecondaryHand = null;

            if (_right != null)
            {
                PrimaryHand = _right;
                primaryChirality = Chirality.Right;
            }
            else if (_left != null)
            {
                PrimaryHand = _left;
                primaryChirality = Chirality.Left;
            }
        }

        void AssignNewSecondary(Hand _left, Hand _right)
        {
            SecondaryHand = null;

            if (_right != null && primaryChirality != Chirality.Right)
            {
                SecondaryHand = _right;
                secondaryChirality = Chirality.Right;
            }
            else if (_left != null && primaryChirality != Chirality.Left)
            {
                SecondaryHand = _left;
                secondaryChirality = Chirality.Left;
            }
        }

        public bool IsLeapServiceConnected()
        {
            return ((LeapServiceProvider)Hands.Provider).IsConnected();
        }
    }
}
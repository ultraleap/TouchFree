using System.Collections;
using UnityEngine;
using Leap.Unity;
using Ultraleap.TouchFree.ServiceShared;

namespace Ultraleap.TouchFree.ServiceUI
{
    public class QuickSetupScreen : ConfigScreen
    {
        public GameObject trackingLost;
        public GameObject setupGuideButton;

        [Space]
        public int trackingFailsToSetupGuide = 3;

        int noTrackingAttempts = 0;

        public GameObject completeScreen;

        Vector3 pos1;
        Vector3 pos2;

        bool pos1Taken = false;
        bool pos2Taken = false;
        bool beganDrawingBox = false;

        public float minFingersTogetherTimer = 2;
        public float maxFingersTogetherDistance = 0.02f;

        public float currentFingersTogetherTimer = 0;

        public UnityEngine.UI.Text text;

        protected override void OnEnable()
        {
            base.OnEnable();
            // reset the quick setup
            pos1Taken = false;
            pos2Taken = false;
            beganDrawingBox = false;
            ScreenManager.Instance.SetCursorState(false);
            HandManager.Instance.useTrackingTransform = false;
            HandManager.Instance.lockTrackingMode = true;
            DisplayTrackingLost(false);
            setupGuideButton.SetActive(false);
            noTrackingAttempts = 0;
        }

        private void OnDisable()
        {
            HandManager.Instance.useTrackingTransform = true;
            HandManager.Instance.lockTrackingMode = false;
            ScreenManager.Instance.SetCursorState(true);
        }

        private void Update()
        {
            if (!pos2Taken && HandManager.Instance.PrimaryHand != null && HandManager.Instance.SecondaryHand != null)
            {
                Vector3 primaryFingerPos = HandManager.Instance.PrimaryHand.GetIndex().TipPosition.ToVector3();
                Vector3 secondaryFingerPos = HandManager.Instance.SecondaryHand.GetIndex().TipPosition.ToVector3();

                if(AreFingersTogether(primaryFingerPos, secondaryFingerPos))
                {
                    currentFingersTogetherTimer += Time.deltaTime;

                    if (!beganDrawingBox)
                    {
                        if (!pos1Taken)
                        {
                            text.text = "HOLD your INDEX fingers together: " + (minFingersTogetherTimer - currentFingersTogetherTimer).ToString("F2");
                        }

                        if (!pos1Taken && currentFingersTogetherTimer >= minFingersTogetherTimer)
                        {
                            pos1 = (primaryFingerPos + secondaryFingerPos) / 2;

                            pos1Taken = true;
                            text.text = "Draw your INDEX fingers around the edge of the screen until they are TOUCHING at the middle of the BOTTOM of the screen";
                        }
                    }
                    else
                    {
                        if (!pos2Taken)
                        {
                            text.text = "HOLD your INDEX fingers together: " + (minFingersTogetherTimer - currentFingersTogetherTimer).ToString("F2");
                        }

                        if (currentFingersTogetherTimer >= minFingersTogetherTimer)
                        {
                            pos2 = (primaryFingerPos + secondaryFingerPos) / 2;
                            pos2Taken = true;
                            text.text = "Completing Setup";
                            CalculatePositions();
                        }
                    }
                }
                else
                {
                    if(!pos1Taken)
                    {
                        text.text = "Place your INDEX fingers together while TOUCHING the middle of the TOP of the screen";
                    }
                    else if(!pos2Taken)
                    {
                        text.text = "Draw your INDEX fingers around the edge of the screen until they are TOUCHING at the middle of the BOTTOM of the screen";
                    }
                    else
                    {
                        text.text = "Completing Setup";
                    }

                    currentFingersTogetherTimer = 0;

                    if (pos1Taken)
                    {
                        beganDrawingBox = true;
                    }
                }
            }
            else
            {
                if(!pos1Taken)
                {
                    text.text = "Place your INDEX fingers together while TOUCHING the middle of the TOP of the screen";
                }
                else if(!beganDrawingBox)
                {
                    text.text = "Draw your INDEX fingers around the edge of the screen until they are TOUCHING at the middle of the BOTTOM of the screen";
                }
                else if(!pos2Taken)
                {
                    text.text = "Place your INDEX fingers together while TOUCHING the middle of the BOTTOM of the screen";
                }
                else
                {
                    text.text = "All done :)";
                }
            }
        }

        bool AreFingersTogether(Vector3 _pos1, Vector3 _pos2)
        {
            if (Vector3.Distance(_pos1, _pos2) < maxFingersTogetherDistance)
            {
                return true;
            }

            return false;
        }

        void CalculatePositions()
        {
            if(pos1.y < pos2.y)
            {
                CompleteQuickSetup(pos1, pos2);
            }
            else
            {
                CompleteQuickSetup(pos2, pos1);
            }
        }

        void CompleteQuickSetup(Vector3 bottomPos, Vector3 topPos)
        {
            ConfigManager.PhysicalConfig.SetAllValuesToDefault();

            CalculateConfigurationValues(bottomPos, topPos);
            VirtualScreen.CaptureCurrentResolution();

            ConfigManager.PhysicalConfig.SaveConfig();

            ScreenManager.Instance.ChangeScreen(completeScreen);
        }

        public void CalculateConfigurationValues(Vector3 bottomPos, Vector3 topPos)
        {
            Vector3 bottomNoX = new Vector3(0, bottomPos.y, bottomPos.z);
            Vector3 topNoX = new Vector3(0, topPos.y, topPos.z);

            ConfigManager.PhysicalConfig.ScreenHeightM = Vector3.Distance(bottomNoX, topNoX);

            ConfigManager.PhysicalConfig.LeapRotationD = LeapRotationRelativeToScreen(bottomPos, topPos);
            ConfigManager.PhysicalConfig.LeapPositionRelativeToScreenBottomM = LeapPositionInScreenSpace(bottomPos, ConfigManager.PhysicalConfig.LeapRotationD);
            ConfigManager.PhysicalConfig.ConfigWasUpdated();
        }

        /// <summary>
        /// Find the position of the camera relative to the screen, using the screen position relative to the camera.
        /// </summary>
        private Vector3 LeapPositionInScreenSpace(Vector3 bottomEdgeRef, Vector3 leapRotation)
        {
            // In Leap Co-ords we know the Leap is at Vector3.zero, and that the bottom of the screen is at "bottomEdgeRef"

            // We know the Leap is rotated at "leapRotation" from the screen.
            // We want to calculate the Vector from the bottom of the screen to the Leap in this rotated co-ord system.

            Vector3 rotationAngles = leapRotation;
            if (ScreenManager.Instance.selectedMountType == MountingType.ABOVE_FACING_SCREEN ||
                    ScreenManager.Instance.selectedMountType == MountingType.ABOVE_FACING_USER)
            {
                // In overhead mode, the stored 'x' angle is inverted so that positive angles always mean
                // the camera is pointed towards the screen. Multiply by -1 here so that it can be used
                // in a calculation.
                rotationAngles.x *= -1f;
            }
            Vector3 rotatedVector = Quaternion.Euler(rotationAngles) * bottomEdgeRef;

            // Multiply by -1 so the vector is from screen to camera
            Vector3 leapPosition = rotatedVector * -1f;
            return leapPosition;
        }

        /// <summary>
        /// Find the angle between the camera and the screen.
        /// Ensure a positive angle always means rotation towards the screen.
        /// </summary>
        public Vector3 LeapRotationRelativeToScreen(Vector3 bottomCentre, Vector3 topCentre)
        {
            Vector3 directionBottomToTop = topCentre - bottomCentre;
            Vector3 rotation = Vector3.zero;

            if (ScreenManager.Instance.selectedMountType == MountingType.ABOVE_FACING_SCREEN ||
                    ScreenManager.Instance.selectedMountType == MountingType.ABOVE_FACING_USER)
            {
                rotation.x = -Vector3.SignedAngle(Vector3.up, directionBottomToTop, Vector3.right) + 180;
                rotation.z = 180;
            }
            else
            {
                rotation.x = Vector3.SignedAngle(Vector3.up, directionBottomToTop, Vector3.left);
            }

            rotation.x = ServiceUtility.CentreRotationAroundZero(rotation.x);
            return rotation;
        }

        void DisplayTrackingLost(bool _display = true)
        {
            trackingLost.SetActive(_display);

            if (_display)
            {
                StartCoroutine(HideTrackingLostAfterTime());

                noTrackingAttempts++;
                if (noTrackingAttempts >= trackingFailsToSetupGuide)
                {
                    setupGuideButton.SetActive(true);
                }
            }
        }

        IEnumerator HideTrackingLostAfterTime()
        {
            yield return new WaitForSeconds(2);
            DisplayTrackingLost(false);
        }
    }
}
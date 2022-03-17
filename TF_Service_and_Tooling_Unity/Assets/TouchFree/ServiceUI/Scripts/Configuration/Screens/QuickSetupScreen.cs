﻿using System.Collections;
using UnityEngine;
using Leap.Unity;
using Ultraleap.TouchFree.ServiceShared;

namespace Ultraleap.TouchFree.ServiceUI
{
    public class QuickSetupScreen : ConfigScreen
    {
        public GameObject step1;
        public GameObject step2;
        public GameObject target1;
        public GameObject target2;
        public GameObject trackingLost;
        public GameObject setupGuideButton;

        [Space]
        public int trackingFailsToSetupGuide = 3;

        Vector3 bottomPosM;
        Vector3 topPosM;

        int noTrackingAttempts = 0;

        public GameObject completeScreen;

        private const float TARGET_DIST_FROM_EDGE_PERCENTAGE = 0.1f;
        private const float HEIGHT_SCALING_FACTOR = 1f / (1f - (2 * TARGET_DIST_FROM_EDGE_PERCENTAGE));
        private const float EDGE_SCALING_FACTOR = ( ( HEIGHT_SCALING_FACTOR - 1f ) / 2f ) + 1f;

        private void OnEnable()
        {
            base.OnEnable();
            // reset the quick setup
            bottomPosM = Vector3.zero;
            topPosM = Vector3.zero;
            SetTargetPositions();
            step1.SetActive(true);
            step2.SetActive(false);
            ScreenManager.Instance.SetCursorState(false, true);
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
            ScreenManager.Instance.SetCursorState(true, true);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (bottomPosM == Vector3.zero)
                {
                    if (HandManager.Instance.PrimaryHand != null)
                    {
                        SetBottomPos(HandManager.Instance.PrimaryHand.GetIndex().TipPosition.ToVector3());
                        // display second quick setup screen
                        step1.SetActive(false);
                        step2.SetActive(true);
                    }
                    else
                    {
                        // Display a notification that tracking was lost
                        DisplayTrackingLost();
                    }
                }
                else if (topPosM == Vector3.zero)
                {
                    if (HandManager.Instance.PrimaryHand != null)
                    {
                        SetTopPos(HandManager.Instance.PrimaryHand.GetIndex().TipPosition.ToVector3());
                        CompleteQuickSetup(bottomPosM, topPosM);
                    }
                    else
                    {
                        // Display a notification that tracking was lost
                        DisplayTrackingLost();
                    }
                }
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

            ConfigManager.PhysicalConfig.ScreenHeightM = Vector3.Distance(bottomNoX, topNoX) * HEIGHT_SCALING_FACTOR;

            var bottomEdge = BottomCentreFromTouches(bottomPos, topPos);
            var topEdge = TopCentreFromTouches(bottomPos, topPos);

            ConfigManager.PhysicalConfig.LeapRotationD = LeapRotationRelativeToScreen(bottomPos, topPos);
            ConfigManager.PhysicalConfig.LeapPositionRelativeToScreenBottomM = LeapPositionInScreenSpace(bottomEdge, ConfigManager.PhysicalConfig.LeapRotationD);
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
        /// BottomTouch -> TopTouch is 1/8th screen height as touch points are placed 10% in from the edge.
        /// We need to offset the touch point by 1/10th of screen height = 1/8th of the distance between touch points.
        /// For this we can Lerp from bottom to top touch travelling an extra 8th distance.
        /// </summary>
        public Vector3 TopCentreFromTouches(Vector3 bottomTouch, Vector3 topTouch)
        {
            return Vector3.LerpUnclamped(bottomTouch, topTouch, EDGE_SCALING_FACTOR);
        }

        /// <summary>
        /// TopTouch -> BottomTouch is 1/8th screen height as touch points are placed 10% in from the edge.
        /// We need to offset the touch point by 1/10th of screen height = 1/8th of the distance between touch points.
        /// For this we can Lerp from top to bottom touch travelling an extra 8th distance
        /// </summary>
        public Vector3 BottomCentreFromTouches(Vector3 bottomTouch, Vector3 topTouch)
        {
            return Vector3.LerpUnclamped(topTouch, bottomTouch, EDGE_SCALING_FACTOR);
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

        public void SetTopPos(Vector3 position)
        {
            topPosM = position;
        }

        public void SetBottomPos(Vector3 position)
        {
            bottomPosM = position;
        }

        private void SetTargetPositions()
        {
            // Canvas is set to scale from 1920 * 1080 to local screen size,
            // so relative positions have to be in that scale
            var verticalDistance = 1080f * TARGET_DIST_FROM_EDGE_PERCENTAGE;

            target1.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, verticalDistance, 0f);
            target2.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, -1f * verticalDistance, 0f);
        }

        public void CancelQuickSetup()
        {
            HandManager.Instance.lockTrackingMode = false;
            HandManager.Instance.useTrackingTransform = true;
            HandManager.Instance.UpdateTrackingTransformAndMode();
            ScreenManager.Instance.PreviousScreen();
        }
    }
}
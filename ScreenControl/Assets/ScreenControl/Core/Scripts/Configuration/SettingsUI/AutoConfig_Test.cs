using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Ultraleap.ScreenControl.Core
{
    public class AutoConfig_Test : MonoBehaviour
    {
        public AutoConfig autoConfig;

        public Transform screen;
        public Transform leap;
        public Transform bottomTouch;
        public Transform bottomEdge;
        public Transform topTouch;
        public Transform topEdge;

        public Transform virtualScreen;

        public Vector3 bottomTouchInLeapSpaceT;
        public Vector3 bottomEdgeInLeapSpaceT;
        public Vector3 topTouchInLeapSpaceT;
        public Vector3 topEdgeInLeapSpaceT;
        public Vector3 leapOffsetInScreenSpaceT;
        public float angleBetweenLeapScreenT;
        public float screenHeightT;

        public Vector3 bottomTouchInLeapSpaceM;
        public Vector3 bottomEdgeInLeapSpaceM;
        public Vector3 topTouchInLeapSpaceM;
        public Vector3 topEdgeInLeapSpaceM;
        public Vector3 leapOffsetInScreenSpaceM;
        public float angleBetweenLeapScreenM;
        public float screenHeightM;

        public bool overHead = true;

        public string bottomEdgeTest = "";
        public string topEdgeTest = "";
        public string angleTest = "";
        public string leapOriginTest = "";
        public string screenHeightTest = "";

        public Text bottomEdgeText;
        public Text topEdgeText;
        public Text angleText;
        public Text leapOriginText;
        public Text screenHeightText;

        public Text passFail;

        public int passcount;
        public int failCount;
        public bool autoRun;


        // Start is called before the first frame update
        void Start()
        {

            for (int i = 0; i < 1000; i++)
            {
                RandomKioskAutoConfiguresCorrectly();
            }
            if (passcount == 1000)
                print("Self Test Passed!");
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown("space"))
            {
                RandomKioskAutoConfiguresCorrectly();
            }
            if (autoRun)
            {
                RandomKioskAutoConfiguresCorrectly();
            }

            DrawSomeLines();
            //autoConfig.DrawBasis();
        }

        public void AutoRun(bool run)
        {
            autoRun = run;
        }

        public void RandomKioskAutoConfiguresCorrectly()
        {
            // Given
            PlaceScreen();
            RandomlyPlaceLeap();
            ReportTestSetup();
            // When
            CalculateScreenAndCameraValues(leap.InverseTransformPoint(bottomTouch.position), leap.InverseTransformPoint(topTouch.position));
            TransformTestScreen();
            // Then
            CompareValues();
        }

        public void PlaceScreen()
        {
            screen.position = new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(0.2f, 0.7f), Random.Range(-0.5f, 0.5f));
            screen.rotation = Quaternion.Euler(Random.Range(-30f, 30f), 0, 0);
        }

        public void RandomlyPlaceLeap()
        {
            if (Random.Range(-100, 100) > 0)
            {
                PlaceLeapTop();
            }
            else
            {
                PlaceLeapBottom();
            }
        }

        public void PlaceLeapTop()
        {
            ConfigurationSetupController.selectedMountType = MountingType.OVERHEAD;

            leap.position = screen.TransformPoint(Random.Range(-0.1f, 0.1f), Random.Range(0.25f, 0.75f), Random.Range(-0.5f, 0));
            leap.rotation = Quaternion.Euler(Random.Range(-30f, 30f), 0, 180);
        }

        public void PlaceLeapBottom()
        {
            ConfigurationSetupController.selectedMountType = MountingType.BOTTOM;

            leap.position = screen.TransformPoint(Random.Range(-0.1f, 0.1f), Random.Range(-0.25f, -0.75f), Random.Range(-0.5f, 0));
            leap.rotation = Quaternion.Euler(Random.Range(-30f, 30f), 0, 0);
        }

        public void ReportTestSetup()
        {
            bottomTouchInLeapSpaceT = leap.InverseTransformPoint(bottomTouch.position);
            bottomEdgeInLeapSpaceT = leap.InverseTransformPoint(bottomEdge.position);
            topTouchInLeapSpaceT = leap.InverseTransformPoint(topTouch.position);
            topEdgeInLeapSpaceT = leap.InverseTransformPoint(topEdge.position);
            leapOffsetInScreenSpaceT = bottomEdge.InverseTransformPoint(leap.position);
            screenHeightT = Vector3.Distance(bottomEdge.position, topEdge.position);

            if (ConfigurationSetupController.selectedMountType == MountingType.OVERHEAD)
            {
                angleBetweenLeapScreenT = autoConfig.CentreRotationAroundZero(Vector3.SignedAngle(leap.up, -screen.up, Vector3.right));
            }
            else
            {
                angleBetweenLeapScreenT = autoConfig.CentreRotationAroundZero(Vector3.SignedAngle(leap.up, screen.up, Vector3.left));
            }
        }

        public void DrawSomeLines()
        {
            Debug.DrawLine(leap.position, leap.position + leap.up, Color.green);
            Debug.DrawLine(screen.position, screen.position + screen.up, Color.cyan);
            Debug.DrawLine(bottomEdge.position, bottomEdge.TransformPoint(leapOffsetInScreenSpaceT), Color.red);

            Debug.DrawLine(bottomEdge.position, bottomEdge.TransformPoint(leapOffsetInScreenSpaceM), Color.magenta);
        }

        public void CompareValues()
        {
            bool fail = false;
            if (Vector3.Distance(bottomEdgeInLeapSpaceT, bottomEdgeInLeapSpaceM) < 0.05f)
            {
                bottomEdgeTest = "PASSED";
            }
            else
            {
                bottomEdgeTest = "FAILED";
                fail = true;
            }

            if (Vector3.Distance(topEdgeInLeapSpaceT, topEdgeInLeapSpaceM) < 0.05f)
            {
                topEdgeTest = "PASSED";
            }
            else
            {
                topEdgeTest = "FAILED";
                fail = true;
            }

            if (Mathf.Abs(angleBetweenLeapScreenT - angleBetweenLeapScreenM) < 0.5f)
            {
                angleTest = "PASSED";
            }
            else
            {
                angleTest = "FAILED";
                fail = true;
            }

            var leapError = Vector3.Distance(leapOffsetInScreenSpaceT, leapOffsetInScreenSpaceM);
            if (leapError < 0.05f)
            {
                leapOriginTest = "PASSED";
            }
            else
            {
                leapOriginTest = "Error = " + leapError.ToString("F3");
                fail = true;
            }

            if (Mathf.Abs(screenHeightT - screenHeightM) < 0.05f)
            {
                screenHeightTest = "PASSED";
            }
            else
            {
                screenHeightTest = "FAILED";
                fail = true;
            }

            if (fail)
                failCount++;
            else
                passcount++;

            bottomEdgeText.text = "Bottom edge position: " + bottomEdgeTest;
            topEdgeText.text = "Top edge position: " + topEdgeTest;
            angleText.text = "Leap/Screen angle: " + angleTest;
            leapOriginText.text = "Leap position: " + leapOriginTest;
            screenHeightText.text = "Screen Height: " + screenHeightTest;
            passFail.text = "Passes: " + passcount + ", Fails: " + failCount;
        }

        void CalculateScreenAndCameraValues(Vector3 bottomPos, Vector3 topPos)
        {
            autoConfig.SetTopPos(topPos);
            autoConfig.SetBottomPos(bottomPos);

            autoConfig.CalculateConfigurationValues(bottomPos, topPos);

            bottomEdgeInLeapSpaceM = autoConfig.BottomCentreFromTouches(bottomPos, topPos);
            topEdgeInLeapSpaceM = autoConfig.TopCentreFromTouches(bottomPos, topPos);

            leapOffsetInScreenSpaceM = ConfigManager.PhysicalConfig.LeapPositionRelativeToScreenBottomM;
            angleBetweenLeapScreenM = ConfigManager.PhysicalConfig.LeapRotationD.x;
            screenHeightM = ConfigManager.PhysicalConfig.ScreenHeightM;

            bottomTouchInLeapSpaceM = bottomPos;
            topTouchInLeapSpaceM = topPos;
        }

        void TransformTestScreen()
        {
            virtualScreen.position = leap.TransformPoint(bottomEdgeInLeapSpaceM);
            virtualScreen.rotation = Quaternion.FromToRotation(Vector3.up, leap.TransformPoint(topEdgeInLeapSpaceM) - leap.TransformPoint(bottomEdgeInLeapSpaceM));
            virtualScreen.GetChild(0).localScale = new Vector3(screenHeightM * (10f / 16f), screenHeightM, 0.05f);
        }
    }
}
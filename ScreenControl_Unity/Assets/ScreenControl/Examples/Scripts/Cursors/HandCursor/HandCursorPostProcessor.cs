using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;
using Leap;
using Ultraleap.ScreenControl.Core;

public class HandCursorPostProcessor : PostProcessProvider
{
    public Transform leftCursor;
    public Transform rightCursor;

    public bool overhead = false;


    [Range(0.1f, 10f)] public float handScale = 1.0f;
    public bool leftHandActive = true;
    public bool rightHandActive = true;

    public Vector3 leftOffsetVector;
    public Vector3 rightOffsetVector;

    // Start is called before the first frame update
    void Awake()
    {
        // Attempt to assign the LeapServiceProvider if one is not assigned
        if (_inputLeapProvider == null)
        {
            _inputLeapProvider = GameObject.Find("LeapHandController").GetComponent<LeapProvider>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Check if the camera is mounted pointing down
        overhead = (Mathf.Abs(ConfigManager.PhysicalConfig.LeapRotationD.z) > 90f);
    }

    public override void ProcessFrame(ref Frame inputFrame)
    {
        if (leftCursor == null || rightCursor == null) return;

        foreach (var hand in inputFrame.Hands) {

            // Scale all joints based on palm position
            foreach(var finger in hand.Fingers)
            {
                foreach (var bone in finger.bones)
                {
                    var offset = (bone.NextJoint - hand.PalmPosition) * handScale;
                    bone.NextJoint = (hand.PalmPosition + offset);
                    offset = (bone.PrevJoint - hand.PalmPosition) * handScale;
                    bone.PrevJoint = (hand.PalmPosition + offset);
                }
            }
            hand.WristPosition = hand.PalmPosition + ((hand.WristPosition - hand.PalmPosition) * handScale);

            Quaternion handRotation = overhead ? TopDownRotatedHand(hand.Rotation.ToQuaternion()) : hand.Rotation.ToQuaternion();

            if (hand.IsLeft)
            {
                hand.SetTransform(
                    leftCursor.position + (leftOffsetVector * handScale),
                    leftCursor.rotation * handRotation
                    );
            }
            else
            {
                hand.SetTransform(
                    rightCursor.position + (rightOffsetVector * handScale),
                    rightCursor.rotation * handRotation
                    );
            }
        }
    }


    public bool HandIsActive(Leap.Hand hand)
    {
        return ((hand.IsLeft && leftHandActive) || (hand.IsRight && rightHandActive));
    }

    public void SetHandScale(float scale)
    {
        handScale = Mathf.Max(0.01f, scale);
    }

    private Quaternion TopDownRotatedHand(Quaternion handRotation)
    {
        return Quaternion.AngleAxis(180, Vector3.forward) * handRotation;
    }

}

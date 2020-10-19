using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Leap;
using Leap.Unity;

public class PokePose : MonoBehaviour
{    
    public float pokePoseThreshold = 0.4f;
    public float unPokePoseThreshold = 0.2f;

    public bool inPose = false;

    private float Overlap(Vector3 a, Vector3 b)
    {
        // Return 1 if the vectors are in the same direction
        // Return 0.5 if they are orthogonal
        // Return 0 if they are opposite each other
        return 0.5f * (1.0f + Vector3.Dot(a.normalized, b.normalized));
    }

    private Vector3 GetFingerVector(Finger finger)
    {
        return finger.TipPosition.ToVector3() - finger.bones[1].NextJoint.ToVector3();
    }

    public bool InPose(Hand hand)
    {
        float poseStrength = PoseStrength(hand);
        if(inPose && (poseStrength < unPokePoseThreshold))
        {
            inPose = false;
        }
        else if (!inPose && (poseStrength > pokePoseThreshold))
        {
            inPose = true;
        }
        return inPose;
    }

    public float PoseStrength(Hand hand)
    {
        // Return the strength of the poke-pose
        // 0 = definitely not a poke-pose
        // 1 = definitely a poke-pose

        Vector3 indexFingerVec = GetFingerVector(hand.GetIndex());
        Vector3 ringFingerVec = GetFingerVector(hand.GetRing());
        return 1.0f - Overlap(indexFingerVec, ringFingerVec);
    }


}

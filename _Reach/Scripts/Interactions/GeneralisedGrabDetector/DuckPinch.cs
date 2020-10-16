using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Leap.Unity;

public class DuckPinch : MonoBehaviour
{
    [Range(0f, 0.04f)]
    public float pinchActivateDistance = 0.01f;

    [Range(0f, 0.04f)]
    public float pinchDeactivateDistance = 0.025f;
    
    public float metric;

    private float strengthZeroDistance = 0.08f;
    public float DuckPinchStrength { 
        get {
            if (grabbing)
            {
                return 1;
            }

            float duckPinchStrength = ReachUtility.MapRangeToRange(metric, pinchActivateDistance, strengthZeroDistance, 1, 0);
            duckPinchStrength = Mathf.Clamp(duckPinchStrength, 0, 1);
            return duckPinchStrength;        
        } 
    }

    private bool grabbing;

    void Start()
    {
        grabbing = false;
    }
    
    public bool IsGrabbing(Leap.Hand hand)
    {
        if (!grabbing)
        {
            grabbing = ShouldGrab(hand);
        }
        else
        {
            grabbing = !ShouldUngrab(hand);
        }
        return grabbing;
    }

    public static float DuckPinchDistance(Leap.Hand hand)
    {
        Vector3 thumbDistal = hand.GetThumb().bones[3].PrevJoint.ToVector3();
        Vector3 thumbTip = hand.GetThumb().TipPosition.ToVector3();

        Vector3 indexMetacarpal = hand.GetIndex().bones[0].PrevJoint.ToVector3();
        Vector3 indexProximal = hand.GetIndex().bones[1].PrevJoint.ToVector3();
        Vector3 indexTip = hand.GetIndex().TipPosition.ToVector3();

        Vector3 middleMetacarpal = hand.GetMiddle().bones[0].PrevJoint.ToVector3();
        Vector3 middleTip = hand.GetMiddle().bones[3].PrevJoint.ToVector3();

        Vector3 ringMetacarpal = hand.GetMiddle().bones[0].PrevJoint.ToVector3();
        Vector3 ringTip = hand.GetRing().bones[3].PrevJoint.ToVector3();


        // Project all except thumb onto a plane
        Vector3 projectionPlane = Vector3.Cross(hand.GetIndex().bones[2].Direction.ToVector3(), hand.PalmarAxis()).normalized;

        if (hand.IsRight)
        {
            // convert to left-handed-rule for Unity
            projectionPlane *= -1f;
        }

        Vector3 planeOrigin = indexProximal;

        indexProximal = Vector3.zero;
        indexMetacarpal = Vector3.ProjectOnPlane(indexMetacarpal - planeOrigin, projectionPlane) + planeOrigin;
        indexTip = Vector3.ProjectOnPlane(indexTip - planeOrigin, projectionPlane) + planeOrigin;
        middleMetacarpal = Vector3.ProjectOnPlane(middleMetacarpal - planeOrigin, projectionPlane) + planeOrigin;
        middleTip = Vector3.ProjectOnPlane(middleTip - planeOrigin, projectionPlane) + planeOrigin;
        ringMetacarpal = Vector3.ProjectOnPlane(ringMetacarpal - planeOrigin, projectionPlane) + planeOrigin;
        ringTip = Vector3.ProjectOnPlane(ringTip - planeOrigin, projectionPlane) + planeOrigin;


        // Limit thumb positions
        float thumbDistalOverlap = Vector3.Dot(thumbDistal - planeOrigin, projectionPlane);
        if (thumbDistalOverlap < 0) {
            thumbDistal += thumbDistalOverlap * projectionPlane;
        }
        float thumbTipOverlap = Vector3.Dot(thumbTip - planeOrigin, projectionPlane);
        if (thumbTipOverlap < 0) {
            thumbTip += thumbTipOverlap * projectionPlane;
        }
        
        float indexMetric = SegmentDisplacement.SegmentToSegmentDistance(indexMetacarpal, indexTip, thumbDistal, thumbTip);
        float middleMetric = SegmentDisplacement.SegmentToSegmentDistance(middleMetacarpal, middleTip, thumbDistal, thumbTip);
        float ringMetric = SegmentDisplacement.SegmentToSegmentDistance(ringMetacarpal, ringTip, thumbDistal, thumbTip);
        
        float averageDistance = (indexMetric + middleMetric + ringMetric) / (3.0f);
        return averageDistance;
    }

    private float DuckPinchMetric(Leap.Hand hand)
    {
        float wipMetric = DuckPinchDistance(hand);
        metric = Mathf.Max(0f, wipMetric - 0.01f);
        return metric;
    }

    private bool ShouldGrab(Leap.Hand hand)
    {

        return DuckPinchMetric(hand) < pinchActivateDistance;
    }

    private bool ShouldUngrab(Leap.Hand hand)
    {
        return DuckPinchMetric(hand) > pinchDeactivateDistance;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;
using Leap;


public class PhysicsGrab : MonoBehaviour
{
    public bool Grabbing {get; private set;}
    public float GrabStrength {get; private set;}


    private float previousHandMetric = 0f;
    private long previousTimestamp = 0;

    public bool useDeadzone = true;
    public float handMetricDeadzoneSize = 0.05f;

    public float maxStiffness = 10f;

    public bool useVelocityThresholds = true;
    public float triggerVelocityThreshold = 1.0f;
    public float untriggerVelocityThreshold = 0.001f;
    private bool gestureTriggered = false;
    private bool gestureIsForward = false;
    private float gestureTriggeredStiffness = 0f;


    private float debug_Metric = 0f;
    private float debug_CursorVelocity = 0f;


    public enum MetricType {
        INVERSE_VOLUME,
        SNAIL,
        SWAN,
    }
    public MetricType physicsMetric = MetricType.SNAIL;

    private Dictionary<MetricType, Vector2> metricBounds = new Dictionary<MetricType, Vector2>
    {
        {MetricType.INVERSE_VOLUME, new Vector2(0.5f, 10f)},
        {MetricType.SNAIL, Vector2.zero},
        {MetricType.SWAN, Vector2.zero}
    };

    void Start()
    {
        Grabbing = false;
        GrabStrength = 0f;
    }

    public void UpdateData(long timestamp, Hand hand, float cursorVelocity)
    {
        float newHandMetric = HandMetric(hand);
        if (previousHandMetric == 0f)
        {
            // For first frame
            previousHandMetric = newHandMetric;
            previousTimestamp = timestamp;
            return;
        }

        float stabilisedHandMetric = newHandMetric;
        if (useDeadzone) 
        {
            if (Mathf.Abs(newHandMetric - previousHandMetric) > handMetricDeadzoneSize)
            {
                float sign = (newHandMetric > previousHandMetric) ? -1f : 1f;
                stabilisedHandMetric = newHandMetric + sign * handMetricDeadzoneSize;
            }
            else
            {
                stabilisedHandMetric = previousHandMetric;
            }
        }
        
        float dx = stabilisedHandMetric - previousHandMetric;
        long dt_microseconds = timestamp - previousTimestamp;
        float dt = (dt_microseconds) / (1000f * 1000f); // seconds

        float v = dx / dt;

        float k = maxStiffness;

        if(useVelocityThresholds)
        {
            if (!gestureTriggered && Mathf.Abs(v) > triggerVelocityThreshold)
            {
                gestureTriggered = true;
                gestureIsForward = v > 0;
                if (Grabbing)
                {
                    gestureTriggeredStiffness = maxStiffness;
                }
                else
                {
                    gestureTriggeredStiffness = CalculateTriggerStiffness(hand);
                    //Debug.Log(Time.time.ToString() + " " + gestureTriggeredStiffness.ToString());
                }
            }
            else if (gestureTriggered)
            {
                float sign = gestureIsForward ? 1f : -1f;
                if (sign * v < untriggerVelocityThreshold)
                {
                    gestureTriggered = false;
                }
            }

            if(!gestureTriggered)
            {
                v = 0;
            }
        }

        //Vector2 vBounds = new Vector2(0.1f, 1f);
        //Vector2 kBounds = new Vector2(0.1f, 0.01f);


        //Debug.Log(timestamp.ToString() + " " + v.ToString("F6"));


        if (cursorVelocity > 0.15f && !Grabbing)
        {
            v = -1f * cursorVelocity;
        }

        if (gestureTriggered)
        {
            k = gestureTriggeredStiffness;
        }
        float dG = k * v * dt;

        /**
        if (cursorVelocity > 0.15f)
        {
            float k2 = 10.0f;
            dG -= k2 * cursorVelocity * dt;
        }
        */



        debug_CursorVelocity = cursorVelocity;

        GrabStrength += dG;
        GrabStrength = Mathf.Clamp01(GrabStrength);

        if (!Grabbing && GrabStrength == 1f)
        {
            Grabbing = true;
        }
        else if (Grabbing && GrabStrength < 0.9f)
        {
            Grabbing = false;
        }

        previousHandMetric = stabilisedHandMetric;
        previousTimestamp = timestamp;
    }

    private float HandMetric(Hand hand)
    {
        float currentMetric = 0f;
        
        switch (physicsMetric)
        {
            case MetricType.INVERSE_VOLUME:
                currentMetric = InverseHandVolume(hand);
                break;
            case MetricType.SNAIL:
                currentMetric = SnailMetric(hand);
                break;
            case MetricType.SWAN:
                currentMetric = SwanMetric(hand);
                break;
        }

        debug_Metric = currentMetric;

        Vector2 bounds = metricBounds[physicsMetric];
        float scaledMetric = currentMetric;
        if (bounds != Vector2.zero)
        {
            scaledMetric = (currentMetric - bounds[0]) / (bounds[1] - bounds[0]);
        }
        return scaledMetric;
    }

    private float InverseHandVolume(Hand hand)
    {
        float idxDist = DistalWristDistance(hand.GetIndex());
        float midDist = DistalWristDistance(hand.GetMiddle());
        float ringDist = DistalWristDistance(hand.GetRing());
        float pinkyDist = DistalWristDistance(hand.GetPinky());

        float avgDistalWristDist = 0.25f * (idxDist + midDist + ringDist + pinkyDist);
        float thumbPinkyDist = ThumbPinkyDist(hand);

        float sphereDiameter= 0.5f * (avgDistalWristDist + thumbPinkyDist);

        float handVolume = (4.0f / 3.0f) * Mathf.PI * Mathf.Pow(0.5f * sphereDiameter, 3);
        const float VOLUME_FACTOR = 1000f;

        return 1.0f / (VOLUME_FACTOR * handVolume);
    }

    private float DistalWristDistance(Finger finger)
    {
        Vector3 metacarpalPos = finger.bones[0].PrevJoint.ToVector3();
        Vector3 distalPos = finger.bones[2].NextJoint.ToVector3();
        return (metacarpalPos - distalPos).magnitude;
    }

    private float ThumbPinkyDist(Hand hand)
    {
        Vector3 thumbDistalPos = hand.GetThumb().bones[2].NextJoint.ToVector3();
        Vector3 pinkyDistalPos = hand.GetPinky().bones[2].NextJoint.ToVector3();
        return (thumbDistalPos - pinkyDistalPos).magnitude;
    }

    private float SnailMetric(Hand hand)
    {
        Finger index = hand.GetIndex();
        
        //float metacarpalProximalAngle = Vector3.SignedAngle(index.bones[0].Direction.ToVector3(), index.bones[1].Direction.ToVector3(), hand.RadialAxis());

        List<float> fingerAngleWeights = new List<float>(){3, 1, 1};
        float sumFingerAngleWeights = SumOfList(fingerAngleWeights);

        List<float> fingerWeights = new List<float>(){0.5f, 1, 1, 1};
        float sumFingerWeights = SumOfList(fingerWeights);

        List<float> thumbAngleWeights = new List<float>(){1, 1, 1};
        float sumThumbAngleWeights = SumOfList(thumbAngleWeights);

        float thumbWeight = 1;

        float wIndexAngles = WeightedFingerAngles(hand.GetIndex(), fingerAngleWeights, 1f);
        float wMiddleAngles = WeightedFingerAngles(hand.GetMiddle(), fingerAngleWeights, 1f);
        float wRingAngles = WeightedFingerAngles(hand.GetRing(), fingerAngleWeights, 1f);
        float wPinkyAngles = WeightedFingerAngles(hand.GetPinky(), fingerAngleWeights, 1f);
        float wThumbAngles = WeightedFingerAngles(hand.GetThumb(), thumbAngleWeights, -1f);

        float angleSum = fingerWeights[0] * wIndexAngles + fingerWeights[1] * wMiddleAngles + fingerWeights[2] * wRingAngles + fingerWeights[3] * wPinkyAngles + thumbWeight * wThumbAngles;
        float sumOfWeights = sumFingerAngleWeights * sumFingerWeights + thumbWeight * sumThumbAngleWeights;

        float metric = angleSum / sumOfWeights;
        return metric;
    }

    private float WeightedFingerAngles(Finger finger, List<float> angleWeights, float chirality)
    {
        List<Vector3> joints = new List<Vector3>(){finger.bones[0].PrevJoint.ToVector3()};

        for(int i=0; i<=3; i++)
        {
            joints.Add(finger.bones[i].NextJoint.ToVector3());
        }

        Vector3 rotationPlaneNormal = chirality * Vector3.Cross(finger.bones[0].Direction.ToVector3(), finger.bones[1].Direction.ToVector3()).normalized;

        List<Vector3> projectedJoints = new List<Vector3>();
        for(int i=0; i<=4; i++)
        {
            Vector3 projectedJoint = Vector3.ProjectOnPlane(joints[i], rotationPlaneNormal);
            projectedJoints.Add(projectedJoint);
        }

        List<float> angles = new List<float>();
        for(int i=0; i<=2; i++)
        {
            Vector3 firstBone = projectedJoints[i+1] - projectedJoints[i];
            Vector3 secondBone = projectedJoints[i+2] - projectedJoints[i+1];
            float angleDeg = Vector3.SignedAngle(firstBone, secondBone, rotationPlaneNormal);
            float angleRad = angleDeg * Mathf.Deg2Rad;
            angles.Add(angleRad);
        }

        if(angleWeights.Count != angles.Count)
        {
            throw new System.Exception();
        }

        float weightedSum = 0f;

        for(int i=0; i < angles.Count; i++)
        {
            weightedSum += angles[i] * angleWeights[i];
        }

        return weightedSum;
    }

    private static float SumOfList(List<float> vals)
    {
        float sum = 0f;
        for(int i=0; i<vals.Count; i++)
        {
            sum += vals[i];
        }
        return sum;
    }

    private float CalculateTriggerStiffness(Hand hand)
    {
        float calculatedStiffness = 0f;
        switch (physicsMetric)
        {
            case MetricType.SNAIL:
                calculatedStiffness = SnailTriggerStiffness(hand);
                break;
            case MetricType.SWAN:
                calculatedStiffness = SwanTriggerStiffness(hand);
                break;
            default:
                break;
        }
        return calculatedStiffness;
    }

    private float SnailTriggerStiffness(Hand hand)
    {
        Vector3 planeNormal = hand.RadialAxis();
        Vector3 originPos = hand.WristPosition.ToVector3();

        Vector3 thumbPos = Vector3.ProjectOnPlane(hand.GetThumb().TipPosition.ToVector3() - originPos, planeNormal);
        Vector3 indexPos = Vector3.ProjectOnPlane(hand.GetIndex().TipPosition.ToVector3() - originPos, planeNormal);
        Vector3 middlePos = Vector3.ProjectOnPlane(hand.GetMiddle().TipPosition.ToVector3() - originPos, planeNormal);
        Vector3 ringPos = Vector3.ProjectOnPlane(hand.GetRing().TipPosition.ToVector3() - originPos, planeNormal);
        Vector3 pinkyPos = Vector3.ProjectOnPlane(hand.GetPinky().TipPosition.ToVector3() - originPos, planeNormal);

        float indexAngle = Vector3.SignedAngle(thumbPos, indexPos, planeNormal);
        float middleAngle = Vector3.SignedAngle(thumbPos, middlePos, planeNormal);
        float ringAngle = Vector3.SignedAngle(thumbPos, ringPos, planeNormal);
        float pinkyAngle = Vector3.SignedAngle(thumbPos, pinkyPos, planeNormal);

        float avgAngle = 0.25f * (indexAngle + middleAngle + ringAngle + pinkyAngle) * Mathf.Deg2Rad;

        float suggestedStiffness = 0f;
        if (avgAngle > 0)
        {
            suggestedStiffness = 1.0f / avgAngle;
        }
        else
        {
            suggestedStiffness = maxStiffness;
        }

        float stiffness = Mathf.Clamp(suggestedStiffness, 0f, maxStiffness);
        return stiffness;
    }

    private float SwanTriggerStiffness(Hand hand)
    {
        float currentSwanNegativeDisance = SwanMetric(hand);

        float typicalTriggerValue = -0.01f;
        float distanceToZero = typicalTriggerValue - currentSwanNegativeDisance;

        float stiffness = 0;
        if (distanceToZero < 0f)
        {
            stiffness = maxStiffness;
        }
        else
        {
            float fudge_factor = 2f;
            stiffness = fudge_factor * 1.0f / distanceToZero;
        }
        return stiffness;
    }

    private float SwanMetric(Hand hand)
    {
        // The SwanMetric is an arbitrarily scaled metric - usually negative.
        // It increases towards 0 at a normal 
        return -1f * DuckPinch.DuckPinchDistance(hand);
    }
    
}

using UnityEngine;
using Leap;
using Leap.Unity;
using System.Collections;

[DefaultExecutionOrder(-1)]
public class SingleHandManager : MonoBehaviour
{
    public static SingleHandManager Instance;

    public Hand CurrentHand { get; private set; }

    bool CurrentIsLeft => CurrentHand != null && CurrentHand.IsLeft;
    bool CurrentIsRight => CurrentHand != null && !CurrentHand.IsLeft;

    [HideInInspector] public bool useTrackingTransform = true;
    LeapTransform TrackingTransform;

    void Awake()
    {
        if(Instance != null)
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
        while(((LeapServiceProvider)Hands.Provider).GetLeapController() == null)
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
        bool foundLeft = false;
        bool foundRight = false;

        Hand left = null;
        Hand right = null;

       if (useTrackingTransform)
            Hands.Provider.CurrentFrame.Transform(TrackingTransform);

        foreach (var hand in Hands.Provider.CurrentFrame.Hands)
        {
            if (hand.IsLeft)
            {
                if (CurrentIsLeft) // left hand is already active and was found, ignore everything
                    return;

                foundLeft = true;
                left = hand;
            }

            if (hand.IsRight)
            {
                if (CurrentIsRight) // right hand is already active and was found, ignore everything
                    return;

                foundRight = true;
                right = hand;
            }
        }

        // if we are here, we might need to set a new hand to be active

        if (foundRight) // prioritise right hand as it is standard.
        {
            // Set it to be active
            CurrentHand = right;
        }
        else if(foundLeft)
        {
            // Set it to be active
            CurrentHand = left;
        }
        else
        {
            CurrentHand = null;
        }
    }

    public Vector3 GetTrackedPointingJoint()
    {
        const float trackedJointDistanceOffset = 0.0533f;

        var bones = CurrentHand.GetIndex().bones;

        Vector3 trackedJointVector = (bones[0].NextJoint.ToVector3() + bones[1].NextJoint.ToVector3()) / 2;
        trackedJointVector.z += trackedJointDistanceOffset;
        return trackedJointVector;
    }

    public long GetTimestamp()
    {
        // Returns the timestamp of the latest frame in microseconds
        Controller leapController = ((LeapServiceProvider)Hands.Provider).GetLeapController();
        return leapController.Frame(0).Timestamp;
    }

    public bool IsLeapServiceConnected()
    {
        return ((LeapServiceProvider)Hands.Provider).IsConnected();
    }
}
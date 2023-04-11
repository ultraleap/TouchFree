using Leap;
using System.Linq;
using System.Numerics;
using Ultraleap.TouchFree.Library.Configuration;

namespace Ultraleap.TouchFree.Library.Interactions.PositionTrackers;

public class ProjectionTracker : IPositionTracker
{
    private Vector3 _projectionOrigin;

    public ProjectionTracker(IConfigManager configManager)
    {
        OnPhysicalConfigUpdated(configManager.PhysicalConfig);
        configManager.OnPhysicalConfigUpdated += OnPhysicalConfigUpdated;
    }

    public TrackedPosition TrackedPosition => TrackedPosition.HAND_PROJECTION;

    public Vector3 GetTrackedPosition(Hand hand)
    {
        var fingerTip = hand.Fingers.FirstOrDefault(finger => (finger.Type == Finger.FingerType.TYPE_INDEX)).TipPosition;
        var trackedFingerTip = Utilities.LeapVectorToNumerics(fingerTip);

        var projectionVector = _projectionOrigin - trackedFingerTip;
        var projectionVectorRatio = (trackedFingerTip.Z / projectionVector.Z);
        var impactLocation = trackedFingerTip - projectionVectorRatio * projectionVector;

        return impactLocation with { Z = 1.2f - projectionVector.Length() };
    }

    private void OnPhysicalConfigUpdated(PhysicalConfigInternal config = null)
    {
        _projectionOrigin = new Vector3(0, config.ScreenHeightMm * 2 / 3000, 0.6f);
    }

}
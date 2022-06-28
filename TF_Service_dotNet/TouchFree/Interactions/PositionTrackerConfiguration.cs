namespace Ultraleap.TouchFree.Library.Interactions
{
    public struct PositionTrackerConfiguration
    {
        public readonly TrackedPosition trackedPosition;
        public readonly int weighting;

        public PositionTrackerConfiguration(TrackedPosition _trackedPosition, int _weighting)
        {
            trackedPosition = _trackedPosition;
            weighting = _weighting;
        }
    }
}

namespace Ultraleap.TouchFree.Library.Interactions
{
    public interface IPositioningModule
    {
        TrackedPosition TrackedPosition { set; }
        Positions CalculatePositions(Leap.Hand hand);
        IPositionStabiliser Stabiliser { get; }
    }
}

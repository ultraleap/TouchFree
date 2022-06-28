using System.Collections.Generic;

namespace Ultraleap.TouchFree.Library.Interactions
{
    public interface IPositioningModule
    {
        Positions CalculatePositions(Leap.Hand hand, IEnumerable<PositionTrackerConfiguration> configuration);
        Positions ApplyStabiliation(Positions positions, IPositionStabiliser stabiliser);
    }
}

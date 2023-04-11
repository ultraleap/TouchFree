using System.Collections.Generic;

namespace Ultraleap.TouchFree.Library.Interactions;

public interface IPositioningModule
{
    Positions CalculatePositions(Leap.Hand hand, IEnumerable<PositionTrackerConfiguration> configuration);
    Positions ApplyStabilisation(Positions positions, IPositionStabiliser stabiliser);
}
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Ultraleap.TouchFree.Library.Interactions.PositionTrackers;

namespace Ultraleap.TouchFree.Library.Interactions;

public class PositioningModule : IPositioningModule
{
    private Positions _positions;

    private readonly IVirtualScreen _virtualScreen;

    private readonly IEnumerable<IPositionTracker> _positionTrackers;

    public PositioningModule(IVirtualScreen virtualScreen, IEnumerable<IPositionTracker> positionTrackers)
    {
        _virtualScreen = virtualScreen;
        _positionTrackers = positionTrackers;
    }

    public Positions CalculatePositions(Leap.Hand hand, IEnumerable<PositionTrackerConfiguration> configuration)
    {
        if (hand == null)
        {
            return _positions;
        }

        var trackerConfigurations = configuration as PositionTrackerConfiguration[] ?? configuration.ToArray();
        int totalWeights = trackerConfigurations.Sum(x => x.Weighting);
        Vector3 worldPosM = new Vector3();

        foreach (var positionItem in trackerConfigurations)
        {
            worldPosM += GetPositionFromTracker(positionItem.TrackedPosition, hand) * positionItem.Weighting / totalWeights;
        }

        Vector3 screenPos = _virtualScreen.WorldPositionToVirtualScreen(worldPosM);

        _positions = new Positions(new Vector2(screenPos.X, screenPos.Y), screenPos.Z);

        return _positions;
    }

    public Positions ApplyStabilisation(Positions positions, IPositionStabiliser stabiliser)
    {
        Vector2 screenPosMm = _virtualScreen.PixelsToMillimeters(positions.CursorPosition);
        screenPosMm = stabiliser.ApplyDeadzone(screenPosMm);
        return positions with { CursorPosition = _virtualScreen.MillimetersToPixels(screenPosMm) };
    }

    public Vector3 GetPositionFromTracker(TrackedPosition trackedPosition, Leap.Hand hand)
    {
        var trackerToUse = _positionTrackers.Single(x => x.TrackedPosition == trackedPosition);
        return trackerToUse.GetTrackedPosition(hand);
    }
}
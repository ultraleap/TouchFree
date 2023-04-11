using Microsoft.Extensions.Options;
using System.Numerics;

namespace Ultraleap.TouchFree.Library.Interactions;

public class ExtrapolationPositionModifier : IPositionModifier
{
    public ExtrapolationPositionModifier(IOptions<InteractionTuning> interactionTuning)
    {
        _enabled = interactionTuning?.Value?.EnableExtrapolation ?? false;
    }

    private readonly bool _enabled;
    private Vector2? _lastPosition;

    public Vector2 ApplyModification(Vector2 position)
    {
        if (!_enabled)
        {
            return position;
        }

        var extrapolatedPosition = new Vector2(position.X, position.Y);

        if (_lastPosition != null)
        {
            var changeInPosition = position - _lastPosition.Value;
            extrapolatedPosition = position + (changeInPosition * 2);
        }

        _lastPosition = position;

        return extrapolatedPosition;
    }
}
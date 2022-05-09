using System.Numerics;
using Microsoft.Extensions.Options;

namespace Ultraleap.TouchFree.Library.Interactions
{
    public class ExtrapolationPositionModifier : IPositionModifier
    {
        public ExtrapolationPositionModifier(IOptions<InteractionTuning> _interactionTuning)
        {
            enabled = _interactionTuning?.Value?.EnableExtrapolation ?? false;
        }

        private readonly bool enabled;
        private Vector2? lastPosition;

        public Vector2 ApplyModification(Vector2 position)
        {
            if (!enabled)
            {
                return position;
            }

            var extrapolatedPosition = new Vector2(position.X, position.Y);

            if (lastPosition != null)
            {
                var changeInPosition = position - lastPosition.Value;
                extrapolatedPosition = position + changeInPosition;
            }

            lastPosition = position;

            return extrapolatedPosition;
        }
    }
}
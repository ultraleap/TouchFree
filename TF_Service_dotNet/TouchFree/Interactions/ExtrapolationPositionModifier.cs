using System.Numerics;

namespace Ultraleap.TouchFree.Library.Interactions
{
    public class ExtrapolationPositionModifier : IPositionModifier
    {
        public ExtrapolationPositionModifier(InteractionTuning _interactionTuning)
        {
            enabled = _interactionTuning?.EnableExtrapolation ?? false;
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
                extrapolatedPosition = position + (changeInPosition * 2);
            }

            lastPosition = position;

            return extrapolatedPosition;
        }
    }
}
using System.Numerics;

namespace Ultraleap.TouchFree.Library.Interactions
{
    public class ExtrapolationPositionModifier : IPositionModifier
    {
        private Vector2? lastPosition;

        public Vector2 ApplyModification(Vector2 position)
        {
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
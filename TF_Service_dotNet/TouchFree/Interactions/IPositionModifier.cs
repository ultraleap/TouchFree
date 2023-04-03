using System.Numerics;

namespace Ultraleap.TouchFree.Library.Interactions;

public interface IPositionModifier
{
    Vector2 ApplyModification(Vector2 position);
}

public static class PositionModifierExtensions
{
    public static Positions ApplyModifier(this in Positions positions, IPositionModifier modifier) =>
        positions with { CursorPosition = modifier.ApplyModification(positions.CursorPosition) };
}
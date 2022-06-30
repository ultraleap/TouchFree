using System.Numerics;

namespace Ultraleap.TouchFree.Library.Interactions
{
    public interface IPositionModifier
    {
        Vector2 ApplyModification(Vector2 position);
    }
}

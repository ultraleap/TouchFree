using System.Numerics;

namespace Ultraleap.TouchFree.Library
{
    public interface IVirtualScreen
    {
        Vector3 VirtualScreenPositionToWorld(Vector2 screenPos, float distanceFromVirtualScreen);
        Vector3 WorldPositionToVirtualScreen(Vector3 worldPosition);
        float DistanceFromScreenPlane(Vector3 worldPosition);
        Vector2 MetersToPixels(Vector2 position);
        Vector2 PixelsToMeters(Vector2 position);
        float MetersToPixels(float distanceM);
    }
}

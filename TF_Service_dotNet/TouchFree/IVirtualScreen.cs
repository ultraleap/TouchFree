using System.Numerics;

namespace Ultraleap.TouchFree.Library;

public interface IVirtualScreen
{
    Vector3 VirtualScreenPositionToWorld(Vector2 screenPos, float distanceFromVirtualScreen);
    Vector3 WorldPositionToVirtualScreen(Vector3 worldPosition);
    Vector2 MillimetersToPixels(Vector2 position);
    Vector2 PixelsToMillimeters(Vector2 position);
    float MillimetersToPixels(float distanceM);
}
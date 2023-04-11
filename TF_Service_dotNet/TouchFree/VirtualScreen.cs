using System.Numerics;
using Ultraleap.TouchFree.Library.Configuration;

namespace Ultraleap.TouchFree.Library;

public class VirtualScreen : IVirtualScreen
{
    public float Width_VirtualPx { get; private set; }
    public float Height_VirtualPx { get; private set; }
    public float Width_PhysicalMillimeters { get; private set; }
    public float Height_PhysicalMillimeters { get; private set; }

    public float MillimetersToPixelsConversion { get; private set; }

    public VirtualScreen(IConfigManager configManager)
    {
        PhysicalConfigUpdated(configManager.PhysicalConfig);
        configManager.OnPhysicalConfigUpdated += PhysicalConfigUpdated;
    }

    private void PhysicalConfigUpdated(PhysicalConfigInternal config)
    {
        Width_VirtualPx = config.ScreenWidthPX;
        Height_VirtualPx = config.ScreenHeightPX;

        Height_PhysicalMillimeters = config.ScreenHeightMm;
        // Calc screen physical width from the physical height and resolution ratio.
        // May not be correct if screen resolution doesn't fill entire physical screen (e.g. 16:9 resolution on a physical 16:10 screen).
        var aspectRatio = config.ScreenHeightPX <= 0 ? 0 : (float)config.ScreenWidthPX / (float)config.ScreenHeightPX;
        Width_PhysicalMillimeters = config.ScreenHeightMm * aspectRatio;

        MillimetersToPixelsConversion = Height_VirtualPx / Height_PhysicalMillimeters;
    }

    /// <summary>
    /// Return value is a screen position whose origin (0,0) is in bottom left corner.
    /// X-axis is positive right.
    /// Y-axis is positive up.
    /// Z-axis is positive in direction of screen's normal.
    /// This is the distance from the virtual screen where positive is further away from it and negative is when through or inside the virtual screen.
    /// </summary>
    /// <param name="worldPosition">Input world position to convert to a screen point + distance.</param>
    /// <returns>Vector3 whose X and Y are screen pixels, and Z is the physical distance from the virtual touch plane in meters.</returns>
    public Vector3 WorldPositionToVirtualScreen(Vector3 worldPosition)
    {
        Vector3 screenPos = Vector3.Zero;

        // World X = 0 is middle of screen, so shift everything over by half width (w/2).
        screenPos.X = MillimetersToPixels(worldPosition.X * 1000 + (Width_PhysicalMillimeters / 2.0f));
        // World Y = 0 is bottom of the screen, so this is linear.
        screenPos.Y = MillimetersToPixels(worldPosition.Y * 1000);

        screenPos.Z = worldPosition.Z;

        return screenPos;
    }

    public Vector3 VirtualScreenPositionToWorld(Vector2 screenPos, float distanceFromVirtualScreen)
    {
        Vector3 worldPos = Vector3.Zero;

        worldPos.X = PixelsToMillimeters(screenPos.X) - (Width_PhysicalMillimeters / 2.0f);
        worldPos.Y = PixelsToMillimeters(screenPos.Y);
        worldPos.Z = distanceFromVirtualScreen;

        return worldPos;
    }

    // Perform a unit conversion from pixels to meters
    //
    // Do not rotate or offset the axes
    //
    // This does not give the "worldPosition", but can be used to calculate distances in pixels
    // instead of metres.
    public Vector2 PixelsToMillimeters(Vector2 positionPx) =>
        MillimetersToPixelsConversion <= 0
            ? new Vector2(0, 0)
            : positionPx / MillimetersToPixelsConversion;

    // Perform a unit conversion from pixels to meters
    //
    // Do not rotate or offset the axes
    //
    // This does not give the "worldPosition", but can be used to calculate distances in metres
    // instead of pixels.
    public float PixelsToMillimeters(float positionPx) =>
        MillimetersToPixelsConversion <= 0
            ? 0
            : positionPx / MillimetersToPixelsConversion;

    // Perform a unit conversion from meters to pixels
    //
    // Do not rotate or offset the axes
    //
    // This does not give the "worldPosition", but can be used to calculate distances in metres
    // instead of pixels.
    public Vector2 MillimetersToPixels(Vector2 positionM) => positionM * MillimetersToPixelsConversion;

    // Perform a unit conversion from meters to pixels
    //
    // Do not rotate or offset the axes
    //
    // This does not give the "worldPosition", but can be used to calculate distances in metres
    // instead of pixels.
    public float MillimetersToPixels(float distanceM) => distanceM * MillimetersToPixelsConversion;
}
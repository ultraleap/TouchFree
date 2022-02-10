using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Ultraleap.TouchFree.Library.Configuration;

namespace Ultraleap.TouchFree.Library
{
    public class VirtualScreen : IVirtualScreen
    {
        public float Width_VirtualPx { get; private set; }
        public float Height_VirtualPx { get; private set; }
        public float Width_PhysicalMillimeters { get; private set; }
        public float Height_PhysicalMillimeters { get; private set; }

        public float MillimetersToPixelsConversion { get; private set; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="configManager"></param>
        public VirtualScreen(IConfigManager configManager)
        {
            PhysicalConfigUpdated(configManager.PhysicalConfig);
            configManager.OnPhysicalConfigUpdated += PhysicalConfigUpdated;
        }

        void PhysicalConfigUpdated(PhysicalConfig _config)
        {
            Width_VirtualPx = _config.ScreenWidthPX;
            Height_VirtualPx = _config.ScreenHeightPX;

            Height_PhysicalMillimeters = _config.ScreenHeightMm;
            // Calc screen physical width from the physical height and resolution ratio.
            // May not be correct if screen resolution doesn't fill entire physical screen (e.g. 16:9 resolution on a physical 16:10 screen).
            var aspectRatio = (float)_config.ScreenWidthPX / (float)_config.ScreenHeightPX;
            Width_PhysicalMillimeters = _config.ScreenHeightMm * aspectRatio;

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
            screenPos.X = (worldPosition.X + (Width_PhysicalMillimeters / 2.0f)) * MillimetersToPixelsConversion;
            // World Y = 0 is bottom of the screen, so this is linear.
            screenPos.Y = worldPosition.Y * MillimetersToPixelsConversion;

            screenPos.Z = worldPosition.Z;

            return screenPos;
        }

        public Vector3 VirtualScreenPositionToWorld(Vector2 screenPos, float distanceFromVirtualScreen)
        {
            Vector3 worldPos = Vector3.Zero;

            worldPos.X = (screenPos.X / MillimetersToPixelsConversion) - (Width_PhysicalMillimeters / 2.0f);
            worldPos.Y = (screenPos.Y / MillimetersToPixelsConversion);
            worldPos.Z = distanceFromVirtualScreen;

            return worldPos;
        }

        // Perform a unit conversion from pixels to meters
        //
        // Do not rotate or offset the axes
        //
        // This does not give the "worldPosition", but can be used to calculate distances in pixels
        // instead of metres.
        public Vector2 PixelsToMillimeters(Vector2 positionPx)
        {
            return positionPx / MillimetersToPixelsConversion;
        }

        // Perform a unit conversion from meters to pixels
        //
        // Do not rotate or offset the axes
        //
        // This does not give the "worldPosition", but can be used to calculate distances in metres
        // instead of pixels.
        public Vector2 MillimetersToPixels(Vector2 positionM)
        {
            return positionM * MillimetersToPixelsConversion;
        }

        // Perform a unit conversion from meters to pixels
        //
        // Do not rotate or offset the axes
        //
        // This does not give the "worldPosition", but can be used to calculate distances in metres
        // instead of pixels.
        public float MillimetersToPixels(float distanceM)
        {
            return distanceM * MillimetersToPixelsConversion;
        }

        [DllImport("User32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern int GetSystemMetrics(int nIndex);

        public static int GetActualScreenWidth()
        {
            return GetSystemMetrics(0);
        }
        public static int GetActualScreenHeight()
        {
            return GetSystemMetrics(1);
        }
    }
}
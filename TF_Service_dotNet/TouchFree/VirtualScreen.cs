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
        public float Width_PhysicalMeters { get; private set; }
        public float Height_PhysicalMeters { get; private set; }

        public float MetersToPixelsConversion { get; private set; }

        /// <summary>
        /// The angle or tilt of the physical screen in degrees, where 0 would be a vertical screen facing the user, and 90 would be a flat screen facing the ceiling.
        /// </summary>
        public float AngleOfPhysicalScreen_Degrees { get; private set; }

        public Plane ScreenPlane { get; private set; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="widthPx"></param>
        /// <param name="heightPx"></param>
        /// <param name="heightPhysicalMeters"></param>
        /// <param name="physicalScreenAngleDegrees">The angle or tilt of the physical screen in degrees, where 0 would be a vertical screen facing the user, and 90 would be a flat screen facing the ceiling.</param>
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

            Height_PhysicalMeters = _config.ScreenHeightM;
            // Calc screen physical width from the physical height and resolution ratio.
            // May not be correct if screen resolution doesn't fill entire physical screen (e.g. 16:9 resolution on a physical 16:10 screen).
            var aspectRatio = (float)_config.ScreenWidthPX / (float)_config.ScreenHeightPX;
            Width_PhysicalMeters = _config.ScreenHeightM * aspectRatio;

            MetersToPixelsConversion = Height_VirtualPx / Height_PhysicalMeters;

            AngleOfPhysicalScreen_Degrees = _config.ScreenRotationD;
            var planeNormal = new Vector3(0f, (float)Math.Sin(DegreesToRadians(AngleOfPhysicalScreen_Degrees)), (float)Math.Cos(DegreesToRadians(AngleOfPhysicalScreen_Degrees)));
            ScreenPlane = new Plane(planeNormal, 0f);
        }

        public float DistanceFromScreenPlane(Vector3 worldPosition)
        {
            return Vector3.Dot(worldPosition, ScreenPlane.Normal);
        }

        /// <summary>
        /// Return value is a screen position whose origin (0,0) is in bottom left corner.
        /// X-axis is positive right.
        /// Y-axis is positive up.
        /// Z-axis is positive in direction of screen's normal.
        /// This is the distance from the virtual screen where positive is further away from it and negative is when through or inside the virtual screen.
        /// </summary>
        /// <param name="worldPosition">Input world position to convert to a screen point + distance.</param>
        /// <param name="planeHitWorldPosition">Out param to provide computed world space plane hit point.</param>
        /// <param name="isRaycastParallelToScreenNormal">Whether the raycast direction is parallel to the screen normal, or parallel to the ground (straight forwards).</param>
        /// <returns>Vector3 whose X and Y are screen pixels, and Z is the physical distance from the virtual touch plane in meters.</returns>
        public Vector3 WorldPositionToVirtualScreen(Vector3 worldPosition, out Vector3 planeHitWorldPosition)
        {
            var distanceFromPlane = DistanceFromScreenPlane(worldPosition);
            planeHitWorldPosition = worldPosition - (ScreenPlane.Normal * distanceFromPlane);

            // If the screen is rotated, this effectively scales down or "projects" the rotated screen vector back onto the UP axis, giving us the new UP axis height of the screen.
            float screenHeight = Height_PhysicalMeters * (float)Math.Cos(DegreesToRadians(AngleOfPhysicalScreen_Degrees));

            Vector3 screenPos = Vector3.Zero;
            float tX = ((Width_PhysicalMeters / 2.0f) + planeHitWorldPosition.X) / Width_PhysicalMeters; // World X = 0 is middle of screen, so shift everything over by half width (w/2).
            float tY = planeHitWorldPosition.Y / screenHeight; // World Y = 0 is bottom of the screen, so this is linear.

            screenPos.X = Width_VirtualPx * tX;
            screenPos.Y = Height_VirtualPx * tY;
            screenPos.Z = distanceFromPlane;

            return screenPos;
        }

        public Vector3 VirtualScreenPositionToWorld(Vector2 screenPos, float distanceFromVirtualScreen)
        {
            Vector3 worldPos = Vector3.Zero;

            float tX = screenPos.X / Width_VirtualPx;
            float tY = screenPos.Y / Height_VirtualPx;

            // If the screen is rotated, this effectively scales down or "projects" the rotated screen vector back onto the UP axis, giving us the new UP axis height of the screen.
            float screenHeight = Height_PhysicalMeters * (float)Math.Cos(DegreesToRadians(AngleOfPhysicalScreen_Degrees));

            worldPos.X = (Width_PhysicalMeters * tX) - (Width_PhysicalMeters / 2.0f);
            worldPos.Y = screenHeight * tY;
            worldPos.Z = distanceFromVirtualScreen;

            return worldPos;
        }

        // Perform a unit conversion from pixels to meters
        //
        // Do not rotate or offset the axes
        //
        // This does not give the "worldPosition", but can be used to calculate distances in pixels
        // instead of metres.
        public Vector2 PixelsToMeters(Vector2 positionPx)
        {
            return positionPx / MetersToPixelsConversion;
        }

        // Perform a unit conversion from meters to pixels
        //
        // Do not rotate or offset the axes
        //
        // This does not give the "worldPosition", but can be used to calculate distances in metres
        // instead of pixels.
        public Vector2 MetersToPixels(Vector2 positionM)
        {
            return positionM * MetersToPixelsConversion;
        }

        // Perform a unit conversion from meters to pixels
        //
        // Do not rotate or offset the axes
        //
        // This does not give the "worldPosition", but can be used to calculate distances in metres
        // instead of pixels.
        public float MetersToPixels(float distanceM)
        {
            return distanceM * MetersToPixelsConversion;
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

        public static float DegreesToRadians(float angle)
        {
            return ((float)Math.PI / 180) * angle;
        }
    }

    public struct Ray
    {
        Vector3 m_Origin;
        Vector3 m_Direction;

        // Creates a ray starting at /origin/ along /direction/.
        public Ray(Vector3 origin, Vector3 direction)
        {
            m_Origin = origin;
            m_Direction = Vector3.Normalize(direction);
        }

        // The origin point of the ray.
        public Vector3 origin
        {
            get { return m_Origin; }
            set { m_Origin = value; }
        }

        // The direction of the ray.
        public Vector3 direction
        {
            get { return m_Direction; }
            set { m_Direction = Vector3.Normalize(value); }
        }
    }
}
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Ultraleap.TouchFree.Library.Configuration;

namespace Ultraleap.TouchFree.Library
{
    public class VirtualScreen
    {
        public float Width_VirtualPx { get; private set; }
        public float Height_VirtualPx { get; private set; }
        public float Width_PhysicalMeters { get; private set; }
        public float Height_PhysicalMeters { get; private set; }
        /// <summary>
        /// The angle or tilt of the physical screen in degrees, where 0 would be a vertical screen facing the user, and 90 would be a flat screen facing the ceiling.
        /// </summary>
        public float AngleOfPhysicalScreen_Degrees { get; private set; }

        public Plane PhysicalScreenPlane { get; private set; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="widthPx"></param>
        /// <param name="heightPx"></param>
        /// <param name="heightPhysicalMeters"></param>
        /// <param name="distanceFromPhysicalMeters"></param>
        /// <param name="physicalScreenAngleDegrees">The angle or tilt of the physical screen in degrees, where 0 would be a vertical screen facing the user, and 90 would be a flat screen facing the ceiling.</param>
        /// <param name="trackingOffset"></param>
        public VirtualScreen(int widthPx, int heightPx, float heightPhysicalMeters, float physicalScreenAngleDegrees)
        {
            Width_VirtualPx = widthPx;
            Height_VirtualPx = heightPx;

            Height_PhysicalMeters = heightPhysicalMeters;
            // Calc screen physical width from the physical height and resolution ratio.
            // May not be correct if screen resolution doesn't fill entire physical screen (e.g. 16:9 resolution on a physical 16:10 screen).
            var aspectRatio = (float)widthPx / (float)heightPx;
            Width_PhysicalMeters = heightPhysicalMeters * aspectRatio;

            AngleOfPhysicalScreen_Degrees = physicalScreenAngleDegrees;
            var planeNormal = new Vector3(0f, (float)Math.Sin(DegreesToRadians(AngleOfPhysicalScreen_Degrees)), -(float)Math.Cos(DegreesToRadians(AngleOfPhysicalScreen_Degrees)));
            PhysicalScreenPlane = new Plane(-planeNormal, 0f);
        }

        public float DistanceFromScreenPlane(Vector3 worldPosition)
        {
            var rayDir = PhysicalScreenPlane.Normal;
            Ray r = new Ray(worldPosition, rayDir);

            RaycastAgainstPlane(PhysicalScreenPlane, r, out float distanceFromPlane);

            return distanceFromPlane;
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
            var rayDir = PhysicalScreenPlane.Normal;

            // Cast a ray towards the physical screen to get a point mapped onto that physical screen.
            Ray r = new Ray(worldPosition, rayDir);
            RaycastAgainstPlane(PhysicalScreenPlane, r, out float distanceFromPlane);
            planeHitWorldPosition = r.origin + (r.direction * distanceFromPlane);

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
        public Vector2 PixelsToMeters(Vector2 position)
        {
            Vector2 positionInMeters = new Vector2(
                position.X * Width_PhysicalMeters / Width_VirtualPx,
                position.Y * Height_PhysicalMeters / Height_VirtualPx);
            return positionInMeters;
        }

        // Perform a unit conversion from meters to pixels
        //
        // Do not rotate or offset the axes
        //
        // This does not give the "worldPosition", but can be used to calculate distances in metres
        // instead of pixels.
        public Vector2 MetersToPixels(Vector2 position)
        {
            Vector2 positionInPixels = new Vector2(
                position.X * Width_VirtualPx / Width_PhysicalMeters,
                position.Y * Height_VirtualPx / Height_PhysicalMeters);
            return positionInPixels;
        }

        public static void CaptureCurrentResolution()
        {
            ConfigManager.PhysicalConfig.ScreenWidthPX = GetActualScreenWidth();
            ConfigManager.PhysicalConfig.ScreenHeightPX = GetActualScreenHeight();

            ConfigManager.InteractionConfig.ConfigWasUpdated();
        }

        public static VirtualScreen virtualScreen
        {
            get
            {
                if (_virtualScreen == null)
                {
                    CreateVirtualScreen();
                }

                return _virtualScreen;
            }
        }
        static VirtualScreen _virtualScreen;

        static void CreateVirtualScreen()
        {
            _virtualScreen = new VirtualScreen(
                ConfigManager.PhysicalConfig.ScreenWidthPX,
                ConfigManager.PhysicalConfig.ScreenHeightPX,
                ConfigManager.PhysicalConfig.ScreenHeightM,
                ConfigManager.PhysicalConfig.ScreenRotationD);
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

        public bool RaycastAgainstPlane(Plane plane, Ray ray, out float enter)
        {
            float vdot = Vector3.Dot(ray.direction, plane.Normal);
            float ndot = -Vector3.Dot(ray.origin, plane.Normal) - plane.D;

            if (vdot < 0.1f && vdot > -0.1f)
            {
                enter = 0.0F;
                return false;
            }

            enter = ndot / vdot;

            return enter > 0.0F;
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
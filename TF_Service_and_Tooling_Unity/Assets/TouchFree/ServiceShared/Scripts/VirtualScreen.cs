using UnityEngine;

namespace Ultraleap.TouchFree.ServiceShared
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
            var planeNormal = new Vector3(0f, Mathf.Sin(AngleOfPhysicalScreen_Degrees * Mathf.Deg2Rad), -Mathf.Cos(AngleOfPhysicalScreen_Degrees * Mathf.Deg2Rad));
            PhysicalScreenPlane = new Plane(-planeNormal, 0f);
        }

        public float DistanceFromScreenPlane(Vector3 worldPosition)
        {
            var rayDir = PhysicalScreenPlane.normal;
            Ray r = new Ray(worldPosition, rayDir);

            PhysicalScreenPlane.Raycast(r, out float distanceFromPlane);

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
            var rayDir = PhysicalScreenPlane.normal;

            // Cast a ray towards the physical screen to get a point mapped onto that physical screen.
            Ray r = new Ray(worldPosition, rayDir);
            //Debug.DrawRay(r.origin, r.direction * 0.02f, Color.green); // Direction of the ray pointing from the world position to intersect the screen plane.
            PhysicalScreenPlane.Raycast(r, out float distanceFromPlane);
            planeHitWorldPosition = r.origin + (r.direction * distanceFromPlane);

            // If the screen is rotated, this effectively scales down or "projects" the rotated screen vector back onto the UP axis, giving us the new UP axis height of the screen.
            var screenHeight = Height_PhysicalMeters * Mathf.Cos(AngleOfPhysicalScreen_Degrees * Mathf.Deg2Rad);

            var screenPos = Vector3.zero;
            var tX = ((Width_PhysicalMeters / 2.0f) + planeHitWorldPosition.x) / Width_PhysicalMeters; // World X = 0 is middle of screen, so shift everything over by half width (w/2).
            var tY = planeHitWorldPosition.y / screenHeight; // World Y = 0 is bottom of the screen, so this is linear.

            screenPos.x = Width_VirtualPx * tX;
            screenPos.y = Height_VirtualPx * tY;
            screenPos.z = distanceFromPlane;

            return screenPos;
        }

        public Vector3 VirtualScreenPositionToWorld(Vector2 screenPos, float distanceFromVirtualScreen)
        {
            Vector3 worldPos = Vector3.zero;

            var tX = screenPos.x / Width_VirtualPx;
            var tY = screenPos.y / Height_VirtualPx;

            // If the screen is rotated, this effectively scales down or "projects" the rotated screen vector back onto the UP axis, giving us the new UP axis height of the screen.
            var screenHeight = Height_PhysicalMeters * Mathf.Cos(AngleOfPhysicalScreen_Degrees * Mathf.Deg2Rad);

            worldPos.x = (Width_PhysicalMeters * tX) - (Width_PhysicalMeters / 2.0f);
            worldPos.y = screenHeight * tY;
            worldPos.z = distanceFromVirtualScreen;

            return worldPos;
        }

        // Perform a unit conversion from pixels to meters
        //
        // Do not rotate or offset the axes
        //
        // This does not give the "worldPosition", but can be used to calculate distances in metres
        // instead of pixels.
        public Vector2 PixelsToMeters(Vector2 position)
        {
            Vector2 positionInMeters = new Vector2(
                position.x * Width_PhysicalMeters / Width_VirtualPx,
                position.y * Height_PhysicalMeters / Height_VirtualPx);
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
                position.x * Width_VirtualPx / Width_PhysicalMeters,
                position.y * Height_VirtualPx / Height_PhysicalMeters);
            return positionInPixels;
        }

        public static void CaptureCurrentResolution()
        {
            ConfigManager.PhysicalConfig.ScreenWidthPX = Display.main.systemWidth;
            ConfigManager.PhysicalConfig.ScreenHeightPX = Display.main.systemHeight;

            ConfigManager.InteractionConfig.ConfigWasUpdated();
            ConfigManager.SaveAllConfigs();
        }
    }
}
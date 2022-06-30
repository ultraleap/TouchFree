using System;
using System.Linq;
using System.Numerics;

namespace Ultraleap.TouchFree.Library.Configuration
{
    public class QuickSetupHandler : IQuickSetupHandler
    {
        private readonly IHandManager handManager;
        private readonly ITrackingConnectionManager trackingConnectionManager;
        private readonly IConfigManager configManager;
        private Leap.Vector? topHandPosition;

        private const float TARGET_DIST_FROM_EDGE_PERCENTAGE = 0.2f;
        private const float HEIGHT_SCALING_FACTOR = 1f / (1f - (2 * TARGET_DIST_FROM_EDGE_PERCENTAGE));
        private const float EDGE_SCALING_FACTOR = ((HEIGHT_SCALING_FACTOR - 1f) / 2f) + 1f;

        public QuickSetupHandler(IHandManager _handManager, ITrackingConnectionManager _trackingConnectionManager, IConfigManager _configManager)
        {
            handManager = _handManager;
            trackingConnectionManager = _trackingConnectionManager;
            configManager = _configManager;
        }

        public void HandleQuickSetupCall(QuickSetupPosition position)
        {
            if (position == QuickSetupPosition.Top)
            {
                topHandPosition = handManager.RawHandPositions.FirstOrDefault();
            }
            else if (position == QuickSetupPosition.Bottom && topHandPosition != null)
            {
                Leap.Vector? bottomHandPosition = handManager.RawHandPositions.FirstOrDefault();

                if (bottomHandPosition != null)
                {
                    UpdateConfigurationValues(
                        Utilities.LeapVectorToNumerics(bottomHandPosition.Value) * 1000,
                        Utilities.LeapVectorToNumerics(topHandPosition.Value) * 1000);
                }
            }
        }

        public void UpdateConfigurationValues(Vector3 bottomPos, Vector3 topPos)
        {
            Vector3 bottomNoX = new(0, bottomPos.Y, bottomPos.Z);
            Vector3 topNoX = new(0, topPos.Y, topPos.Z);

            configManager.PhysicalConfig.ScreenHeightMm = Vector3.Distance(bottomNoX, topNoX) * HEIGHT_SCALING_FACTOR;

            var bottomEdge = BottomCentreFromTouches(bottomPos, topPos);

            configManager.PhysicalConfig.LeapRotationD = LeapRotationRelativeToScreen(bottomPos, topPos);
            configManager.PhysicalConfig.LeapPositionRelativeToScreenBottomMm = LeapPositionInScreenSpace(bottomEdge, configManager.PhysicalConfig.LeapRotationD);
            PhysicalConfigFile.SaveConfig(configManager.PhysicalConfig.ForApi());
            configManager.PhysicalConfigWasUpdated();
        }

        /// <summary>
        /// TopTouch -> BottomTouch is 1/8th screen height as touch points are placed 10% in from the edge.
        /// We need to offset the touch point by 1/10th of screen height = 1/8th of the distance between touch points.
        /// For this we can Lerp from top to bottom touch travelling an extra 8th distance
        /// </summary>
        public static Vector3 BottomCentreFromTouches(Vector3 bottomTouch, Vector3 topTouch)
        {
            var difference = topTouch - bottomTouch;
            return topTouch - (difference * EDGE_SCALING_FACTOR);
        }

        /// <summary>
        /// Find the angle between the camera and the screen.
        /// Ensure a positive angle always means rotation towards the screen.
        /// </summary>
        public Vector3 LeapRotationRelativeToScreen(Vector3 bottomCentre, Vector3 topCentre)
        {
            Vector3 directionBottomToTop = topCentre - bottomCentre;
            Vector3 rotation = Vector3.Zero;

            var xRotation = (float)(Utilities.RADTODEG * Math.Atan2(directionBottomToTop.Z, directionBottomToTop.Y));

            if (trackingConnectionManager.CurrentTrackingMode == TrackingMode.SCREENTOP ||
                trackingConnectionManager.CurrentTrackingMode == TrackingMode.HMD)
            {
                rotation.X = -xRotation + 180;
                rotation.Z = 180;
            }
            else
            {
                rotation.X = xRotation;
            }

            return rotation;
        }

        /// <summary>
        /// Find the position of the camera relative to the screen, using the screen position relative to the camera.
        /// </summary>
        public Vector3 LeapPositionInScreenSpace(Vector3 bottomEdgeRef, Vector3 leapRotation)
        {
            // In Leap Co-ords we know the Leap is at Vector3.zero, and that the bottom of the screen is at "bottomEdgeRef"

            // We know the Leap is rotated at "leapRotation" from the screen.
            // We want to calculate the Vector from the bottom of the screen to the Leap in this rotated co-ord system.

            Vector3 rotationAngles = leapRotation;
            if (trackingConnectionManager.CurrentTrackingMode == TrackingMode.SCREENTOP ||
                trackingConnectionManager.CurrentTrackingMode == TrackingMode.HMD)
            {
                // In overhead mode, the stored 'x' angle is inverted so that positive angles always mean
                // the camera is pointed towards the screen. Multiply by -1 here so that it can be used
                // in a calculation.
                rotationAngles.X *= -1f;
            }
            var quaternion = Quaternion.CreateFromYawPitchRoll(
                Utilities.DegreesToRadians(rotationAngles.Y),
                Utilities.DegreesToRadians(rotationAngles.X),
                Utilities.DegreesToRadians(rotationAngles.Z));
            Vector3 rotatedVector = Vector3.Transform(bottomEdgeRef, quaternion);

            return -rotatedVector;
        }
    }
}

using Ultraleap.TouchFree.Library.Configuration;
using System;
using Leap;
using System.Linq;
using System.Numerics;

namespace Ultraleap.TouchFree.Library
{
    public class HandManager
    {
        public long Timestamp { get; private set; }

        // The PrimaryHand is the hand that appeared first. It does not change until tracking on it is lost.
        public Hand PrimaryHand;
        public HandChirality primaryChirality;

        // The SecondaryHand is the second hand that appears. It may be promoted to the PrimaryHand if the
        // PrimaryHand is lost.
        public Hand SecondaryHand;
        public HandChirality secondaryChirality;

        public event Action HandFound;
        public event Action HandsLost;
        public delegate void HandUpdate(Hand primary, Hand secondary);
        public event HandUpdate HandsUpdated;

        private float PrimaryHandActivity = 1f;
        private float SecondaryHandActivity = 1f;

        private Vector3? lastPrimaryLocation;
        private Vector3? lastSecondaryLocation;

        bool PrimaryIsLeft => PrimaryHand != null && PrimaryHand.IsLeft;
        bool PrimaryIsRight => PrimaryHand != null && !PrimaryHand.IsLeft;
        bool SecondaryIsLeft => SecondaryHand != null && SecondaryHand.IsLeft;
        bool SecondaryIsRight => SecondaryHand != null && !SecondaryHand.IsLeft;

        public Hand LeftHand
        {
            get
            {
                if (PrimaryIsLeft)
                {
                    return PrimaryHand;
                }
                else if (SecondaryIsLeft)
                {
                    return SecondaryHand;
                }
                else
                {
                    return null;
                }
            }
        }

        public Hand RightHand
        {
            get
            {
                if (PrimaryIsRight)
                {
                    return PrimaryHand;
                }
                else if (SecondaryIsRight)
                {
                    return SecondaryHand;
                }
                else
                {
                    return null;
                }
            }
        }

        private LeapTransform trackingTransform;

        private TrackingConnectionManager trackingProvider;
        private readonly IVirtualScreen virtualScreen;
        private readonly IConfigManager configManager;
        private int handsLastFrame;

        public void ConnectToTracking()
        {
            trackingProvider.Connect();
        }

        public void DisconnectFromTracking()
        {
            trackingProvider.Disconnect();
        }

        public HandManager(TrackingConnectionManager _trackingManager, IConfigManager _configManager, IVirtualScreen _virtualScreen)
        {
            handsLastFrame = 0;

            trackingProvider = _trackingManager;
            virtualScreen = _virtualScreen;
            configManager = _configManager;
            if (trackingProvider != null)
            {
                trackingProvider.controller.FrameReady += Update;
            }

            if (_configManager != null)
            {
                _configManager.OnPhysicalConfigUpdated += UpdateTrackingTransform;
                UpdateTrackingTransform(_configManager.PhysicalConfig);
            }
        }

        public bool TrackingServiceConnected()
        {
            return trackingProvider?.controller?.IsServiceConnected ?? false;
        }

        public bool CameraConnected()
        {
            return trackingProvider?.controller?.Devices?.ActiveDevice != null;
        }

        public void UpdateTrackingTransform(PhysicalConfigInternal _config)
        {
            // To simplify the configuration values, positive X angles tilt the Leap towards the screen no matter how its mounted.
            // Therefore, we must convert to the real values before using them.
            // If bottom mounted, the X rotation should be negative if tilted towards the screen so we must negate the X rotation in this instance.
            var isTopMounted = ((_config.LeapRotationD.Z > 179.9f) && (_config.LeapRotationD.Z < 180.1f));
            float xAngleDegree = isTopMounted ? _config.LeapRotationD.X : -_config.LeapRotationD.X;

            System.Numerics.Quaternion quaternion = System.Numerics.Quaternion.CreateFromYawPitchRoll(Utilities.DegreesToRadians(_config.LeapRotationD.Y),
                Utilities.DegreesToRadians(xAngleDegree + _config.ScreenRotationD),
                Utilities.DegreesToRadians(_config.LeapRotationD.Z));

            if (_config.ScreenRotationD != 0)
            {
                var distanceFromScreenBottom = new Leap.Vector(0, _config.LeapPositionRelativeToScreenBottomMm.Y, _config.LeapPositionRelativeToScreenBottomMm.Z).Magnitude;
                var angle = Math.Atan(-_config.LeapPositionRelativeToScreenBottomMm.Z / _config.LeapPositionRelativeToScreenBottomMm.Y);
                var angleWithScreenRotation = Utilities.DegreesToRadians(_config.ScreenRotationD) + angle;

                var translatedYPosition = (float)(distanceFromScreenBottom * Math.Cos(angleWithScreenRotation));
                if (_config.LeapPositionRelativeToScreenBottomMm.Z < 0 && _config.LeapPositionRelativeToScreenBottomMm.Y < 0) {
                    translatedYPosition = -translatedYPosition;
                }

                var translatedUsingScreenPosition = new Leap.Vector(
                    _config.LeapPositionRelativeToScreenBottomMm.X,
                    translatedYPosition,
                    (float)(distanceFromScreenBottom * Math.Sin(angleWithScreenRotation)));

                trackingTransform = new LeapTransform(translatedUsingScreenPosition,
                    new LeapQuaternion(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W));
            }
            else
            {
                trackingTransform = new LeapTransform(
                    new Leap.Vector(
                        _config.LeapPositionRelativeToScreenBottomMm.X,
                        _config.LeapPositionRelativeToScreenBottomMm.Y,
                        -_config.LeapPositionRelativeToScreenBottomMm.Z),
                    new LeapQuaternion(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W));
            }
        }

        public void Update(object sender, FrameEventArgs e)
        {
            var currentFrame = e.frame;
            var handCount = currentFrame.Hands.Count;

            if (handCount == 0 && handsLastFrame > 0)
            {
                HandsLost?.Invoke();
            }
            else if (handCount > 0 && handsLastFrame == 0)
            {
                HandFound?.Invoke();
            }

            handsLastFrame = handCount;

            currentFrame.Transform(trackingTransform);

            Timestamp = currentFrame.Timestamp;

            Hand leftHand = null;
            Hand rightHand = null;

            foreach (Hand hand in currentFrame.Hands)
            {
                if (hand.IsLeft)
                    leftHand = hand;
                else
                    rightHand = hand;
            }

            if (PrimaryHand != null && SecondaryHand != null)
            {
                var primaryHandIndexTip = PrimaryHand.Fingers.Single(x => x.Type == Finger.FingerType.TYPE_INDEX).TipPosition;
                var secondaryHandIndexTip = SecondaryHand.Fingers.Single(x => x.Type == Finger.FingerType.TYPE_INDEX).TipPosition;
                
                var primaryHandIndexTipLocation = virtualScreen.WorldPositionToVirtualScreen(Utilities.LeapVectorToNumerics(primaryHandIndexTip));
                var secondaryHandIndexTipLocation = virtualScreen.WorldPositionToVirtualScreen(Utilities.LeapVectorToNumerics(secondaryHandIndexTip));

                var primaryRelativeXScreenPosition = primaryHandIndexTipLocation.X / configManager.PhysicalConfig.ScreenWidthPX;
                var primaryRelativeYScreenPosition = primaryHandIndexTipLocation.Y / configManager.PhysicalConfig.ScreenHeightPX;
                var secondaryRelativeXScreenPosition = secondaryHandIndexTipLocation.X / configManager.PhysicalConfig.ScreenWidthPX;
                var secondaryRelativeYScreenPosition = secondaryHandIndexTipLocation.Y / configManager.PhysicalConfig.ScreenHeightPX;

                if (lastPrimaryLocation.HasValue && lastSecondaryLocation.HasValue)
                {
                    var primaryLocationChangeX = (lastPrimaryLocation.Value - primaryHandIndexTipLocation).X / configManager.PhysicalConfig.ScreenWidthPX;
                    var primaryLocationChangeY = (lastPrimaryLocation.Value - primaryHandIndexTipLocation).Y / configManager.PhysicalConfig.ScreenHeightPX;
                    var secondaryLocationChangeX = (lastSecondaryLocation.Value - secondaryHandIndexTipLocation).X / configManager.PhysicalConfig.ScreenWidthPX;
                    var secondaryLocationChangeY = (lastSecondaryLocation.Value - secondaryHandIndexTipLocation).Y / configManager.PhysicalConfig.ScreenHeightPX;

                    PrimaryHandActivity = Math.Clamp(PrimaryHandActivity * 0.9f + Math.Abs(primaryLocationChangeX) + Math.Abs(primaryLocationChangeY), 0f, 1f);
                    SecondaryHandActivity = Math.Clamp(SecondaryHandActivity * 0.9f + Math.Abs(secondaryLocationChangeX) + Math.Abs(secondaryLocationChangeY), 0f, 1f);
                }

                lastPrimaryLocation = primaryHandIndexTipLocation;
                lastSecondaryLocation = secondaryHandIndexTipLocation;

                if ((PrimaryHandActivity < 0.01f && SecondaryHandActivity > 0.02f) ||
                    ((primaryRelativeXScreenPosition > 1.4 ||
                      primaryRelativeXScreenPosition < -0.4 ||
                      primaryRelativeYScreenPosition > 1.4 ||
                      primaryRelativeYScreenPosition < -0.4) &&
                     secondaryRelativeXScreenPosition < 1.4 &&
                     secondaryRelativeXScreenPosition > -0.4 &&
                     secondaryRelativeYScreenPosition < 1.4 &&
                     secondaryRelativeYScreenPosition > -0.4))
                {
                    // The primary hand is far off screen swap hands
                    var oldPrimaryHand = PrimaryHand;
                    PrimaryHand = SecondaryHand;
                    SecondaryHand = oldPrimaryHand;
                    primaryChirality = PrimaryHand.IsLeft ? HandChirality.LEFT : HandChirality.RIGHT;
                    secondaryChirality = SecondaryHand.IsLeft ? HandChirality.LEFT : HandChirality.RIGHT;
                }
            }
            else
            {
                PrimaryHandActivity = 1f;
                SecondaryHandActivity = 1f;
                lastPrimaryLocation = null;
                lastSecondaryLocation = null;
            }

            UpdateHandStatus(ref PrimaryHand, leftHand, rightHand, HandType.PRIMARY);
            UpdateHandStatus(ref SecondaryHand, leftHand, rightHand, HandType.SECONDARY);

            HandsUpdated?.Invoke(PrimaryHand, SecondaryHand);
        }

        void UpdateHandStatus(ref Hand _hand, Hand _left, Hand _right, HandType _handType)
        {
            // We must use the cached HandChirality to ensure persistence
            HandChirality handChirality;

            if (_handType == HandType.PRIMARY)
            {
                handChirality = primaryChirality;
            }
            else
            {
                handChirality = secondaryChirality;
            }

            if (_hand == null)
            {
                // Look for a new hand

                if (_handType == HandType.PRIMARY)
                {
                    AssignNewPrimary(_left, _right);
                }
                else
                {
                    AssignNewSecondary(_left, _right);
                }
            }
            else
            {
                // Check hand is still active

                if (handChirality == HandChirality.LEFT && _left != null)
                {
                    // Hand is still left
                    _hand = _left;
                    return;
                }
                else if (handChirality == HandChirality.RIGHT && _right != null)
                {
                    // Hand is still right
                    _hand = _right;
                    return;
                }

                // If we are here, the Hand has been lost. Assign a new Hand.
                if (_handType == HandType.PRIMARY)
                {
                    AssignNewPrimary(_left, _right);
                }
                else
                {
                    AssignNewSecondary(_left, _right);
                }
            }
        }

        void AssignNewPrimary(Hand _left, Hand _right)
        {
            // When assigning a new primary, we should force Secondary to be re-assigned too
            PrimaryHand = null;
            SecondaryHand = null;

            if (_right != null)
            {
                PrimaryHand = _right;
                primaryChirality = HandChirality.RIGHT;
            }
            else if (_left != null)
            {
                PrimaryHand = _left;
                primaryChirality = HandChirality.LEFT;
            }
        }

        void AssignNewSecondary(Hand _left, Hand _right)
        {
            SecondaryHand = null;

            if (_right != null && primaryChirality != HandChirality.RIGHT)
            {
                SecondaryHand = _right;
                secondaryChirality = HandChirality.RIGHT;
            }
            else if (_left != null && primaryChirality != HandChirality.LEFT)
            {
                SecondaryHand = _left;
                secondaryChirality = HandChirality.LEFT;
            }
        }

        public LeapTransform TrackingTransform()
        {
            return trackingTransform;
        }
    }
}
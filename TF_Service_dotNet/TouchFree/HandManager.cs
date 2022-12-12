﻿using Leap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Library.Connections;

namespace Ultraleap.TouchFree.Library
{
    public class HandManager : IHandManager
    {
        public long Timestamp { get; private set; }

        // The PrimaryHand is the hand that appeared first. It does not change until tracking on it is lost.
        public Hand PrimaryHand { get; private set; }
        private HandChirality primaryChirality;

        // The SecondaryHand is the second hand that appears. It may be promoted to the PrimaryHand if the
        // PrimaryHand is lost.
        public Hand SecondaryHand { get; private set; }
        private HandChirality secondaryChirality;

        public List<Leap.Vector> RawHandPositions { get; private set; }

        public event Action HandFound;
        public event Action HandsLost;
        public delegate void HandUpdate(Hand primary, Hand secondary);
        public event HandUpdate HandsUpdated;

        private const float PrimaryHandActivityDefault = 0.02f;
        private const float SecondaryHandActivityDefault = 0.01f;

        private float PrimaryHandActivity = PrimaryHandActivityDefault;
        private float SecondaryHandActivity = SecondaryHandActivityDefault;

        private Vector3? lastPrimaryLocation;
        private Vector3? lastSecondaryLocation;

        public ArraySegment<byte> LastImageData { get; private set; }

        public Image.CameraType HandRenderLens { private get; set; } = Image.CameraType.LEFT;

        public Hand LeftHand =>
            PrimaryHand is { IsLeft: true } ? PrimaryHand :
            SecondaryHand is { IsLeft: true } ? SecondaryHand :
            null;

        public Hand RightHand =>
            PrimaryHand is { IsRight: true } ? PrimaryHand :
            SecondaryHand is { IsRight: true } ? SecondaryHand :
            null;

        private LeapTransform trackingTransform;

        public ITrackingConnectionManager ConnectionManager { get; }

        private readonly IVirtualScreen virtualScreen;
        private readonly IConfigManager configManager;

        private int handsLastFrame;

        private Leap.Image lastImage;

        public HandFrame RawHands { get; private set; }
        public List<Hand> PreConversionRawHands { get; private set; }
        public bool RawHandsUpdated { get; private set; }

        public HandManager(ITrackingConnectionManager _trackingManager, IConfigManager _configManager, IVirtualScreen _virtualScreen, IUpdateBehaviour _updateBehaviour)
        {
            handsLastFrame = 0;

            ConnectionManager = _trackingManager;
            virtualScreen = _virtualScreen;
            configManager = _configManager;

            if (_configManager != null)
            {
                _configManager.OnPhysicalConfigUpdated += UpdateTrackingTransform;
                UpdateTrackingTransform(_configManager.PhysicalConfig);
            }

            if (ConnectionManager != null)
            {
                ConnectionManager.Controller.FrameReady += Update;
                ConnectionManager.Controller.ImageReady += StoreImage;
            }
            _updateBehaviour.OnSlowUpdate += UpdateRawHands;
        }

        public void UpdateTrackingTransform(PhysicalConfigInternal _config)
        {
            // To simplify the configuration values, positive X angles tilt the Leap towards the screen no matter how its mounted.
            // Therefore, we must convert to the real values before using them.
            // If bottom mounted, the X rotation should be negative if tilted towards the screen so we must negate the X rotation in this instance.
            var isTopMounted = _config.LeapRotationD.Z is > 179.9f and < 180.1f;
            float xAngleDegree = isTopMounted ? _config.LeapRotationD.X : -_config.LeapRotationD.X;

            var quaternion = Quaternion.CreateFromYawPitchRoll(Utilities.DegreesToRadians(_config.LeapRotationD.Y),
                Utilities.DegreesToRadians(xAngleDegree + _config.ScreenRotationD),
                Utilities.DegreesToRadians(_config.LeapRotationD.Z));

            if (_config.ScreenRotationD != 0)
            {
                var distanceFromScreenBottom = new Leap.Vector(0, _config.LeapPositionRelativeToScreenBottomMm.Y, _config.LeapPositionRelativeToScreenBottomMm.Z).Magnitude;
                var angle = Math.Atan(-_config.LeapPositionRelativeToScreenBottomMm.Z / _config.LeapPositionRelativeToScreenBottomMm.Y);
                var angleWithScreenRotation = Utilities.DegreesToRadians(_config.ScreenRotationD) + angle;

                var translatedYPosition = (float)(distanceFromScreenBottom * Math.Cos(angleWithScreenRotation));
                if (_config.LeapPositionRelativeToScreenBottomMm.Z < 0 && _config.LeapPositionRelativeToScreenBottomMm.Y < 0)
                {
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

        private Vector3 LeapToCameraFrame(Leap.Vector leapVector, Image image)
        {
            var lensAdjustment = HandRenderLens == Image.CameraType.LEFT ? -32.5f : 32.5f;
            var updatedVector = new Leap.Vector(leapVector.x + lensAdjustment, leapVector.y, leapVector.z);
            var ray = new Leap.Vector((float)Math.Atan2(updatedVector.x, updatedVector.y), (float)Math.Atan2(updatedVector.z, updatedVector.y), (float)Math.Sqrt(updatedVector.x * updatedVector.x + updatedVector.z * updatedVector.z + updatedVector.y * updatedVector.y));

            var renderPosition = image.RectilinearToPixel(HandRenderLens, ray);

            return new Vector3(renderPosition.x / image.Width, renderPosition.y / image.Width, ray.z);
        }

        public void StoreImage(object sender, ImageEventArgs e)
        {
            lastImage = e.image;
        }

        public void UpdateRawHands()
        {
            var imageToUse = lastImage;

            if (lastImage != null)
            {
                LastImageData = new ArraySegment<byte>(imageToUse.Data(Image.CameraType.LEFT), (int)imageToUse.ByteOffset(HandRenderLens), imageToUse.Height * imageToUse.Width * imageToUse.BytesPerPixel);

                if (PreConversionRawHands != null && imageToUse != null && RawHandsUpdated)
                {
                    RawHandsUpdated = false;
                    RawHands = new HandFrame()
                    {
                        Hands = PreConversionRawHands.Select(x => new RawHand()
                        {
                            CurrentPrimary = x.IsLeft == (primaryChirality == HandChirality.LEFT),
                            Fingers = x.Fingers.Select(f => new RawFinger()
                            {
                                Type = (FingerType)f.Type,
                                Bones = f.bones.Select(b => new RawBone()
                                {
                                    NextJoint = b.Type == Bone.BoneType.TYPE_DISTAL ? LeapToCameraFrame(b.NextJoint, imageToUse) : default,
                                    PrevJoint = b.Type== Bone.BoneType.TYPE_PROXIMAL ? LeapToCameraFrame(b.PrevJoint, imageToUse) : default
                                }).ToArray()
                            }).ToArray(),
                            WristPosition = LeapToCameraFrame(x.WristPosition, imageToUse),
                            WristWidth = x.PalmWidth
                        }).ToArray()
                    };
                }

                if (lastImage == imageToUse)
                {
                    lastImage = null;
                }
            }
        }

        public void Update(object sender, FrameEventArgs e)
        {
            var currentFrame = e.frame;
            var handCount = currentFrame.Hands.Count;

            switch (handCount)
            {
                case 0 when handsLastFrame > 0:
                    HandsLost?.Invoke();
                    break;
                case > 0 when handsLastFrame == 0:
                    HandFound?.Invoke();
                    break;
            }

            RawHandPositions = currentFrame.Hands
                .Select(x => x.Fingers?.SingleOrDefault(y => y.Type == Finger.FingerType.TYPE_INDEX)?.TipPosition)
                .Where(x => x.HasValue)
                .Select(x => x.Value)
                .ToList();

            handsLastFrame = handCount;

            PreConversionRawHands = currentFrame.Hands;
            RawHandsUpdated = true;

            currentFrame = currentFrame.TransformedCopy(trackingTransform);

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

            if (PrimaryHand?.Fingers?.Count(x => x.Type == Finger.FingerType.TYPE_INDEX) > 0 &&
                SecondaryHand?.Fingers?.Count(x => x.Type == Finger.FingerType.TYPE_INDEX) > 0)
            {
                var primaryHandIndexTip = PrimaryHand.Fingers.Single(x => x.Type == Finger.FingerType.TYPE_INDEX).TipPosition;
                var secondaryHandIndexTip = SecondaryHand.Fingers.Single(x => x.Type == Finger.FingerType.TYPE_INDEX).TipPosition;

                var primaryHandIndexTipLocation = virtualScreen.WorldPositionToVirtualScreen(Utilities.LeapVectorToNumerics(primaryHandIndexTip));
                var secondaryHandIndexTipLocation = virtualScreen.WorldPositionToVirtualScreen(Utilities.LeapVectorToNumerics(secondaryHandIndexTip));

                var screenWidthPX = configManager.PhysicalConfig.ScreenWidthPX;
                var screenHeightPX = configManager.PhysicalConfig.ScreenHeightPX;

                var primaryRelativeXScreenPosition = primaryHandIndexTipLocation.X / screenWidthPX;
                var primaryRelativeYScreenPosition = primaryHandIndexTipLocation.Y / screenHeightPX;
                var secondaryRelativeXScreenPosition = secondaryHandIndexTipLocation.X / screenWidthPX;
                var secondaryRelativeYScreenPosition = secondaryHandIndexTipLocation.Y / screenHeightPX;

                if (lastPrimaryLocation.HasValue && lastSecondaryLocation.HasValue)
                {
                    var primaryLocationChange = lastPrimaryLocation.Value - primaryHandIndexTipLocation;
                    var primaryLocationChangeX = primaryLocationChange.X / screenWidthPX;
                    var primaryLocationChangeY = primaryLocationChange.Y / screenHeightPX;

                    var secondaryLocationChange = lastSecondaryLocation.Value - secondaryHandIndexTipLocation;
                    var secondaryLocationChangeX = secondaryLocationChange.X / screenWidthPX;
                    var secondaryLocationChangeY = secondaryLocationChange.Y / screenHeightPX;

                    PrimaryHandActivity = Math.Clamp(PrimaryHandActivity * 0.9f + Math.Abs(primaryLocationChangeX) + Math.Abs(primaryLocationChangeY), 0f, 1f);
                    SecondaryHandActivity = Math.Clamp(SecondaryHandActivity * 0.9f + Math.Abs(secondaryLocationChangeX) + Math.Abs(secondaryLocationChangeY), 0f, 1f);
                }

                lastPrimaryLocation = primaryHandIndexTipLocation;
                lastSecondaryLocation = secondaryHandIndexTipLocation;

                if (SecondaryHandIsOnScreen(secondaryRelativeXScreenPosition, secondaryRelativeYScreenPosition) &&
                    (HandActivityInSwapThreshold() ||
                     PrimaryHandIsOffScreen(primaryRelativeXScreenPosition, primaryRelativeYScreenPosition)))
                {
                    (PrimaryHand, SecondaryHand) = (SecondaryHand, PrimaryHand);
                    primaryChirality = PrimaryHand.IsLeft ? HandChirality.LEFT : HandChirality.RIGHT;
                    secondaryChirality = SecondaryHand.IsLeft ? HandChirality.LEFT : HandChirality.RIGHT;
                }
            }
            else
            {
                PrimaryHandActivity = PrimaryHandActivityDefault;
                SecondaryHandActivity = SecondaryHandActivityDefault;
                lastPrimaryLocation = null;
                lastSecondaryLocation = null;
            }

            UpdateHandStatus(PrimaryHand, leftHand, rightHand, HandType.PRIMARY);
            UpdateHandStatus(SecondaryHand, leftHand, rightHand, HandType.SECONDARY);

            HandsUpdated?.Invoke(PrimaryHand, SecondaryHand);
        }

        private bool HandActivityInSwapThreshold()
        {
            return PrimaryHandActivity < SecondaryHandActivityDefault && SecondaryHandActivity > PrimaryHandActivityDefault;
        }

        private static bool PrimaryHandIsOffScreen(float _primaryRelativeXScreenPosition, float _primaryRelativeYScreenPosition) =>
            _primaryRelativeXScreenPosition is > 1.4f or < -0.4f ||
            _primaryRelativeYScreenPosition is > 1.4f or < -0.4f;

        private static bool SecondaryHandIsOnScreen(float _secondaryRelativeXScreenPosition, float _secondaryRelativeYScreenPosition) =>
            _secondaryRelativeXScreenPosition is < 1.4f and > -0.4f &&
            _secondaryRelativeYScreenPosition is < 1.4f and > -0.4f;

        private void UpdateHandStatus(Hand _hand, Hand _left, Hand _right, HandType _handType)
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
                    AssignHandAccordingToType(_handType, _left);
                    return;
                }
                else if (handChirality == HandChirality.RIGHT && _right != null)
                {
                    // Hand is still right
                    AssignHandAccordingToType(_handType, _right);
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

        private void AssignHandAccordingToType(HandType _handType, Hand _hand)
        {
            // Hand is still right
            if (_handType == HandType.PRIMARY)
            {
                PrimaryHand = _hand;
            }
            else
            {
                SecondaryHand = _hand;
            }
        }

        private void AssignNewPrimary(Hand _left, Hand _right)
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

        private void AssignNewSecondary(Hand _left, Hand _right)
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

        public LeapTransform TrackingTransform() => trackingTransform;
    }
}
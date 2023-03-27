using Leap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Library.Connections;

namespace Ultraleap.TouchFree.Library;

public class HandManager : IHandManager
{
    public long Timestamp { get; private set; }

    // The PrimaryHand is the hand that appeared first. It does not change until tracking on it is lost.
    public Hand PrimaryHand { get; private set; }
    private HandChirality _primaryChirality;

    // The SecondaryHand is the second hand that appears. It may be promoted to the PrimaryHand if the
    // PrimaryHand is lost.
    public Hand SecondaryHand { get; private set; }
    private HandChirality _secondaryChirality;

    public List<Leap.Vector> RawHandPositions { get; private set; }

    public event Action HandFound;
    public event Action HandsLost;
    public delegate void HandUpdate(Hand primary, Hand secondary);
    public event HandUpdate HandsUpdated;

    private const float _primaryHandActivityDefault = 0.02f;
    private const float _secondaryHandActivityDefault = 0.01f;

    private float _primaryHandActivity = _primaryHandActivityDefault;
    private float _secondaryHandActivity = _secondaryHandActivityDefault;

    private Vector3? _lastPrimaryLocation;
    private Vector3? _lastSecondaryLocation;

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

    private LeapTransform _trackingTransform;

    public ITrackingConnectionManager ConnectionManager { get; }

    private readonly IVirtualScreen _virtualScreen;
    private readonly IConfigManager _configManager;

    private int _handsLastFrame;

    private Leap.Image _lastImage;

    public HandFrame RawHands { get; private set; }
    
    private List<Hand> _preConversionRawHands;
    private bool _rawHandsUpdated;

    public HandManager(ITrackingConnectionManager trackingManager, IConfigManager configManager, IVirtualScreen virtualScreen, IUpdateBehaviour updateBehaviour)
    {
        _handsLastFrame = 0;

        ConnectionManager = trackingManager;
        _virtualScreen = virtualScreen;
        _configManager = configManager;

        if (configManager != null)
        {
            configManager.OnPhysicalConfigUpdated += UpdateTrackingTransform;
            UpdateTrackingTransform(configManager.PhysicalConfig);
        }

        if (ConnectionManager != null)
        {
            ConnectionManager.Controller.FrameReady += Update;
            ConnectionManager.Controller.ImageReady += StoreImage;
        }
        updateBehaviour.OnSlowUpdate += UpdateRawHands;
    }

    public void UpdateTrackingTransform(PhysicalConfigInternal config)
    {
        // To simplify the configuration values, positive X angles tilt the Leap towards the screen no matter how its mounted.
        // Therefore, we must convert to the real values before using them.
        // If bottom mounted, the X rotation should be negative if tilted towards the screen so we must negate the X rotation in this instance.
        var isTopMounted = config.LeapRotationD.Z is > 179.9f and < 180.1f;
        float xAngleDegree = isTopMounted ? config.LeapRotationD.X : -config.LeapRotationD.X;

        var quaternion = Quaternion.CreateFromYawPitchRoll(Utilities.DegreesToRadians(config.LeapRotationD.Y),
            Utilities.DegreesToRadians(xAngleDegree + config.ScreenRotationD),
            Utilities.DegreesToRadians(config.LeapRotationD.Z));

        if (config.ScreenRotationD != 0)
        {
            var distanceFromScreenBottom = new Leap.Vector(0, config.LeapPositionRelativeToScreenBottomMm.Y, config.LeapPositionRelativeToScreenBottomMm.Z).Magnitude;
            var angle = Math.Atan(-config.LeapPositionRelativeToScreenBottomMm.Z / config.LeapPositionRelativeToScreenBottomMm.Y);
            var angleWithScreenRotation = Utilities.DegreesToRadians(config.ScreenRotationD) + angle;

            var translatedYPosition = (float)(distanceFromScreenBottom * Math.Cos(angleWithScreenRotation));
            if (config.LeapPositionRelativeToScreenBottomMm.Z < 0 && config.LeapPositionRelativeToScreenBottomMm.Y < 0)
            {
                translatedYPosition = -translatedYPosition;
            }

            var translatedUsingScreenPosition = new Leap.Vector(
                config.LeapPositionRelativeToScreenBottomMm.X,
                translatedYPosition,
                (float)(distanceFromScreenBottom * Math.Sin(angleWithScreenRotation)));

            _trackingTransform = new LeapTransform(translatedUsingScreenPosition,
                new LeapQuaternion(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W));
        }
        else
        {
            _trackingTransform = new LeapTransform(
                new Leap.Vector(
                    config.LeapPositionRelativeToScreenBottomMm.X,
                    config.LeapPositionRelativeToScreenBottomMm.Y,
                    -config.LeapPositionRelativeToScreenBottomMm.Z),
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

    private void StoreImage(object sender, ImageEventArgs e)
    {
        _lastImage = e.image;
    }

    private void UpdateRawHands()
    {
        var imageToUse = _lastImage;

        if (_lastImage != null)
        {
            LastImageData = new ArraySegment<byte>(imageToUse.Data(Image.CameraType.LEFT), (int)imageToUse.ByteOffset(HandRenderLens), imageToUse.Height * imageToUse.Width * imageToUse.BytesPerPixel);

            if (_preConversionRawHands != null && imageToUse != null && _rawHandsUpdated)
            {
                _rawHandsUpdated = false;
                RawHands = new HandFrame(Hands: _preConversionRawHands.Select(x =>
                    new RawHand(CurrentPrimary: x.IsLeft == (_primaryChirality == HandChirality.LEFT), Fingers: x
                        .Fingers.Select(f => new RawFinger(Type: (FingerType)f.Type, Bones: f.bones.Select(b =>
                            new RawBone(NextJoint: b.Type == Bone.BoneType.TYPE_DISTAL
                                ? LeapToCameraFrame(b.NextJoint, imageToUse)
                                : default, PrevJoint: b.Type == Bone.BoneType.TYPE_PROXIMAL
                                ? LeapToCameraFrame(b.PrevJoint, imageToUse)
                                : default)).ToArray())).ToArray(), WristPosition: LeapToCameraFrame(x.WristPosition, imageToUse),
                    WristWidth: x.PalmWidth)).ToArray());
            }

            if (_lastImage == imageToUse)
            {
                _lastImage = null;
            }
        }
    }

    public void Update(object sender, FrameEventArgs e)
    {
        var currentFrame = e.frame;
        var handCount = currentFrame.Hands.Count;

        switch (handCount)
        {
            case 0 when _handsLastFrame > 0:
                HandsLost?.Invoke();
                break;
            case > 0 when _handsLastFrame == 0:
                HandFound?.Invoke();
                break;
        }

        RawHandPositions = currentFrame.Hands
            .Select(x => x.Fingers?.SingleOrDefault(y => y.Type == Finger.FingerType.TYPE_INDEX)?.TipPosition)
            .Where(x => x.HasValue)
            .Select(x => x.Value)
            .ToList();

        _handsLastFrame = handCount;

        _preConversionRawHands = currentFrame.Hands;
        _rawHandsUpdated = true;

        currentFrame = currentFrame.TransformedCopy(_trackingTransform);

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

            var primaryHandIndexTipLocation = _virtualScreen.WorldPositionToVirtualScreen(Utilities.LeapVectorToNumerics(primaryHandIndexTip));
            var secondaryHandIndexTipLocation = _virtualScreen.WorldPositionToVirtualScreen(Utilities.LeapVectorToNumerics(secondaryHandIndexTip));

            var screenWidthPx = _configManager.PhysicalConfig.ScreenWidthPX;
            var screenHeightPx = _configManager.PhysicalConfig.ScreenHeightPX;

            var primaryRelativeXScreenPosition = primaryHandIndexTipLocation.X / screenWidthPx;
            var primaryRelativeYScreenPosition = primaryHandIndexTipLocation.Y / screenHeightPx;
            var secondaryRelativeXScreenPosition = secondaryHandIndexTipLocation.X / screenWidthPx;
            var secondaryRelativeYScreenPosition = secondaryHandIndexTipLocation.Y / screenHeightPx;

            if (_lastPrimaryLocation.HasValue && _lastSecondaryLocation.HasValue)
            {
                var primaryLocationChange = _lastPrimaryLocation.Value - primaryHandIndexTipLocation;
                var primaryLocationChangeX = primaryLocationChange.X / screenWidthPx;
                var primaryLocationChangeY = primaryLocationChange.Y / screenHeightPx;

                var secondaryLocationChange = _lastSecondaryLocation.Value - secondaryHandIndexTipLocation;
                var secondaryLocationChangeX = secondaryLocationChange.X / screenWidthPx;
                var secondaryLocationChangeY = secondaryLocationChange.Y / screenHeightPx;

                _primaryHandActivity = Math.Clamp(_primaryHandActivity * 0.9f + Math.Abs(primaryLocationChangeX) + Math.Abs(primaryLocationChangeY), 0f, 1f);
                _secondaryHandActivity = Math.Clamp(_secondaryHandActivity * 0.9f + Math.Abs(secondaryLocationChangeX) + Math.Abs(secondaryLocationChangeY), 0f, 1f);
            }

            _lastPrimaryLocation = primaryHandIndexTipLocation;
            _lastSecondaryLocation = secondaryHandIndexTipLocation;

            if (SecondaryHandIsOnScreen(secondaryRelativeXScreenPosition, secondaryRelativeYScreenPosition) &&
                (HandActivityInSwapThreshold() ||
                 PrimaryHandIsOffScreen(primaryRelativeXScreenPosition, primaryRelativeYScreenPosition)))
            {
                (PrimaryHand, SecondaryHand) = (SecondaryHand, PrimaryHand);
                _primaryChirality = PrimaryHand.IsLeft ? HandChirality.LEFT : HandChirality.RIGHT;
                _secondaryChirality = SecondaryHand.IsLeft ? HandChirality.LEFT : HandChirality.RIGHT;
            }
        }
        else
        {
            _primaryHandActivity = _primaryHandActivityDefault;
            _secondaryHandActivity = _secondaryHandActivityDefault;
            _lastPrimaryLocation = null;
            _lastSecondaryLocation = null;
        }

        UpdateHandStatus(PrimaryHand, leftHand, rightHand, HandType.PRIMARY);
        UpdateHandStatus(SecondaryHand, leftHand, rightHand, HandType.SECONDARY);

        HandsUpdated?.Invoke(PrimaryHand, SecondaryHand);
    }

    private bool HandActivityInSwapThreshold()
    {
        return _primaryHandActivity < _secondaryHandActivityDefault && _secondaryHandActivity > _primaryHandActivityDefault;
    }

    private static bool PrimaryHandIsOffScreen(float primaryRelativeXScreenPosition, float primaryRelativeYScreenPosition) =>
        primaryRelativeXScreenPosition is > 1.4f or < -0.4f ||
        primaryRelativeYScreenPosition is > 1.4f or < -0.4f;

    private static bool SecondaryHandIsOnScreen(float secondaryRelativeXScreenPosition, float secondaryRelativeYScreenPosition) =>
        secondaryRelativeXScreenPosition is < 1.4f and > -0.4f &&
        secondaryRelativeYScreenPosition is < 1.4f and > -0.4f;

    private void UpdateHandStatus(Hand hand, Hand left, Hand right, HandType handType)
    {
        // We must use the cached HandChirality to ensure persistence
        HandChirality handChirality;

        if (handType == HandType.PRIMARY)
        {
            handChirality = _primaryChirality;
        }
        else
        {
            handChirality = _secondaryChirality;
        }

        if (hand == null)
        {
            // Look for a new hand

            if (handType == HandType.PRIMARY)
            {
                AssignNewPrimary(left, right);
            }
            else
            {
                AssignNewSecondary(left, right);
            }
        }
        else
        {
            // Check hand is still active

            if (handChirality == HandChirality.LEFT && left != null)
            {
                // Hand is still left
                AssignHandAccordingToType(handType, left);
                return;
            }
            else if (handChirality == HandChirality.RIGHT && right != null)
            {
                // Hand is still right
                AssignHandAccordingToType(handType, right);
                return;
            }

            // If we are here, the Hand has been lost. Assign a new Hand.
            if (handType == HandType.PRIMARY)
            {
                AssignNewPrimary(left, right);
            }
            else
            {
                AssignNewSecondary(left, right);
            }
        }
    }

    private void AssignHandAccordingToType(HandType handType, Hand hand)
    {
        // Hand is still right
        if (handType == HandType.PRIMARY)
        {
            PrimaryHand = hand;
        }
        else
        {
            SecondaryHand = hand;
        }
    }

    private void AssignNewPrimary(Hand left, Hand right)
    {
        // When assigning a new primary, we should force Secondary to be re-assigned too
        PrimaryHand = null;
        SecondaryHand = null;

        if (right != null)
        {
            PrimaryHand = right;
            _primaryChirality = HandChirality.RIGHT;
        }
        else if (left != null)
        {
            PrimaryHand = left;
            _primaryChirality = HandChirality.LEFT;
        }
    }

    private void AssignNewSecondary(Hand left, Hand right)
    {
        SecondaryHand = null;

        if (right != null && _primaryChirality != HandChirality.RIGHT)
        {
            SecondaryHand = right;
            _secondaryChirality = HandChirality.RIGHT;
        }
        else if (left != null && _primaryChirality != HandChirality.LEFT)
        {
            SecondaryHand = left;
            _secondaryChirality = HandChirality.LEFT;
        }
    }

    public LeapTransform TrackingTransform() => _trackingTransform;
}
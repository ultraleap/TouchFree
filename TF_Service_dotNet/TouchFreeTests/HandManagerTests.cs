using NUnit.Framework;
using Ultraleap.TouchFree.Library;
using Ultraleap.TouchFree.Library.Configuration;
using System;
using Leap;

namespace TouchFreeTests
{
    class HandManagerTests
    {
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void TranslationIsCorrectlyConstructedFromConfig()
        {
            // Given
            HandManager handManger = new (null, null);
            System.Numerics.Vector3 translationInMillimeters = new (100f, 200f, 300f);
            Leap.Vector translationInLeapSpace = new Leap.Vector(translationInMillimeters.X, translationInMillimeters.Y, -translationInMillimeters.Z);
            PhysicalConfig testConfig = new () { LeapPositionRelativeToScreenBottomMm = translationInMillimeters };

            // When
            handManger.UpdateTrackingTransform(testConfig);

            // Then
            Assert.AreEqual(translationInLeapSpace, handManger.TrackingTransform().translation);
        }

        [Test]
        public void UpdateTrackingTransform_TopMountedOrientation_XRotationNotInverted()
        {
            // Given
            HandManager handManger = new (null, null);
            System.Numerics.Vector3 topDownRotation = new (45, 0, 180);
            System.Numerics.Quaternion topDownQuaternion = System.Numerics.Quaternion.CreateFromYawPitchRoll(
                Utilities.DegreesToRadians(topDownRotation.Y),
                Utilities.DegreesToRadians(topDownRotation.X),
                Utilities.DegreesToRadians(topDownRotation.Z));

            PhysicalConfig testConfig = new () { LeapRotationD = topDownRotation };

            //When
            handManger.UpdateTrackingTransform(testConfig);

            //Then
            System.Numerics.Quaternion handManagerRotation = new ()
            {
                X = handManger.TrackingTransform().rotation.x,
                Y = handManger.TrackingTransform().rotation.y,
                Z = handManger.TrackingTransform().rotation.z,
                W = handManger.TrackingTransform().rotation.w
            };
            Assert.AreEqual(topDownQuaternion, handManagerRotation);
        }

        [Test]
        public void UpdateTrackingTransform_BottomMountedOrientation_XRotationInverted()
        {
            // Given
            HandManager handManger = new (null, null);
            System.Numerics.Vector3 bottomRotation = new (45, 0, 0);
            System.Numerics.Quaternion bottomQuaternion = System.Numerics.Quaternion.CreateFromYawPitchRoll(
                Utilities.DegreesToRadians(bottomRotation.Y),
                Utilities.DegreesToRadians(-bottomRotation.X),
                Utilities.DegreesToRadians(bottomRotation.Z));

            PhysicalConfig testConfig = new () { LeapRotationD = bottomRotation };

            //When
            handManger.UpdateTrackingTransform(testConfig);

            //Then
            System.Numerics.Quaternion handManagerRotation = new ()
            {
                X = handManger.TrackingTransform().rotation.x,
                Y = handManger.TrackingTransform().rotation.y,
                Z = handManger.TrackingTransform().rotation.z,
                W = handManger.TrackingTransform().rotation.w
            };
            Assert.AreEqual(bottomQuaternion, handManagerRotation);
        }

        [Test]
        public void UpdateTrackingTransform_BottomMountedOrientation_UseLeapPositionRelative()
        {
            // Given
            HandManager handManger = new(null, null);
            System.Numerics.Vector3 bottomRotation = new(45, 0, 0);
            System.Numerics.Vector3 relativePosition = new(1000, 2000, 3000);
            System.Numerics.Vector3 positionTranslation = new(1000, 2000, -3000);

            PhysicalConfig testConfig = new() { LeapRotationD = bottomRotation, LeapPositionRelativeToScreenBottomMm = relativePosition };

            //When
            handManger.UpdateTrackingTransform(testConfig);

            //Then
            System.Numerics.Vector3 handManagerRelativePosition = new()
            {
                X = handManger.TrackingTransform().translation.x,
                Y = handManger.TrackingTransform().translation.y,
                Z = handManger.TrackingTransform().translation.z
            };
            Assert.AreEqual(positionTranslation, handManagerRelativePosition);
        }

        [Test]
        public void UpdateTrackingTransform_BottomMountedOrientationScreenIsRotated_UseLeapPositionRelativeWithScreenRotation()
        {
            // Given
            HandManager handManger = new(null, null);
            System.Numerics.Vector3 bottomRotation = new(45, 0, 0);
            System.Numerics.Vector3 relativePosition = new(1000, 1000, 1000);
            System.Numerics.Vector3 positionTranslation = new(1000, 1158.46f, -811.16f);

            PhysicalConfig testConfig = new() { LeapRotationD = bottomRotation, LeapPositionRelativeToScreenBottomMm = relativePosition, ScreenRotationD = 10 };

            //When
            handManger.UpdateTrackingTransform(testConfig);

            //Then
            Assert.AreEqual(positionTranslation.X, handManger.TrackingTransform().translation.x, 0.01);
            Assert.AreEqual(positionTranslation.Y, handManger.TrackingTransform().translation.y, 0.01);
            Assert.AreEqual(positionTranslation.Z, handManger.TrackingTransform().translation.z, 0.01);
        }

        [Test]
        public void Update_NoHandsInFrame_HandsAreNull()
        {
            //Given
            Frame frame = new Frame();
            FrameEventArgs frameEventArgs = new FrameEventArgs(frame);
            HandManager handManger = new(null, null);

            //When
            handManger.Update(this, frameEventArgs);

            //Then
            Assert.IsNull(handManger.PrimaryHand);
            Assert.IsNull(handManger.SecondaryHand);
        }

        [Test]
        public void Update_LeftHandInFrame_LeftHandSetToPrimaryAndSecondaryIsNull()
        {
            //Given
            Hand hand = new Hand() { IsLeft = true };
            Frame frame = new Frame();
            frame.Hands.Add(hand);
            FrameEventArgs frameEventArgs = new FrameEventArgs(frame);
            HandManager handManger = new(null, null);

            //When
            handManger.Update(this, frameEventArgs);

            //Then
            Assert.IsNotNull(handManger.PrimaryHand);
            Assert.AreEqual(true, handManger.PrimaryHand.IsLeft);
            Assert.IsNull(handManger.SecondaryHand);
        }

        [Test]
        public void Update_RightHandInFrame_RightHandSetToPrimaryAndSecondaryIsNull()
        {
            //Given
            Hand hand = new Hand() { IsLeft = false };
            Frame frame = new Frame();
            frame.Hands.Add(hand);
            FrameEventArgs frameEventArgs = new FrameEventArgs(frame);
            HandManager handManger = new(null, null);

            //When
            handManger.Update(this, frameEventArgs);

            //Then
            Assert.IsNotNull(handManger.PrimaryHand);
            Assert.AreEqual(false, handManger.PrimaryHand.IsLeft);
            Assert.IsNull(handManger.SecondaryHand);
        }

        [Test]
        public void Update_BothHandsInFrame_RightHandSetToPrimaryAndLeftHandSetToSecondary()
        {
            //Given
            Hand leftHand = new Hand() { IsLeft = true };
            Hand rightHand = new Hand() { IsLeft = false };
            Frame frame = new Frame();
            frame.Hands.Add(leftHand);
            frame.Hands.Add(rightHand);
            FrameEventArgs frameEventArgs = new FrameEventArgs(frame);
            HandManager handManger = new(null, null);

            //When
            handManger.Update(this, frameEventArgs);

            //Then
            Assert.IsNotNull(handManger.PrimaryHand);
            Assert.AreEqual(false, handManger.PrimaryHand.IsLeft);
            Assert.IsNotNull(handManger.SecondaryHand);
            Assert.AreEqual(true, handManger.SecondaryHand.IsLeft);
        }

        [Test]
        public void Update_LeftHandAlreadyInFrameAndThenRightHandAddedToFrame_LeftHandPrimaryAndRightHandSecondary()
        {
            //Given
            Hand leftHand = new Hand() { IsLeft = true };
            Hand rightHand = new Hand() { IsLeft = false };
            Frame firstFrame = new Frame();
            firstFrame.Hands.Add(leftHand);
            FrameEventArgs firstFrameEventArgs = new FrameEventArgs(firstFrame);
            HandManager handManger = new(null, null);
            handManger.Update(this, firstFrameEventArgs);

            Frame secondFrame = new Frame();
            FrameEventArgs secondFrameEventArgs = new FrameEventArgs(secondFrame);
            secondFrame.Hands.Add(leftHand);
            secondFrame.Hands.Add(rightHand);

            //When
            handManger.Update(this, secondFrameEventArgs);

            //Then
            Assert.IsNotNull(handManger.PrimaryHand);
            Assert.AreEqual(true, handManger.PrimaryHand.IsLeft);
            Assert.IsNotNull(handManger.SecondaryHand);
            Assert.AreEqual(false, handManger.SecondaryHand.IsLeft);
        }

        [Test]
        public void Update_RightHandPrimaryLeftSecondaryThenRightLostFromFrame_LeftHandSetToPrimaryAndRightIsNull()
        {
            //Given
            Hand leftHand = new Hand() { IsLeft = true };
            Hand rightHand = new Hand() { IsLeft = false };
            Frame firstFrame = new Frame();
            firstFrame.Hands.Add(leftHand);
            firstFrame.Hands.Add(rightHand);
            FrameEventArgs firstFrameEventArgs = new FrameEventArgs(firstFrame);
            HandManager handManger = new(null, null);
            handManger.Update(this, firstFrameEventArgs);

            Frame secondFrame = new Frame();
            secondFrame.Hands.Add(leftHand);
            FrameEventArgs secondFrameEventArgs = new FrameEventArgs(secondFrame);

            //When
            handManger.Update(this, secondFrameEventArgs);

            //Then
            Assert.IsNotNull(handManger.PrimaryHand);
            Assert.AreEqual(true, handManger.PrimaryHand.IsLeft);
            Assert.IsNull(handManger.SecondaryHand);
        }
    }
}

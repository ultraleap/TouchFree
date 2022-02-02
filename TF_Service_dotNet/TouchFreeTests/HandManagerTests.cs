using NUnit.Framework;
using Ultraleap.TouchFree.Library;
using Ultraleap.TouchFree.Library.Configuration;
using System;

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
            System.Numerics.Vector3 translationInMeters = new (0.1f, 0.2f, 0.3f);
            Leap.Vector translationInLeapSpace = new Leap.Vector(translationInMeters.X, translationInMeters.Y, -translationInMeters.Z) * 1000;
            PhysicalConfig testConfig = new () { LeapPositionRelativeToScreenBottomM = translationInMeters };

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
                VirtualScreen.DegreesToRadians(topDownRotation.Y), 
                VirtualScreen.DegreesToRadians(topDownRotation.X),
                VirtualScreen.DegreesToRadians(topDownRotation.Z));

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
                VirtualScreen.DegreesToRadians(bottomRotation.Y),
                VirtualScreen.DegreesToRadians(-bottomRotation.X),
                VirtualScreen.DegreesToRadians(bottomRotation.Z));

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
            System.Numerics.Vector3 relativePosition = new(1, 2, 3);
            System.Numerics.Vector3 positionTranslation = new(1000, 2000, -3000);

            PhysicalConfig testConfig = new() { LeapRotationD = bottomRotation, LeapPositionRelativeToScreenBottomM = relativePosition };

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
            System.Numerics.Vector3 relativePosition = new(1, 1, 1);
            System.Numerics.Vector3 positionTranslation = new(1000, 1158.46f, -811.16f);

            PhysicalConfig testConfig = new() { LeapRotationD = bottomRotation, LeapPositionRelativeToScreenBottomM = relativePosition, ScreenRotationD = 10 };

            //When
            handManger.UpdateTrackingTransform(testConfig);

            //Then
            Assert.AreEqual(positionTranslation.X, handManger.TrackingTransform().translation.x, 0.01);
            Assert.AreEqual(positionTranslation.Y, handManger.TrackingTransform().translation.y, 0.01);
            Assert.AreEqual(positionTranslation.Z, handManger.TrackingTransform().translation.z, 0.01);
        }

    }
}

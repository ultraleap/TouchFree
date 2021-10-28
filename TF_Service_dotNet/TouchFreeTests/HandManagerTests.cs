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
            HandManager handManger = new HandManager(null);
            System.Numerics.Vector3 translationInMeters = new System.Numerics.Vector3(0.1f, 0.2f, 0.3f);
            Leap.Vector translationInLeapSpace = new Leap.Vector(translationInMeters.X, translationInMeters.Y, -translationInMeters.Z) * 1000;
            PhysicalConfig testConfig = new PhysicalConfig { LeapPositionRelativeToScreenBottomM = translationInMeters };

            // When 
            handManger.UpdateTrackingTransform(testConfig);

            // Then 
            Assert.IsTrue(handManger.TrackingTransform().translation == translationInLeapSpace);
        }

        [Test]
        public void TopMountedOrientationDoesNotInvertXRotation()
        {
            // Given
            HandManager handManger = new HandManager(null);
            System.Numerics.Vector3 topDownRotation = new System.Numerics.Vector3(45, 0, 180);
            System.Numerics.Quaternion topDownQuaternion = System.Numerics.Quaternion.CreateFromYawPitchRoll(
                VirtualScreen.DegreesToRadians(topDownRotation.Y), 
                VirtualScreen.DegreesToRadians(topDownRotation.X),
                VirtualScreen.DegreesToRadians(topDownRotation.Z));

            PhysicalConfig testConfig = new PhysicalConfig { LeapRotationD = topDownRotation };

            //When
            handManger.UpdateTrackingTransform(testConfig);

            //Then 
            System.Numerics.Quaternion handManagerRotation = new System.Numerics.Quaternion
            {
                X = handManger.TrackingTransform().rotation.x,
                Y = handManger.TrackingTransform().rotation.y,
                Z = handManger.TrackingTransform().rotation.z,
                W = handManger.TrackingTransform().rotation.w
            };
            Assert.IsTrue(handManagerRotation == topDownQuaternion);
        }

        [Test]
        public void BottomMountedOrientationInvertsXRotation()
        {
            // Given
            HandManager handManger = new HandManager(null);
            System.Numerics.Vector3 bottomRotation = new System.Numerics.Vector3(45, 0, 0);
            System.Numerics.Quaternion bottomQuaternion = System.Numerics.Quaternion.CreateFromYawPitchRoll(
                VirtualScreen.DegreesToRadians(bottomRotation.Y),
                VirtualScreen.DegreesToRadians(-bottomRotation.X),
                VirtualScreen.DegreesToRadians(bottomRotation.Z));

            PhysicalConfig testConfig = new PhysicalConfig { LeapRotationD = bottomRotation };

            //When
            handManger.UpdateTrackingTransform(testConfig);

            //Then 
            System.Numerics.Quaternion handManagerRotation = new System.Numerics.Quaternion
            {
                X = handManger.TrackingTransform().rotation.x,
                Y = handManger.TrackingTransform().rotation.y,
                Z = handManger.TrackingTransform().rotation.z,
                W = handManger.TrackingTransform().rotation.w
            };
            Assert.IsTrue(handManagerRotation == bottomQuaternion);
        }

    }
}

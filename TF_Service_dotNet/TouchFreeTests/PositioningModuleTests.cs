using System.Numerics;
using NUnit.Framework;
using TouchFreeTests.TestImplementations;
using Ultraleap.TouchFree.Library;
using Ultraleap.TouchFree.Library.Interactions;

namespace TouchFreeTests
{
    class PositioningModuleTests
    {
        PositioningModule sut;
        TestVirtualScreenManager virtualScreenManager = new TestVirtualScreenManager();

        public PositioningModuleTests()
        {
            sut = new PositioningModule(new TestPositionStabiliser(), Ultraleap.TouchFree.Library.TrackedPosition.INDEX_TIP, virtualScreenManager);
        }

        [TestCase(1000, 2000, 3000, 100, 200, 1, 250, 400, 3)]
        [TestCase(500, 1000, 2000, 100, 200, 1, 150, 200, 2)]
        [TestCase(500, 1000, 2000, 100, 200, 0.5f, 250, 400, 2)]
        [TestCase(500, 1000, 2000, 200, 300, 1, 250, 300, 2)]
        public void CalculatePositions_ValidHandPosition_Returns2dVector(float fingerTipX, float fingerTipY, float fingerTipZ, int screenWidthPx, int screenHeightPx, float screenPhysicalHeightM, float cursorX, float cursorY, float distanceFromScreen)
        {
            //Given
            Leap.Vector fingerTipPosition = new Leap.Vector(fingerTipX, fingerTipY, fingerTipZ);
            Leap.Finger finger = new Leap.Finger(0, 0, 0, 1, fingerTipPosition, new Leap.Vector(), 1, 1, true, Leap.Finger.FingerType.TYPE_INDEX, new Leap.Bone(), new Leap.Bone(), new Leap.Bone(), new Leap.Bone());
            Leap.Hand hand = new Leap.Hand();
            hand.Fingers.Add(finger);

            virtualScreenManager.virtualScreen = new VirtualScreen(screenWidthPx, screenHeightPx, screenPhysicalHeightM, 0, new TestConfigManager());

            //When
            Ultraleap.TouchFree.Library.Positions position = sut.CalculatePositions(hand);

            //Then
            Assert.AreEqual(cursorX, position.CursorPosition.X);
            Assert.AreEqual(cursorY, position.CursorPosition.Y);
            Assert.AreEqual(distanceFromScreen, position.DistanceFromScreen);
        }

        [TestCase(1000, 2000, 3000, 100, 200, 1, 250, 289.734192f, 3.30171967f)]
        [TestCase(500, 1000, 2000, 100, 200, 1, 150, 127.502274f, 2.14326358f)]
        [TestCase(500, 1000, 2000, 100, 200, 0.5f, 250, 255.004547f, 2.14326358f)]
        [TestCase(500, 1000, 2000, 200, 300, 1, 250, 191.253418f, 2.14326358f)]
        public void CalculatePositions_ValidHandPositionScreenAtAngle_Returns2dVector(float fingerTipX, float fingerTipY, float fingerTipZ, int screenWidthPx, int screenHeightPx, float screenPhysicalHeightM, float cursorX, float cursorY, float distanceFromScreen)
        {
            //Given
            Leap.Vector fingerTipPosition = new Leap.Vector(fingerTipX, fingerTipY, fingerTipZ);
            Leap.Finger finger = new Leap.Finger(0, 0, 0, 1, fingerTipPosition, new Leap.Vector(), 1, 1, true, Leap.Finger.FingerType.TYPE_INDEX, new Leap.Bone(), new Leap.Bone(), new Leap.Bone(), new Leap.Bone());
            Leap.Hand hand = new Leap.Hand();
            hand.Fingers.Add(finger);

            virtualScreenManager.virtualScreen = new VirtualScreen(screenWidthPx, screenHeightPx, screenPhysicalHeightM, 10, new TestConfigManager());

            //When
            Ultraleap.TouchFree.Library.Positions position = sut.CalculatePositions(hand);

            //Then
            Assert.AreEqual(cursorX, position.CursorPosition.X);
            Assert.AreEqual(cursorY, position.CursorPosition.Y);
            Assert.AreEqual(distanceFromScreen, position.DistanceFromScreen);
        }

        [Test]
        public void GetTrackedPointingJoint_ValidPointingIndexFinger_ReturnsAveragePlacementOfJoints()
        {
            //Given
            Leap.Vector firstJointPosition = new Leap.Vector(1000, 2000, -2990);
            Leap.Vector secondJointPosition = new Leap.Vector(1000, 2000, -3010);
            Leap.Finger finger = new Leap.Finger(0, 0, 0, 1, new Leap.Vector(), new Leap.Vector(), 1, 1, true, Leap.Finger.FingerType.TYPE_INDEX, new Leap.Bone(), new Leap.Bone(), new Leap.Bone(), new Leap.Bone());
            Leap.Hand hand = new Leap.Hand();
            hand.Fingers.Add(finger);
            finger.bones.SetValue(new Leap.Bone() { NextJoint = firstJointPosition }, 0);
            finger.bones.SetValue(new Leap.Bone() { NextJoint = secondJointPosition }, 1);

            //When
            Vector3 position = sut.GetTrackedPointingJoint(hand);

            //Then
            Assert.AreEqual(1f, position.X);
            Assert.AreEqual(2f, position.Y);
            Assert.AreEqual(3.0533f, position.Z);
            // Note there is an offset of 0.0533 from the average
        }

        private class TestPositionStabiliser : IPositionStabiliser
        {
            public float defaultDeadzoneRadius { get; set; }
            public float smoothingRate { get; set; }
            public float internalShrinkFactor { get; set; }
            public float currentDeadzoneRadius { get; set; }
            public bool isShrinking { get; set; }

            public Vector2 ApplyDeadzone(Vector2 position)
            {
                return position;
            }

            public Vector2 ApplyDeadzoneSized(Vector2 previous, Vector2 current, float radius)
            {
                throw new System.NotImplementedException();
            }

            public void ResetValues()
            {
            }

            public void StartShrinkingDeadzone(float speed)
            {
                throw new System.NotImplementedException();
            }

            public void StopShrinkingDeadzone()
            {
                throw new System.NotImplementedException();
            }
        }

        private class TestVirtualScreenManager : IVirtualScreenManager
        {
            public VirtualScreen virtualScreen { get; set; }
        }
    }
}

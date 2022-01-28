using System;
using System.Numerics;
using NUnit.Framework;
using TouchFreeTests.TestImplementations;
using Ultraleap.TouchFree.Library;
using Ultraleap.TouchFree.Library.Interactions;
using Ultraleap.TouchFree.Library.Interactions.PositionTrackers;

namespace TouchFreeTests
{
    class PositioningModuleTests
    {
        PositioningModule sut;
        TestVirtualScreenManager virtualScreenManager = new TestVirtualScreenManager();

        public PositioningModuleTests()
        {
            // TODO: Remove dependency on the Trackers here and instead use vector3 from mocked tracker instead of hand coordinates to make the tests simpler
            sut = new PositioningModule(new TestPositionStabiliser(), virtualScreenManager, 
                new IPositionTracker[] {
                    new IndexTipTracker(),
                    new IndexStableTracker(),
                    new NearestTracker(virtualScreenManager),
                    new WristTracker()
                });
            sut.TrackedPosition = TrackedPosition.INDEX_TIP;
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
            Assert.AreEqual(cursorX, position.CursorPosition.X, 0.001);
            Assert.AreEqual(cursorY, position.CursorPosition.Y, 0.001);
            Assert.AreEqual(distanceFromScreen, position.DistanceFromScreen, 0.001);
        }

        [TestCase(TrackedPosition.INDEX_TIP, typeof(IndexTipTracker))]
        [TestCase(TrackedPosition.INDEX_STABLE, typeof(IndexStableTracker))]
        [TestCase(TrackedPosition.NEAREST, typeof(NearestTracker))]
        [TestCase(TrackedPosition.WRIST, typeof(WristTracker))]
        public void TrackerToUse_TrackedPositionChanged_SetToExpectedTracker(TrackedPosition trackedPosition, Type expectedTracker)
        {
            //Given

            //When
            sut.TrackedPosition = trackedPosition;

            //Then
            Assert.AreEqual(expectedTracker, sut.TrackerToUse.GetType());
            Assert.AreEqual(trackedPosition, sut.TrackerToUse.TrackedPosition);
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
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Moq;
using NUnit.Framework;
using TouchFreeTests.TestImplementations;
using Ultraleap.TouchFree.Library;
using Ultraleap.TouchFree.Library.Interactions;
using Ultraleap.TouchFree.Library.Interactions.PositionTrackers;

namespace TouchFreeTests
{
    class PositioningModuleTests
    {
        TestVirtualScreenManager virtualScreenManager = new TestVirtualScreenManager();

        private PositioningModule CreatePositioningModule(IEnumerable<IPositionTracker> positionTrackers)
        {
            if (!positionTrackers.Any(x => x.TrackedPosition == TrackedPosition.INDEX_STABLE))
            {
                positionTrackers = positionTrackers.Append(new IndexStableTracker());
            }

            Mock<IPositionStabiliser> stabiliser = new Mock<IPositionStabiliser>();
            stabiliser.Setup(x => x.ApplyDeadzone(It.IsAny<Vector2>())).Returns<Vector2>(v => v);

            PositioningModule positioningModule = new PositioningModule(stabiliser.Object, virtualScreenManager,
                positionTrackers);
            positioningModule.TrackedPosition = TrackedPosition.INDEX_TIP;

            return positioningModule;
        }

        private IPositionTracker CreateTrackerWithPosition(TrackedPosition trackedPosition, float x, float y, float z)
        {
            Mock<IPositionTracker> mockTracker = new Mock<IPositionTracker>();
            mockTracker.SetupGet(x => x.TrackedPosition).Returns(trackedPosition);
            mockTracker.Setup(x => x.GetTrackedPosition(It.IsAny<Leap.Hand>())).Returns(new Vector3(x, y, z));
            return mockTracker.Object;
        }

        [TestCase(0, 0.5f, 3, 100, 200, 1, 50, 100, 3)]
        [TestCase(0.25f, 0.5f, 2, 100, 200, 1, 100, 100, 2)]
        [TestCase(0.25f, 0.5f, 2, 100, 200, 2f, 75, 50, 2)]
        [TestCase(0.25f, 0.5f, 2, 200, 300, 1, 175, 150, 2)]
        public void CalculatePositions_ValidHandPosition_Returns2dVector(float positionX, float positionY, float positionZ, int screenWidthPx, int screenHeightPx, float screenPhysicalHeightM, float cursorX, float cursorY, float distanceFromScreen)
        {
            //Given
            virtualScreenManager.virtualScreen = new VirtualScreen(screenWidthPx, screenHeightPx, screenPhysicalHeightM, 0, new TestConfigManager());
            IPositionTracker positionTracker = CreateTrackerWithPosition(TrackedPosition.INDEX_TIP, positionX, positionY, positionZ);
            PositioningModule positioningModule = CreatePositioningModule(new[] { positionTracker });

            //When
            Ultraleap.TouchFree.Library.Positions position = positioningModule.CalculatePositions(new Leap.Hand());

            //Then
            Assert.AreEqual(cursorX, position.CursorPosition.X);
            Assert.AreEqual(cursorY, position.CursorPosition.Y);
            Assert.AreEqual(distanceFromScreen, position.DistanceFromScreen);
        }

        [TestCase(0, 1f, 1, 100, 200, 1, 50, 162.23191f, 1.1584558f)]
        [TestCase(0.25f, 0.5f, 1, 100, 200, 1, 100, 63.751144f, 1.0716317f)]
        [TestCase(0.25f, 0.5f, 1, 100, 200, 2f, 75, 31.8755722f, 1.0716317f)]
        [TestCase(0.25f, 0.5f, 1, 200, 300, 1, 175, 95.62671f, 1.0716317f)]
        public void CalculatePositions_ValidHandPositionScreenAtAngle_Returns2dVector(float positionX, float positionY, float positionZ, int screenWidthPx, int screenHeightPx, float screenPhysicalHeightM, float cursorX, float cursorY, float distanceFromScreen)
        {
            //Given
            virtualScreenManager.virtualScreen = new VirtualScreen(screenWidthPx, screenHeightPx, screenPhysicalHeightM, 10, new TestConfigManager());
            IPositionTracker positionTracker = CreateTrackerWithPosition(TrackedPosition.INDEX_TIP, positionX, positionY, positionZ);
            PositioningModule positioningModule = CreatePositioningModule(new[] { positionTracker });

            //When
            Ultraleap.TouchFree.Library.Positions position = positioningModule.CalculatePositions(new Leap.Hand());

            //Then
            Assert.AreEqual(cursorX, position.CursorPosition.X, 0.001);
            Assert.AreEqual(cursorY, position.CursorPosition.Y, 0.001);
            Assert.AreEqual(distanceFromScreen, position.DistanceFromScreen, 0.001);
        }

        [Test]
        public void CalculatePositions_NoHand_ReturnLastPosition()
        {
            //Given
            virtualScreenManager.virtualScreen = new VirtualScreen(100, 200, 10, 0, new TestConfigManager());
            IPositionTracker positionTracker = CreateTrackerWithPosition(TrackedPosition.INDEX_TIP, 0, 1, 1);
            PositioningModule positioningModule = CreatePositioningModule(new[] { positionTracker });
            Ultraleap.TouchFree.Library.Positions oldPosition = positioningModule.CalculatePositions(new Leap.Hand());

            //When
            Ultraleap.TouchFree.Library.Positions position = positioningModule.CalculatePositions(null);

            //Then
            Assert.AreEqual(oldPosition.CursorPosition.X, position.CursorPosition.X, 0.001);
            Assert.AreEqual(oldPosition.CursorPosition.Y, position.CursorPosition.Y, 0.001);
            Assert.AreEqual(oldPosition.DistanceFromScreen, position.DistanceFromScreen, 0.001);
            Assert.AreEqual(50, position.CursorPosition.X, 0.001);
            Assert.AreEqual(20, position.CursorPosition.Y, 0.001);
            Assert.AreEqual(1, position.DistanceFromScreen, 0.001);
        }

        [TestCase(TrackedPosition.INDEX_TIP, typeof(IndexTipTracker))]
        [TestCase(TrackedPosition.INDEX_STABLE, typeof(IndexStableTracker))]
        [TestCase(TrackedPosition.NEAREST, typeof(NearestTracker))]
        [TestCase(TrackedPosition.WRIST, typeof(WristTracker))]
        public void TrackerToUse_TrackedPositionChanged_SetToExpectedTracker(TrackedPosition trackedPosition, Type expectedTracker)
        {
            //Given
            PositioningModule positioningModule = CreatePositioningModule(new IPositionTracker[] {
                    new IndexTipTracker(),
                    new IndexStableTracker(),
                    new NearestTracker(virtualScreenManager),
                    new WristTracker()
                });

            //When
            positioningModule.TrackedPosition = trackedPosition;

            //Then
            Assert.AreEqual(expectedTracker, positioningModule.TrackerToUse.GetType());
            Assert.AreEqual(trackedPosition, positioningModule.TrackerToUse.TrackedPosition);
        }
    }
}

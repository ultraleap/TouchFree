using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Moq;
using NUnit.Framework;
using Ultraleap.TouchFree.Library;
using Ultraleap.TouchFree.Library.Interactions;
using Ultraleap.TouchFree.Library.Interactions.PositionTrackers;

namespace TouchFreeTests
{
    class PositioningModuleTests
    {
        private Mock<IVirtualScreen> mockVirtualScreen = new Mock<IVirtualScreen>();
        private Mock<IPositionTracker> mockTracker = new Mock<IPositionTracker>();

        private PositioningModule CreatePositioningModule(IEnumerable<IPositionTracker> positionTrackers)
        {
            if (!positionTrackers.Any(x => x.TrackedPosition == TrackedPosition.INDEX_STABLE))
            {
                positionTrackers = positionTrackers.Append(new IndexStableTracker());
            }

            Mock<IPositionStabiliser> stabiliser = new();
            stabiliser.Setup(x => x.ApplyDeadzone(It.IsAny<Vector2>())).Returns<Vector2>(v => v);

            PositioningModule positioningModule = new PositioningModule(stabiliser.Object, mockVirtualScreen.Object, positionTrackers);
            positioningModule.TrackedPosition = TrackedPosition.INDEX_TIP;

            return positioningModule;
        }

        private void SetTrackerPosition(TrackedPosition trackedPosition, Vector3 trackerPositionInM)
        {
            mockTracker.SetupGet(x => x.TrackedPosition).Returns(trackedPosition);
            mockTracker.Setup(x => x.GetTrackedPosition(It.IsAny<Leap.Hand>())).Returns(trackerPositionInM);
        }

        [Test]
        public void CalculatePositions_Hand_ReturnPositionFromVirtualScreen()
        {
            //Given
            Vector3 worldPosition = new Vector3(1, 2, 3);
            Vector3 screenPosition = new Vector3(4, 5, 6);
            Vector2 screenPositionMm = new Vector2(7, 8);

            SetTrackerPosition(TrackedPosition.INDEX_TIP, worldPosition);

            PositioningModule positioningModule = CreatePositioningModule(new[] { mockTracker.Object });

            mockVirtualScreen.Setup(x => x.WorldPositionToVirtualScreen(worldPosition)).Returns(screenPosition);
            mockVirtualScreen.Setup(x => x.PixelsToMillimeters(It.Is<Vector2>(v => v.X == screenPosition.X && v.Y == screenPosition.Y))).Returns(screenPositionMm);
            mockVirtualScreen.Setup(x => x.MillimetersToPixels(screenPositionMm)).Returns(new Vector2(screenPosition.X, screenPosition.Y));

            Vector2 expectedHandPositionPx = new Vector2(4, 5);
            float expectedDistanceFromScreenM = 6;

            //When
            Ultraleap.TouchFree.Library.Positions position = positioningModule.CalculatePositions(new Leap.Hand());

            //Then
            Assert.AreEqual(expectedHandPositionPx.X, position.CursorPosition.X, 0.001);
            Assert.AreEqual(expectedHandPositionPx.Y, position.CursorPosition.Y, 0.001);
            Assert.AreEqual(expectedDistanceFromScreenM, position.DistanceFromScreen, 0.001);
        }

        [Test]
        public void CalculatePositions_NoHand_ReturnLastPosition()
        {
            //Given
            Vector3 worldPosition = new Vector3(1, 2, 3);
            Vector3 screenPosition = new Vector3(4, 5, 6);
            Vector2 screenPositionMm = new Vector2(7, 8);

            SetTrackerPosition(TrackedPosition.INDEX_TIP, worldPosition);

            PositioningModule positioningModule = CreatePositioningModule(new[] { mockTracker.Object });

            mockVirtualScreen.Setup(x => x.WorldPositionToVirtualScreen(worldPosition)).Returns(screenPosition);
            mockVirtualScreen.Setup(x => x.PixelsToMillimeters(It.Is<Vector2>(v => v.X == screenPosition.X && v.Y == screenPosition.Y))).Returns(screenPositionMm);
            mockVirtualScreen.Setup(x => x.MillimetersToPixels(screenPositionMm)).Returns(new Vector2(screenPosition.X, screenPosition.Y));

            Ultraleap.TouchFree.Library.Positions oldPosition = positioningModule.CalculatePositions(new Leap.Hand());

            Vector2 expectedHandPositionPx = new Vector2(4, 5);
            float expectedDistanceFromScreenM = 6;

            Vector3 newWorldPosition = new Vector3(11, 12, 13);
            SetTrackerPosition(TrackedPosition.INDEX_TIP, worldPosition);

            //When
            Ultraleap.TouchFree.Library.Positions position = positioningModule.CalculatePositions(null);

            //Then
            Assert.AreEqual(oldPosition.CursorPosition.X, position.CursorPosition.X, 0.01);
            Assert.AreEqual(oldPosition.CursorPosition.Y, position.CursorPosition.Y, 0.01);
            Assert.AreEqual(oldPosition.DistanceFromScreen, position.DistanceFromScreen, 0.01);
            Assert.AreEqual(expectedHandPositionPx.X, position.CursorPosition.X, 0.01);
            Assert.AreEqual(expectedHandPositionPx.Y, position.CursorPosition.Y, 0.01);
            Assert.AreEqual(expectedDistanceFromScreenM, position.DistanceFromScreen, 0.01);
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
                    new NearestTracker(),
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

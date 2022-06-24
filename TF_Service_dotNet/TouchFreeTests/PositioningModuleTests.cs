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

            PositioningModule positioningModule = new PositioningModule(mockVirtualScreen.Object, positionTrackers);

            return positioningModule;
        }

        private void SetTrackerPosition(TrackedPosition trackedPosition, Vector3 trackerPositionInM)
        {
            mockTracker.SetupGet(x => x.TrackedPosition).Returns(trackedPosition);
            mockTracker.Setup(x => x.GetTrackedPosition(It.IsAny<Leap.Hand>())).Returns(trackerPositionInM);
        }

        private Mock<IPositionTracker> CreatePositionTracker(TrackedPosition trackedPosition, Vector3 trackerPositionInM)
        {
            var newMockTracker = new Mock<IPositionTracker>();
            newMockTracker.SetupGet(x => x.TrackedPosition).Returns(trackedPosition);
            newMockTracker.Setup(x => x.GetTrackedPosition(It.IsAny<Leap.Hand>())).Returns(trackerPositionInM);

            return newMockTracker;
        }

        private IEnumerable<PositionTrackerConfiguration> CreateConfiguration(TrackedPosition trackedPosition)
        {
            return new[] { new PositionTrackerConfiguration(trackedPosition, 1) };
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
            Ultraleap.TouchFree.Library.Positions position = positioningModule.CalculatePositions(new Leap.Hand(), CreateConfiguration(TrackedPosition.INDEX_TIP));

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

            Ultraleap.TouchFree.Library.Positions oldPosition = positioningModule.CalculatePositions(new Leap.Hand(), CreateConfiguration(TrackedPosition.INDEX_TIP));

            Vector2 expectedHandPositionPx = new Vector2(4, 5);
            float expectedDistanceFromScreenM = 6;

            Vector3 newWorldPosition = new Vector3(11, 12, 13);
            SetTrackerPosition(TrackedPosition.INDEX_TIP, worldPosition);

            //When
            Ultraleap.TouchFree.Library.Positions position = positioningModule.CalculatePositions(null, CreateConfiguration(TrackedPosition.INDEX_TIP));

            //Then
            Assert.AreEqual(oldPosition.CursorPosition.X, position.CursorPosition.X, 0.01);
            Assert.AreEqual(oldPosition.CursorPosition.Y, position.CursorPosition.Y, 0.01);
            Assert.AreEqual(oldPosition.DistanceFromScreen, position.DistanceFromScreen, 0.01);
            Assert.AreEqual(expectedHandPositionPx.X, position.CursorPosition.X, 0.01);
            Assert.AreEqual(expectedHandPositionPx.Y, position.CursorPosition.Y, 0.01);
            Assert.AreEqual(expectedDistanceFromScreenM, position.DistanceFromScreen, 0.01);
        }

        [TestCase(TrackedPosition.INDEX_TIP, 1)]
        [TestCase(TrackedPosition.INDEX_STABLE, 2)]
        [TestCase(TrackedPosition.NEAREST, 3)]
        [TestCase(TrackedPosition.WRIST, 4)]
        public void TrackerToUse_TrackedPositionChanged_SetToExpectedTracker(TrackedPosition trackedPosition, float expectedReturnedXPosition)
        {
            //Given
            var trackers = new IPositionTracker[] {
                    CreatePositionTracker(TrackedPosition.INDEX_TIP, new Vector3(1, 0, 0)).Object,
                    CreatePositionTracker(TrackedPosition.INDEX_STABLE, new Vector3(2, 0, 0)).Object,
                    CreatePositionTracker(TrackedPosition.NEAREST, new Vector3(3, 0, 0)).Object,
                    CreatePositionTracker(TrackedPosition.WRIST, new Vector3(4, 0, 0)).Object
                };
            PositioningModule positioningModule = CreatePositioningModule(trackers);

            //When
            var result = positioningModule.GetPositionFromTracker(trackedPosition, null);

            //Then
            Assert.AreEqual(result.X, expectedReturnedXPosition);
        }
    }
}

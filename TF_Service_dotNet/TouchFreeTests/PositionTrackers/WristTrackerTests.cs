using NUnit.Framework;
using System.Numerics;
using Ultraleap.TouchFree.Library.Interactions.PositionTrackers;

namespace TouchFreeTests.PositionTrackers
{
    public class WristTrackerTests
    {
        WristTracker sut;

        public WristTrackerTests()
        {
            sut = new WristTracker();
        }

        private static object[] positionsData = new object[]
        {
            new object[] { new Vector3(1000, 2000, 3000), new Vector2(1, 2), 3 },
            new object[] { new Vector3(500, 1000, 2000), new Vector2(0.5f, 1), 2 }
        };

        [TestCaseSource(nameof(positionsData))]
        public void CalculatePositions_ValidHandPosition_Returns2dVector(Vector3 wristPositionMm, Vector2 expectedPositionM, float distanceFromScreenM)
        {
            //Given
            Leap.Hand hand = new Leap.Hand();
            hand.WristPosition = new Leap.Vector(wristPositionMm.X, wristPositionMm.Y, wristPositionMm.Z);

            //When
            Vector3 position = sut.GetTrackedPosition(hand);

            //Then
            Assert.AreEqual(expectedPositionM.X, position.X);
            Assert.AreEqual(expectedPositionM.Y, position.Y);
            Assert.AreEqual(distanceFromScreenM, position.Z);
        }
    }
}

using System.Numerics;
using NUnit.Framework;
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

        [TestCase(1000, 2000, 3000, 1, 2, 3)]
        [TestCase(500, 1000, 2000, 0.5f, 1, 2)]
        public void CalculatePositions_ValidHandPosition_Returns2dVector(float fingerTipX, float fingerTipY, float fingerTipZ, float cursorX, float cursorY, float distanceFromScreen)
        {
            //Given
            Leap.Hand hand = new Leap.Hand();
            hand.WristPosition = new Leap.Vector(fingerTipX, fingerTipY, fingerTipZ);

            //When
            Vector3 position = sut.GetTrackedPosition(hand);

            //Then
            Assert.AreEqual(cursorX, position.X);
            Assert.AreEqual(cursorY, position.Y);
            Assert.AreEqual(distanceFromScreen, position.Z);
        }
    }
}

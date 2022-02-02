using System.Numerics;
using NUnit.Framework;
using Ultraleap.TouchFree.Library.Interactions.PositionTrackers;

namespace TouchFreeTests.PositionTrackers
{
    public class IndexTipTrackerTests
    {
        IndexTipTracker sut;

        public IndexTipTrackerTests()
        {
            sut = new IndexTipTracker();
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
            Leap.Vector fingerTipPosition = new Leap.Vector(wristPositionMm.X, wristPositionMm.Y, wristPositionMm.Z);
            Leap.Finger finger = new Leap.Finger(0, 0, 0, 1, fingerTipPosition, new Leap.Vector(), 1, 1, true, Leap.Finger.FingerType.TYPE_INDEX, new Leap.Bone(), new Leap.Bone(), new Leap.Bone(), new Leap.Bone());
            Leap.Hand hand = new Leap.Hand();
            hand.Fingers.Add(finger);

            //When
            Vector3 position = sut.GetTrackedPosition(hand);

            //Then
            Assert.AreEqual(expectedPositionM.X, position.X);
            Assert.AreEqual(expectedPositionM.Y, position.Y);
            Assert.AreEqual(distanceFromScreenM, position.Z);
        }
    }
}

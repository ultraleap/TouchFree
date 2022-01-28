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

        [TestCase(1000, 2000, 3000, 1, 2, 3)]
        [TestCase(500, 1000, 2000, 0.5f, 1, 2)]
        public void CalculatePositions_ValidHandPosition_Returns2dVector(float fingerTipX, float fingerTipY, float fingerTipZ, float cursorX, float cursorY, float distanceFromScreen)
        {
            //Given
            Leap.Vector fingerTipPosition = new Leap.Vector(fingerTipX, fingerTipY, fingerTipZ);
            Leap.Finger finger = new Leap.Finger(0, 0, 0, 1, fingerTipPosition, new Leap.Vector(), 1, 1, true, Leap.Finger.FingerType.TYPE_INDEX, new Leap.Bone(), new Leap.Bone(), new Leap.Bone(), new Leap.Bone());
            Leap.Hand hand = new Leap.Hand();
            hand.Fingers.Add(finger);

            //When
            Vector3 position = sut.GetTrackedPosition(hand);

            //Then
            Assert.AreEqual(cursorX, position.X);
            Assert.AreEqual(cursorY, position.Y);
            Assert.AreEqual(distanceFromScreen, position.Z);
        }
    }
}

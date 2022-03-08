using NUnit.Framework;
using Ultraleap.TouchFree.Library;
using Ultraleap.TouchFree.Library.Interactions.PositionTrackers;

namespace TouchFreeTests.PositionTrackers
{
    internal class NearestTrackerTests
    {
        NearestTracker sut;

        public NearestTrackerTests()
        {
            sut = new NearestTracker();
        }

        [Test]
        public void GetTrackedPointingJoint_ValidPointingIndexFinger_ReturnsNearestJoint()
        {
            //Given
            Leap.Vector firstJointPosition = new Leap.Vector(1000, 2000, 2990);
            Leap.Vector secondJointPosition = new Leap.Vector(1000, 2000, 3010);
            Leap.Finger finger = new Leap.Finger(0, 0, 0, 1, new Leap.Vector(), new Leap.Vector(), 1, 1, true, Leap.Finger.FingerType.TYPE_INDEX, new Leap.Bone(), new Leap.Bone(), new Leap.Bone(), new Leap.Bone());
            Leap.Hand hand = new Leap.Hand();
            hand.Fingers.Clear();
            hand.Fingers.Add(finger);
            finger.bones = new Leap.Bone[] {
                new Leap.Bone() { NextJoint = firstJointPosition },
                new Leap.Bone() { NextJoint = secondJointPosition }
            };
            Vector3 expectedPositionM = new Vector3(1, 2, 2.99f);

            //When
            Vector3 position = sut.GetTrackedPosition(hand);

            //Then
            Assert.AreEqual(expectedPositionM.X, position.X);
            Assert.AreEqual(expectedPositionM.Y, position.Y);
            Assert.AreEqual(expectedPositionM.Z, position.Z);
        }
    }
}

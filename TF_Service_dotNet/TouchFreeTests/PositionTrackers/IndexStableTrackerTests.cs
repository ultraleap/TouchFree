using System.Numerics;
using NUnit.Framework;
using Ultraleap.TouchFree.Library.Interactions.PositionTrackers;

namespace TouchFreeTests.PositionTrackers
{
    class IndexStableTrackerTests
    {
        IndexStableTracker sut;

        public IndexStableTrackerTests()
        {
            sut = new IndexStableTracker();
        }

        [Test]
        public void GetTrackedPointingJoint_ValidPointingIndexFinger_ReturnsAveragePlacementOfFirstTwoJoints()
        {
            //Given
            Leap.Vector firstJointPosition = new Leap.Vector(1000, 2000, 2990);
            Leap.Vector secondJointPosition = new Leap.Vector(1000, 2000, 3010);
            Leap.Vector thirdJointPosition = new Leap.Vector(1000, 2000, 3030);
            Leap.Vector fourthJointPosition = new Leap.Vector(1000, 2000, 3050);
            Leap.Finger finger = new Leap.Finger(0, 0, 0, 1, new Leap.Vector(), new Leap.Vector(), 1, 1, true, Leap.Finger.FingerType.TYPE_INDEX, 
                new Leap.Bone() { NextJoint = firstJointPosition }, 
                new Leap.Bone() { NextJoint = secondJointPosition }, 
                new Leap.Bone() { NextJoint = thirdJointPosition }, 
                new Leap.Bone() { NextJoint = fourthJointPosition });
            Leap.Hand hand = new Leap.Hand();
            hand.Fingers.Add(finger);

            Vector3 expectedPositionM = new Vector3(1, 2, 2.9467f);
            // Note there is an offset of 0.0533 for the Z position from the average

            //When
            Vector3 position = sut.GetTrackedPosition(hand);

            //Then
            Assert.AreEqual(expectedPositionM.X, position.X);
            Assert.AreEqual(expectedPositionM.Y, position.Y);
            Assert.AreEqual(expectedPositionM.Z, position.Z);
        }
    }
}

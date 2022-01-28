using System.Numerics;
using NUnit.Framework;
using TouchFreeTests.TestImplementations;
using Ultraleap.TouchFree.Library;
using Ultraleap.TouchFree.Library.Interactions.PositionTrackers;

namespace TouchFreeTests.PositionTrackers
{
    internal class NearestTrackerTests
    {
        NearestTracker sut;
        TestVirtualScreenManager virtualScreenManager = new TestVirtualScreenManager();

        public NearestTrackerTests()
        {
            sut = new NearestTracker(virtualScreenManager);

            virtualScreenManager.virtualScreen = new VirtualScreen(100, 100, 1, 0, new TestConfigManager());
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

            //When
            Vector3 position = sut.GetTrackedPosition(hand);

            //Then
            Assert.AreEqual(1f, position.X);
            Assert.AreEqual(2f, position.Y);
            Assert.AreEqual(2.99f, position.Z);
        }
    }
}

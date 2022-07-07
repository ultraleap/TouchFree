using NUnit.Framework;
using Ultraleap.TouchFree.Library;

namespace TouchFreeTests
{
    public class TrackingConnectionManagerTests
    {
        [TestCase(TrackingMode.SCREENTOP, true, false)]
        [TestCase(TrackingMode.HMD, false, true)]
        [TestCase(TrackingMode.DESKTOP, false, false)]
        public void TrackingModeIsIncorrect_CorrectMode_ReturnsFalse(TrackingMode expectedMode, bool inScreenTop, bool inHmd)
        {
            // Arrange

            // Act
            var result = TrackingConnectionManager.TrackingModeIsIncorrect(expectedMode, inScreenTop, inHmd);

            // Assert
            Assert.AreEqual(false, result);
        }

        [TestCase(TrackingMode.HMD, true, false)]
        [TestCase(TrackingMode.DESKTOP, true, false)]
        [TestCase(TrackingMode.SCREENTOP, false, true)]
        [TestCase(TrackingMode.DESKTOP, false, true)]
        [TestCase(TrackingMode.SCREENTOP, false, false)]
        [TestCase(TrackingMode.HMD, false, false)]
        public void TrackingModeIsIncorrect_IncorrectMode_ReturnsTrue(TrackingMode expectedMode, bool inScreenTop, bool inHmd)
        {
            // Arrange

            // Act
            var result = TrackingConnectionManager.TrackingModeIsIncorrect(expectedMode, inScreenTop, inHmd);

            // Assert
            Assert.AreEqual(true, result);
        }
    }
}

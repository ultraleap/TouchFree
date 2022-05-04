using NUnit.Framework;
using Ultraleap.TouchFree.Library;

namespace TouchFreeTests
{
    public class TrackingConnectionManagerTests
    {
        [TestCase(TrackingConnectionManager.TrackingMode.SCREENTOP, true, false)]
        [TestCase(TrackingConnectionManager.TrackingMode.HMD, false, true)]
        [TestCase(TrackingConnectionManager.TrackingMode.DESKTOP, false, false)]
        public void TrackingModeIsIncorrect_CorrectMode_ReturnsFalse(TrackingConnectionManager.TrackingMode expectedMode, bool inScreenTop, bool inHmd)
        {
            // Arrange

            // Act
            var result = TrackingConnectionManager.TrackingModeIsIncorrect(expectedMode, inScreenTop, inHmd);

            // Assert
            Assert.AreEqual(false, result);
        }

        [TestCase(TrackingConnectionManager.TrackingMode.HMD, true, false)]
        [TestCase(TrackingConnectionManager.TrackingMode.DESKTOP, true, false)]
        [TestCase(TrackingConnectionManager.TrackingMode.SCREENTOP, false, true)]
        [TestCase(TrackingConnectionManager.TrackingMode.DESKTOP, false, true)]
        [TestCase(TrackingConnectionManager.TrackingMode.SCREENTOP, false, false)]
        [TestCase(TrackingConnectionManager.TrackingMode.HMD, false, false)]
        public void TrackingModeIsIncorrect_IncorrectMode_ReturnsTrue(TrackingConnectionManager.TrackingMode expectedMode, bool inScreenTop, bool inHmd)
        {
            // Arrange

            // Act
            var result = TrackingConnectionManager.TrackingModeIsIncorrect(expectedMode, inScreenTop, inHmd);

            // Assert
            Assert.AreEqual(true, result);
        }
    }
}

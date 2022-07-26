using Moq;
using NUnit.Framework;
using System;
using System.Numerics;
using Ultraleap.TouchFree.Library;
using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Library.Configuration.QuickSetup;

namespace TouchFreeTests.Configuration
{
    public class QuickSetupHandlerTests
    {
        private Mock<IHandManager> handManager;
        private Mock<ITrackingConnectionManager> trackingConnectionManager;
        private Mock<IConfigManager> configManager;

        #region LeapRotationRelativeToScreen
        [Test]
        public void LeapRotationRelativeToScreen_Vertical_ReturnsRotationsOfZero()
        {
            // Arrange
            var bottomCentre = new Vector3(0, 20, 0);
            var topCentre = new Vector3(0, 80, 0);
            var sut = CreateSut();

            // Act
            var result = sut.LeapRotationRelativeToScreen(bottomCentre, topCentre);

            // Assert
            Assert.AreEqual(0f, result.X);
            Assert.AreEqual(0f, result.Y);
            Assert.AreEqual(0f, result.Z);
        }

        [Test]
        public void LeapRotationRelativeToScreen_Horizontal_ReturnsRotationOfMinusNinety()
        {
            // Arrange
            var bottomCentre = new Vector3(0, 0, 20);
            var topCentre = new Vector3(0, 0, 80);
            var sut = CreateSut();

            // Act
            var result = sut.LeapRotationRelativeToScreen(bottomCentre, topCentre);

            // Assert
            Assert.AreEqual(-90f, result.X);
            Assert.AreEqual(0f, result.Y);
            Assert.AreEqual(0f, result.Z);
        }

        [Test]
        public void LeapRotationRelativeToScreen_ScreentopCameraWithVerticalScreen_ReturnsRotationOf180XAndZ()
        {
            // Arrange
            var bottomCentre = new Vector3(0, 20, 0);
            var topCentre = new Vector3(0, 80, 0);
            var sut = CreateSut();
            trackingConnectionManager.SetupGet(x => x.CurrentTrackingMode).Returns(TrackingMode.SCREENTOP);

            // Act
            var result = sut.LeapRotationRelativeToScreen(bottomCentre, topCentre);

            // Assert
            Assert.AreEqual(180f, result.X);
            Assert.AreEqual(0f, result.Y);
            Assert.AreEqual(180f, result.Z);
        }

        [Test]
        public void LeapRotationRelativeToScreen_ScreentopCameraWithHorizontalScreen_ReturnsRotationOf90XAnd180Z()
        {
            // Arrange
            var bottomCentre = new Vector3(0, 0, 20);
            var topCentre = new Vector3(0, 0, 80);
            var sut = CreateSut();
            trackingConnectionManager.SetupGet(x => x.CurrentTrackingMode).Returns(TrackingMode.SCREENTOP);

            // Act
            var result = sut.LeapRotationRelativeToScreen(bottomCentre, topCentre);

            // Assert
            Assert.AreEqual(90f, result.X);
            Assert.AreEqual(0f, result.Y);
            Assert.AreEqual(180f, result.Z);
        }
        #endregion

        #region LeapPositionInScreenSpace
        [Test]
        public void LeapPositionInScreenSpace_AtBottomOfScreen_ReturnsZero()
        {
            // Arrange
            var bottomCentre = new Vector3(0, 0, 0);
            var rotation = new Vector3(0, 0, 0);
            var sut = CreateSut();

            // Act
            var result = sut.LeapPositionInScreenSpace(bottomCentre, rotation);

            // Assert
            Assert.AreEqual(0f, result.X);
            Assert.AreEqual(0f, result.Y);
            Assert.AreEqual(0f, result.Z);
        }

        [Test]
        public void LeapPositionInScreenSpace_MinusOneZAwayFromBottomOfScreen_ReturnsOneZ()
        {
            // Arrange
            var bottomCentre = new Vector3(0, 0, -1);
            var rotation = new Vector3(0, 0, 0);
            var sut = CreateSut();

            // Act
            var result = sut.LeapPositionInScreenSpace(bottomCentre, rotation);

            // Assert
            Assert.AreEqual(0f, result.X);
            Assert.AreEqual(0f, result.Y);
            Assert.AreEqual(1f, result.Z);
        }

        [Test]
        public void LeapPositionInScreenSpace_CameraAt45DegreesAndAt45DegreesFromBottomOfScreen_ReturnsRoot2Z()
        {
            // Arrange
            var bottomCentre = new Vector3(0, -1, -1);
            var rotation = new Vector3(45, 0, 0);
            var sut = CreateSut();

            // Act
            var result = sut.LeapPositionInScreenSpace(bottomCentre, rotation);

            // Assert
            Assert.AreEqual(0f, result.X);
            Assert.AreEqual(0f, result.Y, 0.00001f);
            Assert.AreEqual(Math.Sqrt(2), result.Z, 0.00001f);
        }
        #endregion

        private QuickSetupHandler CreateSut()
        {
            handManager = new Mock<IHandManager>();
            trackingConnectionManager = new Mock<ITrackingConnectionManager>();
            configManager = new Mock<IConfigManager>();
            return new QuickSetupHandler(handManager.Object, trackingConnectionManager.Object, configManager.Object);
        }
    }
}

using System.Numerics;
using NUnit.Framework;
using TouchFreeTests.TestImplementations;
using Ultraleap.TouchFree.Library;
using Ultraleap.TouchFree.Library.Configuration;

namespace TouchFreeTests
{
    public class VirtualScreenTests
    {
        [Test]
        public void Constructor_ValidInputs_ReturnsInstance()
        {
            //Given
            IConfigManager configManager = new TestConfigManager();

            //When
            VirtualScreen virtualScreen = new VirtualScreen(1080, 1920, 0.4f, 0, configManager);

            //Then
            Assert.AreEqual(1080, virtualScreen.Width_VirtualPx);
            Assert.AreEqual(1920, virtualScreen.Height_VirtualPx);
            Assert.AreEqual(0.4f, virtualScreen.Height_PhysicalMeters);
            Assert.AreEqual(0.225f, virtualScreen.Width_PhysicalMeters, 0.001);
            Assert.AreEqual(0, virtualScreen.AngleOfPhysicalScreen_Degrees);
            Assert.AreEqual(0, virtualScreen.ScreenPlane.Normal.X);
            Assert.AreEqual(0, virtualScreen.ScreenPlane.Normal.Y);
            Assert.AreEqual(1, virtualScreen.ScreenPlane.Normal.Z);
        }

        private int ScreenWidthInPixels = 1080;
        private int ScreenHeightInPixels = 1920;
        private float ScreenHeightInMeters = 0.4f;

        private VirtualScreen CreateVirtualScreen(float angleInDegrees)
        {
            IConfigManager configManager = new TestConfigManager();
            return new VirtualScreen(ScreenWidthInPixels, ScreenHeightInPixels, ScreenHeightInMeters, angleInDegrees, configManager);
        }

        [TestCase(480, 960, 0.1f, 0.2f)]
        [TestCase(0, 540, 0f, 0.1125f)]
        public void PixelsToMeters_ConvertsPixelPositionToMeters(int pixelX, int pixelY, float metersX, float metersY)
        {
            //Given
            VirtualScreen virtualScreen = CreateVirtualScreen(0);
            Vector2 pixelPosition = new Vector2(pixelX, pixelY);

            //When
            Vector2 meterPosition = virtualScreen.PixelsToMeters(pixelPosition);

            //Then
            Assert.AreEqual(metersX, meterPosition.X, 0.0001);
            Assert.AreEqual(metersY, meterPosition.Y, 0.0001);
        }

        [TestCase(0.1f, 0.2f, 480, 960)]
        [TestCase(0f, 0.1125f, 0, 540)]
        public void MetersToPixels_ConvertsMeterPositionToPixels(float metersX, float metersY, int pixelX, int pixelY)
        {
            //Given
            VirtualScreen virtualScreen = CreateVirtualScreen(0);
            Vector2 meterPosition = new Vector2(metersX, metersY);

            //When
            Vector2 pixelPosition = virtualScreen.MetersToPixels(meterPosition);

            //Then
            Assert.AreEqual(pixelX, pixelPosition.X, 0.0001);
            Assert.AreEqual(pixelY, pixelPosition.Y, 0.0001);
        }

        [TestCase(1, 0, 1, 0, 1)]
        [TestCase(1, 1, 1, 0, 1)]
        [TestCase(0, 1, 1, 0, 1)]
        [TestCase(1, 0, 1, 45, 0.707f)]
        [TestCase(1, 1, 1, 45, 1.414f)]
        [TestCase(0, 1, 1, 45, 1.414f)]
        public void DistanceFromScreenPlane_IsPositiveDistanceFromScreenPlane_ReturnsDistance(float x, float y, float z, float screenAngle, float expectedDistance)
        {
            //Given
            VirtualScreen virtualScreen = CreateVirtualScreen(screenAngle);
            Vector3 worldPosition = new Vector3(x, y, z);

            //When
            var distance = virtualScreen.DistanceFromScreenPlane(worldPosition);

            //Then
            Assert.AreEqual(expectedDistance, distance, 0.001);
        }

        [Test]
        public void WorldPositionToVirtualScreen_ZeroedCoordinates_ReturnsCenterBottomOfScreen()
        {
            //Given
            VirtualScreen virtualScreen = CreateVirtualScreen(angleInDegrees: 0);
            Vector3 worldPosition = new Vector3(0, 0, 1);

            //When
            var screenPosition = virtualScreen.WorldPositionToVirtualScreen(worldPosition, out var planeHitWorldPosition);

            //Then
            Assert.AreEqual(540, screenPosition.X);
            Assert.AreEqual(0, screenPosition.Y);
            Assert.AreEqual(1, screenPosition.Z);
        }

        [TestCase(0.1f, 0, 1, 1020, 0, 1)]
        [TestCase(-0.1f, 0.1f, 1, 60, 480, 1)]
        [TestCase(0, 0.15f, 2, 540, 720, 2)]
        public void WorldPositionToVirtualScreen_UnangledScreen_ReturnsMappedPosition(float x, float y, float z, float expectedX, float expectedY, float expectedZ)
        {
            //Given
            VirtualScreen virtualScreen = CreateVirtualScreen(angleInDegrees: 0);
            Vector3 worldPosition = new Vector3(x, y, z);

            //When
            var screenPosition = virtualScreen.WorldPositionToVirtualScreen(worldPosition, out var planeHitWorldPosition);

            //Then
            Assert.AreEqual(expectedX, screenPosition.X, 0.001);
            Assert.AreEqual(expectedY, screenPosition.Y, 0.001);
            Assert.AreEqual(expectedZ, screenPosition.Z, 0.001);
        }

        [TestCase(0.1f, 0, 0, 1020, 0, 0)]
        [TestCase(0, 0.1f, 0, 540, 415.692f, 0.05f)]
        [TestCase(0, 0.1f, 0.05f, 540, 295.692f, 0.093f)]
        public void WorldPositionToVirtualScreen_AngledScreen_ReturnsMappedPosition(float x, float y, float z, float expectedX, float expectedY, float expectedZ)
        {
            //Given
            VirtualScreen virtualScreen = CreateVirtualScreen(angleInDegrees: 30);
            Vector3 worldPosition = new Vector3(x, y, z);

            //When
            var screenPosition = virtualScreen.WorldPositionToVirtualScreen(worldPosition, out var planeHitWorldPosition);

            //Then
            Assert.AreEqual(expectedX, screenPosition.X, 0.001);
            Assert.AreEqual(expectedY, screenPosition.Y, 0.001);
            Assert.AreEqual(expectedZ, screenPosition.Z, 0.001);
        }
    }
}

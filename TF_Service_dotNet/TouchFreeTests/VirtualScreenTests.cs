using System.Numerics;
using Moq;
using NUnit.Framework;
using Ultraleap.TouchFree.Library;
using Ultraleap.TouchFree.Library.Configuration;

namespace TouchFreeTests
{
    public class VirtualScreenTests
    {

        private IConfigManager CreateMockedConfigManager(PhysicalConfig physicalConfig)
        {
            Mock<IConfigManager> mockConfigManager = new Mock<IConfigManager>();
            mockConfigManager.SetupGet(x => x.InteractionConfig).Returns(new InteractionConfig());
            mockConfigManager.SetupGet(x => x.PhysicalConfig).Returns(physicalConfig);
            return mockConfigManager.Object;
        }

        [Test]
        public void Constructor_ValidInputs_ReturnsInstance()
        {
            //Given
            IConfigManager configManager = CreateMockedConfigManager(new PhysicalConfig()
            {
                ScreenWidthPX = 1080, 
                ScreenHeightPX = 1920,
                ScreenHeightM = 0.4f,
                ScreenRotationD = 0
            });

            //When
            VirtualScreen virtualScreen = new VirtualScreen(configManager);

            //Then
            Assert.AreEqual(1080, virtualScreen.Width_VirtualPx);
            Assert.AreEqual(1920, virtualScreen.Height_VirtualPx);
            Assert.AreEqual(0.4f, virtualScreen.Height_PhysicalMeters);
            Assert.AreEqual(0.225f, virtualScreen.Width_PhysicalMeters, 0.001);
        }

        private int ScreenWidthInPixels = 1080;
        private int ScreenHeightInPixels = 1920;
        private float ScreenHeightInMeters = 0.4f;

        private VirtualScreen CreateVirtualScreen()
        {
            var physicalConfig = new PhysicalConfig()
            {
                ScreenWidthPX = ScreenWidthInPixels,
                ScreenHeightPX = ScreenHeightInPixels,
                ScreenHeightM = ScreenHeightInMeters
            };
            IConfigManager configManager = CreateMockedConfigManager(physicalConfig);

            return new VirtualScreen(configManager);
        }

        private static object[] pixelsToMetersCases = new object[]
        {
            new[] { new Vector2 (480, 960), new Vector2 (0.1f, 0.2f) },
            new[] { new Vector2 (0, 540), new Vector2 (0f, 0.1125f) }
        };

        [TestCaseSource(nameof(pixelsToMetersCases))]
        public void PixelsToMeters_ConvertsPixelPositionToMeters(Vector2 positionPx, Vector2 expectedPositionM)
        {
            //Given
            VirtualScreen virtualScreen = CreateVirtualScreen();

            //When
            Vector2 meterPosition = virtualScreen.PixelsToMeters(positionPx);

            //Then
            Assert.AreEqual(expectedPositionM.X, meterPosition.X, 0.0001);
            Assert.AreEqual(expectedPositionM.Y, meterPosition.Y, 0.0001);
        }

        private static object[] metersToPixelsCases = new object[]
        {
            new[] { new Vector2 (0.1f, 0.2f), new Vector2 (480, 960) },
            new[] { new Vector2(0f, 0.1125f), new Vector2 (0, 540) }
        };

        [TestCaseSource(nameof(metersToPixelsCases))]
        public void MetersToPixels_ConvertsMeterPositionToPixels(Vector2 positionM, Vector2 expectedPositionPx)
        {
            //Given
            VirtualScreen virtualScreen = CreateVirtualScreen();

            //When
            Vector2 pixelPosition = virtualScreen.MetersToPixels(positionM);

            //Then
            Assert.AreEqual(expectedPositionPx.X, pixelPosition.X, 0.0001);
            Assert.AreEqual(expectedPositionPx.Y, pixelPosition.Y, 0.0001);
        }

        [TestCase(0.1f, 480)]
        [TestCase(0, 0)]
        public void MetersToPixels_ConvertsMeterPositionToPixels(float positionM, float expectedPositionPx)
        {
            //Given
            VirtualScreen virtualScreen = CreateVirtualScreen();

            //When
            float pixelPosition = virtualScreen.MetersToPixels(positionM);

            //Then
            Assert.AreEqual(expectedPositionPx, pixelPosition, 0.0001);
        }

        private static object[] distanceFromScreenPlaneCases = new object[]
        {
            new object[] { new Vector3(1, 0, 1), 1 },
            new object[] { new Vector3(1, 1, 1), 1 },
            new object[] { new Vector3(0, 1, 1), 1 }
        };

        [TestCaseSource(nameof(distanceFromScreenPlaneCases))]
        public void DistanceFromScreenPlane_IsPositiveDistanceFromScreenPlane_ReturnsDistance(Vector3 worldPositionM, float expectedDistanceM)
        {
            //Given
            VirtualScreen virtualScreen = CreateVirtualScreen();

            //When
            var distance = virtualScreen.DistanceFromScreenPlane(worldPositionM);

            //Then
            Assert.AreEqual(expectedDistanceM, distance, 0.001);
        }

        [Test]
        public void WorldPositionToVirtualScreen_ZeroedCoordinates_ReturnsCenterBottomOfScreen()
        {
            //Given
            VirtualScreen virtualScreen = CreateVirtualScreen();
            Vector3 worldPosition = new Vector3(0, 0, 1);

            //When
            var screenPosition = virtualScreen.WorldPositionToVirtualScreen(worldPosition);

            //Then
            Assert.AreEqual(540, screenPosition.X);
            Assert.AreEqual(0, screenPosition.Y);
            Assert.AreEqual(1, screenPosition.Z);
        }

        private static object[] worldPositionToVirtualScreenUnangledScreenCases = new object[]
        {
            new object[] { new Vector3(0.1f, 0, 1), new Vector3(1020, 0, 1) },
            new object[] { new Vector3(-0.1f, 0.1f, 1), new Vector3(60, 480, 1) },
            new object[] { new Vector3(0, 0.15f, 2), new Vector3(540, 720, 2) },
        };

        [TestCaseSource(nameof(worldPositionToVirtualScreenUnangledScreenCases))]
        public void WorldPositionToVirtualScreen_ReturnsMappedPosition(Vector3 worldPositionM, Vector3 expectedScreenPosition)
        {
            //Given
            VirtualScreen virtualScreen = CreateVirtualScreen();

            //When
            var screenPosition = virtualScreen.WorldPositionToVirtualScreen(worldPositionM);

            //Then
            Assert.AreEqual(expectedScreenPosition.X, screenPosition.X, 0.001);
            Assert.AreEqual(expectedScreenPosition.Y, screenPosition.Y, 0.001);
            Assert.AreEqual(expectedScreenPosition.Z, screenPosition.Z, 0.001);
        }
    }
}

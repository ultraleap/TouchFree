using Moq;
using NUnit.Framework;
using System.Numerics;
using Ultraleap.TouchFree.Library;
using Ultraleap.TouchFree.Library.Configuration;

namespace TouchFreeTests
{
    public class VirtualScreenTests
    {

        private IConfigManager CreateMockedConfigManager(PhysicalConfigInternal physicalConfig)
        {
            Mock<IConfigManager> mockConfigManager = new Mock<IConfigManager>();
            mockConfigManager.SetupGet(x => x.InteractionConfig).Returns(new InteractionConfigInternal());
            mockConfigManager.SetupGet(x => x.PhysicalConfig).Returns(physicalConfig);
            return mockConfigManager.Object;
        }

        [Test]
        public void Constructor_ValidInputs_ReturnsInstance()
        {
            //Given
            IConfigManager configManager = CreateMockedConfigManager(new PhysicalConfigInternal()
            {
                ScreenWidthPX = 1080,
                ScreenHeightPX = 1920,
                ScreenHeightMm = 400f,
                ScreenRotationD = 0
            });

            //When
            VirtualScreen virtualScreen = new VirtualScreen(configManager);

            //Then
            Assert.AreEqual(1080, virtualScreen.Width_VirtualPx);
            Assert.AreEqual(1920, virtualScreen.Height_VirtualPx);
            Assert.AreEqual(400f, virtualScreen.Height_PhysicalMillimeters);
            Assert.AreEqual(225f, virtualScreen.Width_PhysicalMillimeters, 0.01);
        }

        private int ScreenWidthInPixels = 1080;
        private int ScreenHeightInPixels = 1920;
        private float ScreenHeightInMillimeters = 400f;

        private VirtualScreen CreateVirtualScreen()
        {
            var physicalConfig = new PhysicalConfigInternal()
            {
                ScreenWidthPX = ScreenWidthInPixels,
                ScreenHeightPX = ScreenHeightInPixels,
                ScreenHeightMm = ScreenHeightInMillimeters
            };
            IConfigManager configManager = CreateMockedConfigManager(physicalConfig);

            return new VirtualScreen(configManager);
        }

        private static object[] pixelsToMillimetersCases = new object[]
        {
            new[] { new Vector2 (480, 960), new Vector2 (100f, 200f) },
            new[] { new Vector2 (0, 540), new Vector2 (0f, 112.5f) }
        };

        [TestCaseSource(nameof(pixelsToMillimetersCases))]
        public void PixelsToMillimeters_ConvertsPixelPositionToMillimeters(Vector2 positionPx, Vector2 expectedPositionM)
        {
            //Given
            VirtualScreen virtualScreen = CreateVirtualScreen();

            //When
            Vector2 meterPosition = virtualScreen.PixelsToMillimeters(positionPx);

            //Then
            Assert.AreEqual(expectedPositionM.X, meterPosition.X, 0.01);
            Assert.AreEqual(expectedPositionM.Y, meterPosition.Y, 0.01);
        }

        private static object[] millimetersToPixelsCases = new object[]
        {
            new[] { new Vector2 (100f, 200f), new Vector2 (480, 960) },
            new[] { new Vector2(0f, 112.5f), new Vector2 (0, 540) }
        };

        [TestCaseSource(nameof(millimetersToPixelsCases))]
        public void MillimetersToPixels_ConvertsMillimeterPositionToPixels(Vector2 positionMm, Vector2 expectedPositionPx)
        {
            //Given
            VirtualScreen virtualScreen = CreateVirtualScreen();

            //When
            Vector2 pixelPosition = virtualScreen.MillimetersToPixels(positionMm);

            //Then
            Assert.AreEqual(expectedPositionPx.X, pixelPosition.X, 0.01);
            Assert.AreEqual(expectedPositionPx.Y, pixelPosition.Y, 0.01);
        }

        [TestCase(100f, 480)]
        [TestCase(0, 0)]
        public void MillimetersToPixels_ConvertsMillimeterPositionToPixels(float positionMm, float expectedPositionPx)
        {
            //Given
            VirtualScreen virtualScreen = CreateVirtualScreen();

            //When
            float pixelPosition = virtualScreen.MillimetersToPixels(positionMm);

            //Then
            Assert.AreEqual(expectedPositionPx, pixelPosition, 0.0001);
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
            new object[] { new Vector3(0.1f, 0, 1f), new Vector3(1020, 0, 1f) },
            new object[] { new Vector3(-0.1f, 0.1f, 1f), new Vector3(60, 480, 1f) },
            new object[] { new Vector3(0, 0.15f, 2f), new Vector3(540, 720, 2f) },
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

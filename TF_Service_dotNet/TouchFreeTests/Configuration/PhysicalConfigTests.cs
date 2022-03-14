using NUnit.Framework;
using Ultraleap.TouchFree.Library.Configuration;

namespace TouchFreeTests.Configuration
{
    public class PhysicalConfigTests
    {
        [Test]
        public void NewPhysicalConfig_CreatedWithDefaults_DefaultsAreSet()
        {
            //Given
            PhysicalConfigInternal config = null;

            //When
            config = new PhysicalConfigInternal();

            //Then
            Assert.AreEqual(330f, config.ScreenHeightMm);
            Assert.AreEqual(0f, config.LeapPositionRelativeToScreenBottomMm.X);
            Assert.AreEqual(-120f, config.LeapPositionRelativeToScreenBottomMm.Y);
            Assert.AreEqual(-250f, config.LeapPositionRelativeToScreenBottomMm.Z);
            Assert.AreEqual(0f, config.LeapRotationD.X);
            Assert.AreEqual(0f, config.LeapRotationD.Y);
            Assert.AreEqual(0f, config.LeapRotationD.Z);
            Assert.AreEqual(0f, config.ScreenRotationD);
            Assert.AreEqual(0, config.ScreenWidthPX);
            Assert.AreEqual(0, config.ScreenHeightPX);
        }
    }
}

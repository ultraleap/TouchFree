using NUnit.Framework;
using Ultraleap.TouchFree.Library.Configuration;

namespace TouchFreeTests
{
    public class PhysicalConfigTests
    {
        [Test]
        public void NewPhysicalConfig_CreatedWithDefaults_DefaultsAreSet()
        {
            //Given
            PhysicalConfig config = null;

            //When
            config = new PhysicalConfig();

            //Then
            Assert.AreEqual(0.33f, config.ScreenHeightM);
            Assert.AreEqual(0f, config.LeapPositionRelativeToScreenBottomM.X);
            Assert.AreEqual(-0.12f, config.LeapPositionRelativeToScreenBottomM.Y);
            Assert.AreEqual(-0.25f, config.LeapPositionRelativeToScreenBottomM.Z);
            Assert.AreEqual(0f, config.LeapRotationD.X);
            Assert.AreEqual(0f, config.LeapRotationD.Y);
            Assert.AreEqual(0f, config.LeapRotationD.Z);
            Assert.AreEqual(0f, config.ScreenRotationD);
            Assert.AreEqual(0, config.ScreenWidthPX);
            Assert.AreEqual(0, config.ScreenHeightPX);
        }
    }
}

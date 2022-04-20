using NUnit.Framework;
using Ultraleap.TouchFree.Library.Configuration;

namespace TouchFreeTests.Configuration
{
    public class ConfigManagerTests
    {
        [Test]
        public void AreConfigsInGoodState_DefaultConfigs_ReturnsFalse()
        {
            //Given
            var sut = new ConfigManager();
            sut.PhysicalConfig = new PhysicalConfigInternal();
            sut.InteractionConfig = new InteractionConfigInternal();

            //When
            bool result = sut.AreConfigsInGoodState();

            //Then
            Assert.AreEqual(false, result);
        }

        [Test]
        public void AreConfigsInGoodState_DefaultConfigsWithScreenHeightAndWidthSet_ReturnsTrue()
        {
            //Given
            var sut = new ConfigManager();
            sut.PhysicalConfig = new PhysicalConfigInternal()
            {
                ScreenHeightPX = 1,
                ScreenWidthPX = 1
            };
            sut.InteractionConfig = new InteractionConfigInternal();

            //When
            bool result = sut.AreConfigsInGoodState();

            //Then
            Assert.AreEqual(true, result);
        }
    }
}

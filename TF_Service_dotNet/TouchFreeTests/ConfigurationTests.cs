using NUnit.Framework;
using Ultraleap.TouchFree.Library.Configuration;

namespace TouchFreeTests
{
    public class Tests
    {
        PhysicalConfig physicalConfig;
        InteractionConfig interactionConfig;

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void PhysicalScreenHeightIsPositive()
        {
            Assert.IsTrue(ConfigManager.PhysicalConfig.ScreenHeightM > 0);
        }

        [Test]
        public void UpdateEventPassesPhysicalConfig()
        {
            // Given
            PhysicalConfig testConfig = new PhysicalConfig { ScreenHeightM = 0.5f };
            PhysicalConfig.OnConfigUpdated += OnPhysicalConfig;
            
            // When
            testConfig.ConfigWasUpdated();

            // Then
            Assert.IsTrue(physicalConfig != null);
            Assert.IsTrue(physicalConfig.ScreenHeightM == 0.5f);
        }

        [Test]
        public void UpdateEventPassesInteractionConfig()
        {
            // Given
            InteractionConfig testConfig = new InteractionConfig { UseScrollingOrDragging = true };
            InteractionConfig.OnConfigUpdated += OnInteractionConfig;

            // When
            testConfig.ConfigWasUpdated();

            // Then
            Assert.IsTrue(interactionConfig != null);
            Assert.IsTrue(interactionConfig.UseScrollingOrDragging == true);
        }


        public void OnPhysicalConfig(BaseConfig config)
        {
            physicalConfig = config as PhysicalConfig;
        }

        public void OnInteractionConfig(BaseConfig config)
        {
            interactionConfig = config as InteractionConfig;
        }
    }
}
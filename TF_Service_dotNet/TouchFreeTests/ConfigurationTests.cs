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
            ConfigManager.OnPhysicalConfigUpdated += OnPhysicalConfig;

            // When
            ConfigManager.PhysicalConfig = testConfig;
            ConfigManager.PhysicalConfigWasUpdated();

            // Then
            Assert.IsTrue(physicalConfig != null);
            Assert.IsTrue(physicalConfig.ScreenHeightM == 0.5f);
        }

        [Test]
        public void UpdateEventPassesInteractionConfig()
        {
            // Given
            InteractionConfig testConfig = new InteractionConfig { UseScrollingOrDragging = true };
            ConfigManager.OnInteractionConfigUpdated += OnInteractionConfig;

            // When
            ConfigManager.InteractionConfig = testConfig;
            ConfigManager.InteractionConfigWasUpdated();

            // Then
            Assert.IsTrue(interactionConfig != null);
            Assert.IsTrue(interactionConfig.UseScrollingOrDragging == true);
        }


        public void OnPhysicalConfig(PhysicalConfig config)
        {
            physicalConfig = config;
        }

        public void OnInteractionConfig(InteractionConfig config)
        {
            interactionConfig = config;
        }
    }
}
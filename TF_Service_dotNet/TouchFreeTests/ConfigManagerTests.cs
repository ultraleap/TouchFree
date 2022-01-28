using NUnit.Framework;
using Ultraleap.TouchFree.Library.Configuration;

namespace TouchFreeTests
{
    public class ConfigManagerTests
    {
        PhysicalConfig physicalConfig;
        InteractionConfig interactionConfig;
        ConfigManager configManager;

        [SetUp]
        public void Setup()
        {
            configManager = new ConfigManager();
        }

        [Test]
        public void UpdateEventPassesPhysicalConfig()
        {
            // Given
            PhysicalConfig testConfig = new PhysicalConfig { ScreenHeightM = 0.5f };
            configManager.OnPhysicalConfigUpdated += OnPhysicalConfig;

            // When
            configManager.PhysicalConfig = testConfig;
            configManager.PhysicalConfigWasUpdated();

            // Then
            Assert.IsNotNull(physicalConfig);
            Assert.AreEqual(0.5f, physicalConfig.ScreenHeightM);
        }

        [Test]
        public void UpdateEventPassesInteractionConfig()
        {
            // Given
            InteractionConfig testConfig = new InteractionConfig { UseScrollingOrDragging = true };
            configManager.OnInteractionConfigUpdated += OnInteractionConfig;

            // When
            configManager.InteractionConfig = testConfig;
            configManager.InteractionConfigWasUpdated();

            // Then
            Assert.IsNotNull(interactionConfig);
            Assert.IsTrue(interactionConfig.UseScrollingOrDragging);
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
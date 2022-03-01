using NUnit.Framework;
using Ultraleap.TouchFree.Library.Configuration;

namespace TouchFreeTests
{
    public class ConfigManagerTests
    {
        PhysicalConfigInternal physicalConfig;
        InteractionConfigInternal interactionConfig;
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
            PhysicalConfigInternal testConfig = new PhysicalConfigInternal { ScreenHeightMm = 50f };
            configManager.OnPhysicalConfigUpdated += OnPhysicalConfig;

            // When
            configManager.PhysicalConfig = testConfig;
            configManager.PhysicalConfigWasUpdated();

            // Then
            Assert.IsNotNull(physicalConfig);
            Assert.AreEqual(50f, physicalConfig.ScreenHeightMm);
        }

        [Test]
        public void UpdateEventPassesInteractionConfig()
        {
            // Given
            InteractionConfigInternal testConfig = new InteractionConfigInternal { UseScrollingOrDragging = true };
            configManager.OnInteractionConfigUpdated += OnInteractionConfig;

            // When
            configManager.InteractionConfig = testConfig;
            configManager.InteractionConfigWasUpdated();

            // Then
            Assert.IsNotNull(interactionConfig);
            Assert.IsTrue(interactionConfig.UseScrollingOrDragging);
        }


        public void OnPhysicalConfig(PhysicalConfigInternal config)
        {
            physicalConfig = config;
        }

        public void OnInteractionConfig(InteractionConfigInternal config)
        {
            interactionConfig = config;
        }
    }
}
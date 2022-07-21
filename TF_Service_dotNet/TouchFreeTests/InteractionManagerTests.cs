﻿using System.Linq;
using Moq;
using NUnit.Framework;
using Ultraleap.TouchFree.Library;
using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Library.Interactions;

namespace TouchFreeTests
{
    public class InteractionManagerTests
    {
        [Test]
        public void Constructor_DefaultInteractionUsed_PushInteractionInActiveInteractions()
        {
            // Arrange
            // Act
            var sut = CreateSut();

            // Assert
            Assert.AreEqual(InteractionType.PUSH, sut.activeInteractions.Single().Key.InteractionType);
        }

        [Test]
        public void OnInteractionSettingsUpdated_HoverInteractionInUpdatedSettings_HoverInteractionInActiveInteractions()
        {
            // Arrange
            var sut = CreateSut();
            var updatedConfig = new InteractionConfigInternal();
            updatedConfig.InteractionType = InteractionType.HOVER;

            // Act
            sut.OnInteractionSettingsUpdated(updatedConfig);

            // Assert
            Assert.AreEqual(InteractionType.HOVER, sut.activeInteractions.Single().Key.InteractionType);
        }

        public InteractionManager CreateSut()
        {
            var configManager = new Mock<IConfigManager>();
            configManager.SetupGet(x => x.InteractionConfig).Returns(new InteractionConfigInternal());
            var interactions = new[]
            {
                CreateInteractionWithType(InteractionType.AIRCLICK),
                CreateInteractionWithType(InteractionType.GRAB),
                CreateInteractionWithType(InteractionType.HOVER),
                CreateInteractionWithType(InteractionType.PUSH),
                CreateInteractionWithType(InteractionType.TOUCHPLANE),
                CreateInteractionWithType(InteractionType.VELOCITYSWIPE)
            };
            var updateBehaviour = new UpdateBehaviour();
            var sut = new InteractionManager(updateBehaviour, null, interactions, null, configManager.Object, null);
            return sut;
        }

        private IInteraction CreateInteractionWithType(InteractionType interactionType)
        {
            var interaction = new Mock<IInteraction>();
            interaction.SetupGet(x => x.InteractionType).Returns(interactionType);

            return interaction.Object;
        }
    }
}

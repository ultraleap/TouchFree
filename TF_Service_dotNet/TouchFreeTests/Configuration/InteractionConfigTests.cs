﻿using NUnit.Framework;
using Ultraleap.TouchFree.Library;
using Ultraleap.TouchFree.Library.Configuration;

namespace TouchFreeTests.Configuration
{
    public class InteractionConfigTests
    {
        [Test]
        public void NewInteractionConfig_CreatedWithDefaults_DefaultsAreSet()
        {
            //Given
            InteractionConfigInternal config = null;

            //When
            config = new InteractionConfigInternal();

            //Then
            Assert.AreEqual(true, config.UseScrollingOrDragging);
            Assert.AreEqual(3f, config.DeadzoneRadiusMm);
            Assert.AreEqual(false, config.InteractionZoneEnabled);
            Assert.AreEqual(0.0f, config.InteractionMinDistanceMm);
            Assert.AreEqual(250.0f, config.InteractionMaxDistanceMm);
            Assert.AreEqual(InteractionType.PUSH, config.InteractionType);
            Assert.AreEqual(0.5f, config.HoverAndHold.HoverStartTimeS);
            Assert.AreEqual(0.6f, config.HoverAndHold.HoverCompleteTimeS);
            Assert.AreEqual(50f, config.TouchPlane.TouchPlaneActivationDistanceMm);
            Assert.AreEqual(TrackedPosition.NEAREST, config.TouchPlane.TouchPlaneTrackedPosition);
        }
    }
}

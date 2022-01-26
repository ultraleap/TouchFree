using NUnit.Framework;
using Ultraleap.TouchFree.Library;
using Ultraleap.TouchFree.Library.Configuration;

namespace TouchFreeTests
{
    public class InteractionConfigTests
    {
        [Test]
        public void NewInteractionConfig_CreatedWithDefaults_DefaultsAreSet()
        {
            //Given
            InteractionConfig config = null;

            //When
            config = new InteractionConfig();

            //Then
            Assert.AreEqual(false, config.UseScrollingOrDragging);
            Assert.AreEqual(0.003f, config.DeadzoneRadius);
            Assert.AreEqual(false, config.InteractionZoneEnabled);
            Assert.AreEqual(0.0f, config.InteractionMinDistanceCm);
            Assert.AreEqual(25.0f, config.InteractionMaxDistanceCm);
            Assert.AreEqual(InteractionType.PUSH, config.InteractionType);
            Assert.AreEqual(0.5f, config.HoverAndHold.HoverStartTimeS);
            Assert.AreEqual(0.6f, config.HoverAndHold.HoverCompleteTimeS);
            Assert.AreEqual(5f, config.TouchPlane.TouchPlaneActivationDistanceCM);
            Assert.AreEqual(TrackedPosition.NEAREST, config.TouchPlane.TouchPlaneTrackedPosition);
        }
    }
}

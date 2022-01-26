using System.Numerics;
using NUnit.Framework;
using TouchFreeTests.TestImplementations;
using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Library.Interactions;

namespace TouchFreeTests
{
    public class PositionStabiliserTests
    {
        [Test]
        public void Constructor_ValidConfigManagerPassedIn_ReturnsInstance()
        {
            //Given
            IConfigManager configManager = new TestConfigManager();

            //When
            var instance = new PositionStabiliser(configManager);

            //Then
            Assert.IsNotNull(instance);
        }

        private PositionStabiliser CreatePositionStabiliser()
        {
            IConfigManager configManager = new TestConfigManager();
            return new PositionStabiliser(configManager);
        }

        [Test]
        public void ApplyDeadzoneSized_NewPositionInsideDeadzone_ReturnsPreviousPosition()
        {
            //Given
            Vector2 currentPosition = new Vector2(1.001f, 1.001f);
            Vector2 previousPosition = new Vector2(1, 1);
            float deadzoneRadius = 0.1f;
            PositionStabiliser positionStabiliser = CreatePositionStabiliser();

            //When
            var result = positionStabiliser.ApplyDeadzoneSized(previousPosition, currentPosition, deadzoneRadius);

            //Then
            Assert.AreEqual(previousPosition, result);
        }

        [TestCase(1.5f, 1f, 1.4f, 1f)]
        [TestCase(1f, 1.5f, 1f, 1.4f)]
        public void ApplyDeadzoneSized_NewPositionOutsideDeadzone_ReturnsCurrentPositionReducedByTheDeadzone(float currentX, float currentY, float expectedX, float expectedY)
        {
            //Given
            Vector2 currentPosition = new Vector2(currentX, currentY);
            Vector2 previousPosition = new Vector2(1, 1);
            float deadzoneRadius = 0.1f;
            PositionStabiliser positionStabiliser = CreatePositionStabiliser();

            //When
            var result = positionStabiliser.ApplyDeadzoneSized(previousPosition, currentPosition, deadzoneRadius);

            //Then
            Assert.AreEqual(expectedX, result.X);
            Assert.AreEqual(expectedY, result.Y);
        }

        [Test]
        public void ApplyDeadzone_DefaultDeadzoneRadiusIsZero_ReturnsPosition()
        {
            //Given
            Vector2 position = new Vector2(1, 2);
            PositionStabiliser positionStabiliser = CreatePositionStabiliser();
            positionStabiliser.defaultDeadzoneRadius = 0;

            //When
            var result = positionStabiliser.ApplyDeadzone(position);

            //Then
            Assert.AreEqual(position.X, result.X);
            Assert.AreEqual(position.Y, result.Y);
        }

        [Test]
        public void ApplyDeadzone_NoPreviousPosition_ReturnsPosition()
        {
            //Given
            Vector2 position = new Vector2(1, 2);
            PositionStabiliser positionStabiliser = CreatePositionStabiliser();
            positionStabiliser.defaultDeadzoneRadius = 0.2f;

            //When
            var result = positionStabiliser.ApplyDeadzone(position);

            //Then
            Assert.AreEqual(position.X, result.X);
            Assert.AreEqual(position.Y, result.Y);
        }

        [Test]
        public void ApplyDeadzone_CurrentPositionWithinDeadzone_ReturnsPreviousPosition()
        {
            //Given
            Vector2 previousPosition = new Vector2(1, 2);
            Vector2 currentPosition = new Vector2(1.1f, 2.1f);
            PositionStabiliser positionStabiliser = CreatePositionStabiliser();
            positionStabiliser.defaultDeadzoneRadius = 0.2f;
            positionStabiliser.currentDeadzoneRadius = 0.2f;
            positionStabiliser.ApplyDeadzone(previousPosition);

            //When
            var result = positionStabiliser.ApplyDeadzone(currentPosition);

            //Then
            Assert.AreEqual(previousPosition.X, result.X);
            Assert.AreEqual(previousPosition.Y, result.Y);
        }
    }
}

using System.Numerics;
using Moq;
using NUnit.Framework;
using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Library.Interactions;

namespace TouchFreeTests
{
    public class PositionStabiliserTests
    {
        private IConfigManager CreateMockedConfigManager()
        {
            Mock<IConfigManager> mockConfigManager = new Mock<IConfigManager>();
            mockConfigManager.SetupGet(x => x.InteractionConfig).Returns(new InteractionConfig());
            return mockConfigManager.Object;
        }

        [Test]
        public void Constructor_ValidConfigManagerPassedIn_ReturnsInstance()
        {
            //Given
            IConfigManager configManager = CreateMockedConfigManager();

            //When
            var instance = new PositionStabiliser(configManager);

            //Then
            Assert.IsNotNull(instance);
        }

        private PositionStabiliser CreatePositionStabiliser()
        {
            IConfigManager configManager = CreateMockedConfigManager();
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

        private static object[] applyDeadzoneSizedCases = new[]
        {
            new object[] { new Vector2(1.5f, 1f), new Vector2(1.4f, 1f)},
            new object[] { new Vector2(1f, 1.5f), new Vector2(1f, 1.4f)}
        };

        [TestCaseSource(nameof(applyDeadzoneSizedCases))]
        public void ApplyDeadzoneSized_NewPositionOutsideDeadzone_ReturnsCurrentPositionReducedByTheDeadzone(Vector2 currentPosition, Vector2 expectedPosition)
        {
            //Given
            Vector2 previousPosition = new Vector2(1, 1);
            float deadzoneRadius = 0.1f;
            PositionStabiliser positionStabiliser = CreatePositionStabiliser();

            //When
            var result = positionStabiliser.ApplyDeadzoneSized(previousPosition, currentPosition, deadzoneRadius);

            //Then
            Assert.AreEqual(expectedPosition.X, result.X);
            Assert.AreEqual(expectedPosition.Y, result.Y);
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

        [TestCase(1, 1.60000f, 2.400f)]
        [TestCase(2, 1.4000f, 4.600f)]
        [TestCase(3, 1.2000f, 6.800f)]
        [TestCase(4, 1.0000f, 9.000f)]
        [TestCase(5, 0.8000f, 11.200f)]
        public void ApplyDeadzone_StartShrinkingDeadzone_CurrentDeadzoneRadiusDecreases(int cursorMovementCount, float expectedDeadzoneSize, float expectedCursorY)
        {
            //Given
            Vector2 previousPosition = new Vector2(1, 2);
            PositionStabiliser positionStabiliser = CreatePositionStabiliser();
            positionStabiliser.defaultDeadzoneRadius = 0.2f;
            positionStabiliser.currentDeadzoneRadius = 1.6f;
            positionStabiliser.ApplyDeadzone(previousPosition);
            positionStabiliser.StartShrinkingDeadzone(0.1f);

            //When
            Vector2 result = previousPosition;
            for (var i = 1; i <= cursorMovementCount; i++)
            {
                Vector2 newPosition = Vector2.Add(previousPosition, new Vector2(0, 2 * i));
                result = positionStabiliser.ApplyDeadzone(newPosition);
            }

            //Then
            Assert.AreEqual(1f, result.X, 0.001);
            Assert.AreEqual(expectedCursorY, result.Y, 0.001);
            Assert.AreEqual(expectedDeadzoneSize, positionStabiliser.currentDeadzoneRadius, 0.0001);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public void ApplyDeadzone_StartShrinkingDeadzoneNoMovement_DeadzoneDoesNotShrink(int cursorMovementCount)
        {
            //Given
            Vector2 previousPosition = new Vector2(1, 2);
            PositionStabiliser positionStabiliser = CreatePositionStabiliser();
            positionStabiliser.defaultDeadzoneRadius = 0.2f;
            positionStabiliser.currentDeadzoneRadius = 1.6f;
            positionStabiliser.ApplyDeadzone(previousPosition);
            positionStabiliser.StartShrinkingDeadzone(0.1f);

            //When
            Vector2 result = previousPosition;
            for (var i = 1; i <= cursorMovementCount; i++)
            {
                result = positionStabiliser.ApplyDeadzone(previousPosition);
            }

            //Then
            Assert.AreEqual(1f, result.X, 0.001);
            Assert.AreEqual(2f, result.Y, 0.001);
            Assert.AreEqual(1.6f, positionStabiliser.currentDeadzoneRadius, 0.0001);
        }

        [Test]
        public void ApplyDeadzone_DeadzoneOffsetApplied_OffsetAppliedOnSubsequentMovement()
        {
            //Given
            Vector2 previousPosition = new Vector2(1, 2);
            PositionStabiliser positionStabiliser = CreatePositionStabiliser();
            positionStabiliser.defaultDeadzoneRadius = 0.2f;
            positionStabiliser.currentDeadzoneRadius = 1f;
            positionStabiliser.ApplyDeadzone(previousPosition);
            positionStabiliser.ApplyDeadzone(new Vector2(1, 2.5f));
            positionStabiliser.SetDeadzoneOffset();

            //When
            var result = positionStabiliser.ApplyDeadzone(new Vector2(1, 5));

            //Then
            Assert.AreEqual(1f, result.X, 0.001);
            Assert.AreEqual(3.5f, result.Y, 0.001);
            Assert.AreEqual(1f, positionStabiliser.currentDeadzoneRadius, 0.0001);
        }

        [Test]
        public void ApplyDeadzone_DeadzoneOffsetReduced_OffsetIsReduced()
        {
            //Given
            Vector2 previousPosition = new Vector2(1, 2);
            PositionStabiliser positionStabiliser = CreatePositionStabiliser();
            positionStabiliser.defaultDeadzoneRadius = 0.2f;
            positionStabiliser.currentDeadzoneRadius = 1f;
            positionStabiliser.ApplyDeadzone(previousPosition);
            positionStabiliser.ApplyDeadzone(new Vector2(1, 2.5f));
            positionStabiliser.SetDeadzoneOffset();
            positionStabiliser.ReduceDeadzoneOffset();

            //When
            var result = positionStabiliser.ApplyDeadzone(new Vector2(1, 5));

            //Then
            Assert.AreEqual(1f, result.X, 0.001);
            Assert.AreEqual(3.55f, result.Y, 0.001);
            Assert.AreEqual(1f, positionStabiliser.currentDeadzoneRadius, 0.0001);
        }
    }
}

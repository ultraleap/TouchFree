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
            new object[] { new Vector2(1500f, 1000f), new Vector2(1400f, 1000f)},
            new object[] { new Vector2(1000f, 1500f), new Vector2(1000f, 1400f)}
        };

        [TestCaseSource(nameof(applyDeadzoneSizedCases))]
        public void ApplyDeadzoneSized_NewPositionOutsideDeadzone_ReturnsCurrentPositionReducedByTheDeadzone(Vector2 currentPosition, Vector2 expectedPosition)
        {
            //Given
            Vector2 previousPosition = new Vector2(1000, 1000);
            float deadzoneRadius = 100f;
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

        [TestCase(1, 1600f,  2400f)]
        [TestCase(2, 1400f,  4600f)]
        [TestCase(3, 1200f,  6800f)]
        [TestCase(4, 1000f,  9000f)]
        [TestCase(5, 800f, 11200f)]
        public void ApplyDeadzone_StartShrinkingDeadzone_CurrentDeadzoneRadiusDecreases(int cursorMovementCount, float expectedDeadzoneSize, float expectedCursorY)
        {
            //Given
            Vector2 previousPosition = new Vector2(1000f, 2000f);
            PositionStabiliser positionStabiliser = CreatePositionStabiliser();
            positionStabiliser.defaultDeadzoneRadius = 200f;
            positionStabiliser.currentDeadzoneRadius = 1600f;
            positionStabiliser.ApplyDeadzone(previousPosition);
            positionStabiliser.StartShrinkingDeadzone(0.1f);

            //When
            Vector2 result = previousPosition;
            for (var i = 1; i <= cursorMovementCount; i++)
            {
                Vector2 newPosition = Vector2.Add(previousPosition, new Vector2(0, 2000f * i));
                result = positionStabiliser.ApplyDeadzone(newPosition);
            }

            //Then
            Assert.AreEqual(expectedDeadzoneSize, positionStabiliser.currentDeadzoneRadius, 0.0001);
            Assert.AreEqual(1000f, result.X, 0.001);
            Assert.AreEqual(expectedCursorY, result.Y, 0.01);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public void ApplyDeadzone_StartShrinkingDeadzoneNoMovement_DeadzoneDoesNotShrink(int cursorMovementCount)
        {
            //Given
            Vector2 previousPosition = new Vector2(1000, 2000);
            PositionStabiliser positionStabiliser = CreatePositionStabiliser();
            positionStabiliser.defaultDeadzoneRadius = 200f;
            positionStabiliser.currentDeadzoneRadius = 1600f;
            positionStabiliser.ApplyDeadzone(previousPosition);
            positionStabiliser.StartShrinkingDeadzone(0.1f);

            //When
            Vector2 result = previousPosition;
            for (var i = 1; i <= cursorMovementCount; i++)
            {
                result = positionStabiliser.ApplyDeadzone(previousPosition);
            }

            //Then
            Assert.AreEqual(1000f, result.X, 0.001);
            Assert.AreEqual(2000f, result.Y, 0.001);
            Assert.AreEqual(1600f, positionStabiliser.currentDeadzoneRadius, 0.0001);
        }

        [Test]
        public void ApplyDeadzone_DeadzoneOffsetApplied_OffsetAppliedOnSubsequentMovement()
        {
            //Given
            Vector2 previousPosition = new Vector2(1000f, 2000f);
            PositionStabiliser positionStabiliser = CreatePositionStabiliser();
            positionStabiliser.defaultDeadzoneRadius = 200f;
            positionStabiliser.currentDeadzoneRadius = 1000f;
            positionStabiliser.ApplyDeadzone(previousPosition);
            positionStabiliser.ApplyDeadzone(new Vector2(1000, 2500f));
            positionStabiliser.SetDeadzoneOffset();

            //When
            var result = positionStabiliser.ApplyDeadzone(new Vector2(1000, 5000));

            //Then
            Assert.AreEqual(1000f, result.X, 0.001);
            Assert.AreEqual(3500f, result.Y, 0.001);
            Assert.AreEqual(1000f, positionStabiliser.currentDeadzoneRadius, 0.0001);
        }

        [Test]
        public void ApplyDeadzone_DeadzoneOffsetReduced_OffsetIsReduced()
        {
            //Given
            Vector2 previousPosition = new Vector2(1000, 2000);
            PositionStabiliser positionStabiliser = CreatePositionStabiliser();
            positionStabiliser.defaultDeadzoneRadius = 200f;
            positionStabiliser.currentDeadzoneRadius = 1000f;
            positionStabiliser.ApplyDeadzone(previousPosition);
            positionStabiliser.ApplyDeadzone(new Vector2(1000, 2500f));
            positionStabiliser.SetDeadzoneOffset();
            positionStabiliser.ReduceDeadzoneOffset();

            //When
            var result = positionStabiliser.ApplyDeadzone(new Vector2(1000, 5000));

            //Then
            Assert.AreEqual(1000f, result.X, 0.001);
            Assert.AreEqual(3550f, result.Y, 0.001);
            Assert.AreEqual(1000f, positionStabiliser.currentDeadzoneRadius, 0.0001);
        }
    }
}

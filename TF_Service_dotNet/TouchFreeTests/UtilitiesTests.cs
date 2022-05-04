using NUnit.Framework;
using Ultraleap.TouchFree.Library;

namespace TouchFreeTests
{
    public class UtilitiesTests
    {
        #region "LeapVectorToNumerics"
        [TestCase(1000, 2000, 3000, 1, 2, 3)]
        public void LeapVectorToNumerics(float x, float y, float z, float expectedX, float expectedY, float expectedZ)
        {
            // Arrange
            Leap.Vector vector = new Leap.Vector(x, y, z);

            // Act
            var result = Utilities.LeapVectorToNumerics(vector);

            // Assert
            Assert.AreEqual(expectedX, result.X);
            Assert.AreEqual(expectedY, result.Y);
            Assert.AreEqual(expectedZ, result.Z);
        }
        #endregion

        #region "Lerp & InverseLerp"
        [TestCase(1, 2, 0.5f, 1.5f)]
        [TestCase(1, 2, 0, 1)]
        [TestCase(1, 2, 1, 2)]
        public void Lerp(float first, float second, float amount, float expected)
        {
            // Arrange

            // Act
            var result = Utilities.Lerp(first, second, amount);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestCase(1, 2, 1.5f, 0.5f)]
        [TestCase(1, 2, 1, 0)]
        [TestCase(1, 2, 2, 1)]
        public void InverseLerp(float first, float second, float value, float expected)
        {
            // Arrange

            // Act
            var result = Utilities.InverseLerp(first, second, value);

            // Assert
            Assert.AreEqual(expected, result);
        }
        #endregion

        #region "MapRangeToRange"
        [TestCase(0, 0, 1)]
        [TestCase(2, 2, 0)]
        [TestCase(1, 1, -1)]
        public void MapRangeToRange_ValueIsMinOfOldRange_ReturnsMinOfNewRange(float value, float minOfOldRange, float minOfNewRange)
        {
            // Arrange

            // Act
            var result = Utilities.MapRangeToRange(value, minOfOldRange, 100, minOfNewRange, 100);

            // Assert
            Assert.AreEqual(minOfNewRange, result);
        }

        [TestCase(0, 0, 1)]
        [TestCase(2, 2, 0)]
        [TestCase(1, 1, -1)]
        public void MapRangeToRange_ValueIsMaxOfOldRange_ReturnsMaxOfNewRange(float value, float maxOfOldRange, float maxOfNewRange)
        {
            // Arrange

            // Act
            var result = Utilities.MapRangeToRange(value, -100, maxOfOldRange, -100, maxOfNewRange);

            // Assert
            Assert.AreEqual(maxOfNewRange, result);
        }

        [TestCase(1, 0, 2, 0, 4, 2)]
        [TestCase(10, 0, 20, 0, 10, 5)]
        public void MapRangeToRange_ValueIsMiddleOfOldRange_ReturnsMiddleOfNewRange(float value, float minOfOldRange, float maxOfOldRange, float minOfNewRange, float maxOfNewRange, float expected)
        {
            // Arrange

            // Act
            var result = Utilities.MapRangeToRange(value, minOfOldRange, maxOfOldRange, minOfNewRange, maxOfNewRange);

            // Assert
            Assert.AreEqual(expected, result);
        }
        #endregion
    }
}

using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using Ultraleap.ScreenControl.Core;

namespace Tests
{
    public class ScreenControlUtilityTest
    {
        // A Test behaves as an ordinary method
        [Test]
        public void MapToRangeAtLowestBound()
        {
            float initial = 1.0f;

            float initialMin = 1.0f;
            float initialMax = 5.0f;

            float newMin = 10.0f;
            float newMax = 50.0f;

            float expectedResult = newMin;

            float result = ScreenControlUtility.MapRangeToRange(
                initial,
                initialMin,
                initialMax,
                newMin,
                newMax);

            Assert.True(Mathf.Approximately(result, expectedResult));
        }

        [Test]
        public void MapToRangeAtHighestBound()
        {
            float initial = 5.0f;

            float initialMin = 1.0f;
            float initialMax = 5.0f;

            float newMin = 10.0f;
            float newMax = 50.0f;

            float expectedResult = newMax;

            float result = ScreenControlUtility.MapRangeToRange(
                initial,
                initialMin,
                initialMax,
                newMin,
                newMax);

            Assert.True(Mathf.Approximately(result, expectedResult));
        }

        [Test]
        public void MapToRangeAtCentre()
        {
            float initial = 7.5f;

            float initialMin = 5.0f;
            float initialMax = 10.0f;

            float newMin = 0.0f;
            float newMax = 15.0f;

            float expectedResult = initial;

            float result = ScreenControlUtility.MapRangeToRange(
                initial,
                initialMin,
                initialMax,
                newMin,
                newMax);

            Assert.True(Mathf.Approximately(result, expectedResult));
        }

        [Test]
        public void ParseColorFromHexNoAlpha()
        {
            string textColor = "#00ff00";

            Color result = ScreenControlUtility.ParseColor(textColor);

            Assert.AreEqual(Color.green, result);
        }

        [Test]
        public void ParseColorDefaultAlphaOverride()
        {
            string textColor = "#00ff0000";

            Color result = ScreenControlUtility.ParseColor(textColor);

            Assert.AreEqual(Color.green, result);
        }

        [Test]
        public void ParseColorCustomAlphaOverride()
        {
            string textColor = "#00ff007f";

            Color result = ScreenControlUtility.ParseColor(textColor, 0.25f);

            Assert.True(Mathf.Approximately(0.00f, result.r));
            Assert.True(Mathf.Approximately(1.00f, result.g));
            Assert.True(Mathf.Approximately(0.00f, result.b));
            Assert.True(Mathf.Approximately(0.25f, result.a));
        }
    }
}

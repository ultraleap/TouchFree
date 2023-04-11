using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Ultraleap.TouchFree.Library.Connections;

namespace TouchFreeTests.Connections
{
    public class MessageValidationTests
    {
        [Test]
        public void ValidateConfigJson_FloatingPointDataInIntegerField_FailsValidation()
        {
            var json = 
@"{
    ""requestID"": ""d1bc31f0-dfe1-4801-9b17-c30f80406f6c"",
    ""interaction"": null,
    ""physical"": {
        ""LeapPositionRelativeToScreenBottomM"": {
            ""X"": 0,
            ""Y"": -0.3,
            ""Z"": -0.05
        },
        ""LeapRotationD"": {
            ""X"": 0,
            ""Y"": 0,
            ""Z"": 0
        },
        ""ScreenHeightM"": 0.3,
        ""ScreenRotationD"": 0,
        ""ScreenHeightPX"": 1079.9999530642,
        ""ScreenWidthPX"": 1920.0001304921
    }
}";
            var jObj = JsonConvert.DeserializeObject<JObject>(json);
            var result = MessageValidation.ValidateConfigJson(jObj);
            Assert.That(result.IsError);
        }

        [TestCase(
@"{
    ""requestID"": ""d1bc31f0-dfe1-4801-9b17-c30f80406f6c"",
    ""physical"": {
        ""LeapPositionRelativeToScreenBottomM"": {
            ""X"": 0,
            ""Y"": -0.3,
            ""Z"": -0.05
        },
        ""LeapRotationD"": {
            ""X"": 0,
            ""Y"": 0,
            ""Z"": 0
        },
        ""ScreenHeightM"": 0.3,
        ""ScreenRotationD"": 0,
        ""ScreenHeightPX"": 1080,
        ""ScreenWidthPX"": 1920
    }
}")]
        [TestCase(
@"{
    ""requestID"": ""d1bc31f0-dfe1-4801-9b17-c30f80406f6c"",
    ""interaction"": null
}")]
        public void ValidateConfigJson_OptionalConfigMissing_SucceedsValidation(string json)
        {
            var jObj = JsonConvert.DeserializeObject<JObject>(json);
            var result = MessageValidation.ValidateConfigJson(jObj);
            Assert.That(result.IsSuccess);
        }

        [TestCase(
@"{
    ""requestID"": ""d1bc31f0-dfe1-4801-9b17-c30f80406f6c""
}")]
        [TestCase(
@"{
    ""requestID"": ""d1bc31f0-dfe1-4801-9b17-c30f80406f6c"",
    ""anUnlikelyConfigName"": {
        ""numberField"": 1000
    }
}")]
        public void ValidateConfigJson_NoValidConfigContent_FailsValidation(string json)
        {
            var jObj = JsonConvert.DeserializeObject<JObject>(json);
            var result = MessageValidation.ValidateConfigJson(jObj);
            Assert.That(result.IsError);
        }

        [TestCase(
@"{
    ""requestID"": ""d1bc31f0-dfe1-4801-9b17-c30f80406f6c"",
    ""physical"": {
        ""LeapPositionRelativeToScreenBottomM"": {
            ""X"": 0,
            ""Y"": -0.3,
            ""Z"": -0.05
        },
        ""LeapRotationD"": {
            ""X"": 0,
            ""Y"": 0,
            ""Z"": 0
        },
        ""ScreenHeightM"": 0.3,
        ""ScreenRotationD"": 0,
        ""ScreenHeightPX"": 1080,
        ""ScreenWidthPX"": 1920
    },
    ""extra"": ""2"",
    ""bar"": ""foo""
}")]
        [TestCase(
            @"{
    ""requestID"": ""d1bc31f0-dfe1-4801-9b17-c30f80406f6c"",
    ""physical"": {
        ""LeapPositionRelativeToScreenBottomM"": {
            ""X"": 0,
            ""Y"": -0.3,
            ""Z"": -0.05
        },
        ""LeapRotationD"": {
            ""X"": 0,
            ""Y"": 0,
            ""Z"": 0
        },
        ""ScreenHeightM"": 0.3,
        ""ScreenRotationD"": 0,
        ""ScreenHeightPX"": 1080,
        ""ScreenWidthPX"": 1920,
        ""ExtraFieldShouldFailValidation"": 2000,
        ""AnotherExtraField"": 1000
    }
}")]
        public void ValidateConfigJson_ExtraUnknownContent_FailsValidation(string json)
        {
            var jObj = JsonConvert.DeserializeObject<JObject>(json);
            var result = MessageValidation.ValidateConfigJson(jObj);
            Assert.That(result.IsError);
        }
        
        [Test]
        public void ValidateRequestId_RequestIDOnObject_ReturnsTrue()
        {
            // Arrange
            JObject testObject = new JObject();
            testObject.Add("requestID", "test");

            // Act
            var result = MessageValidation.ValidateRequestId(testObject);

            // Assert
            Assert.That(result.IsSuccess);
        }

        [Test]
        public void ValidateRequestId_NoRequestIDOnObject_ReturnsFalse()
        {
            // Arrange
            JObject testObject = new JObject();

            // Act
            var result = MessageValidation.ValidateRequestId(testObject);

            // Assert
            Assert.That(result.IsError);
        }

        [Test]
        public void ValidateRequestId_NoRequestIDIsEmptyOnObject_ReturnsFalse()
        {
            // Arrange
            JObject testObject = new JObject();
            testObject.Add("requestID", string.Empty);

            // Act
            var result = MessageValidation.ValidateRequestId(testObject);

            // Assert
            Assert.That(result.IsError);
        }

        [Test]
        public void ValidateRequestId_ReturnsCorrectRequestId()
        {
            var idString = "testrequestidstring";
            JObject testObject = new JObject();
            testObject.Add("requestID", idString);

            var result = MessageValidation.ValidateRequestId(testObject);
            
            Assert.That(result.TryGetValue(out var id) && id == idString);
        }
    }
}
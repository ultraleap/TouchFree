using System;
using Newtonsoft.Json;
using NUnit.Framework;
using Ultraleap.TouchFree.Library.Configuration;

namespace TouchFreeTests.Configuration
{
    public class ConfigFileTests
    {
        [Test]
        public void DeserialiseRawText_NoErrors_ReturnsConfigObject()
        {
            //Given
            TestConfig testConfig = new TestConfig();
            SetTestConfigValues(testConfig);
            string jsonConfig = JsonConvert.SerializeObject(testConfig);
            ImplementedConfigFile configFile = new ImplementedConfigFile();

            //When
            TestConfig result = configFile.DeserialiseText(jsonConfig);

            //Then
            Assert.IsNotNull(result);
            Assert.AreEqual("Data in the configuration", result.SubConfig.Data);
        }

        [Test]
        public void DeserialiseRawText_CloseBracesMissing_ThrowsSerialisationException()
        {
            //Given
            TestConfig testConfig = new TestConfig();
            SetTestConfigValues(testConfig);
            string jsonConfig = JsonConvert.SerializeObject(testConfig);
            ImplementedConfigFile configFile = new ImplementedConfigFile();
            jsonConfig = jsonConfig.TrimEnd(new char[] { '}', ' ' });

            //When & Then
            Assert.Throws<JsonSerializationException>(() => configFile.DeserialiseText(jsonConfig));
        }

        [Test]
        public void DeserialiseRawText_OpenBracesMissing_ErrorHandledAndNullReturnedSoDefaultConfigCreated()
        {
            //Given
            TestConfig testConfig = new TestConfig();
            SetTestConfigValues(testConfig);
            string jsonConfig = JsonConvert.SerializeObject(testConfig);
            ImplementedConfigFile configFile = new ImplementedConfigFile();
            jsonConfig = jsonConfig.TrimStart(new char[] { '{', ' ' });

            //When
            var result = configFile.DeserialiseText(jsonConfig);

            //Then
            Assert.IsNull(result);
        }

        [Test]
        public void DeserialiseRawText_ExtraEntry_DeserialisesAndIgnoresExtraEntry()
        {
            //Given
            TestConfig testConfig = new TestConfigWithExtraEntries();
            SetTestConfigValues(testConfig);
            string jsonConfig = JsonConvert.SerializeObject(testConfig);
            ImplementedConfigFile configFile = new ImplementedConfigFile();

            //When
            TestConfig result = configFile.DeserialiseText(jsonConfig);

            //Then
            Assert.IsNotNull(result);
            Assert.AreEqual("Data in the configuration", result.SubConfig.Data);
        }

        [Test]
        public void DeserialiseRawText_MissingEntry_DeserialisesAndIgnoresMissingEntry()
        {
            //Given
            TestConfig testConfig = new TestConfig();
            SetTestConfigValues(testConfig);
            testConfig.Name = null;
            string jsonConfig = JsonConvert.SerializeObject(testConfig, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            ImplementedConfigFile configFile = new ImplementedConfigFile();

            //When
            TestConfig result = configFile.DeserialiseText(jsonConfig);

            //Then
            Assert.IsNotNull(result);
            Assert.AreEqual("Data in the configuration", result.SubConfig.Data);
            Assert.IsNull(result.Name);
        }

        [Test]
        public void DeserialiseRawText_EmptyFile_ReturnsNullObject()
        {
            //Given
            ImplementedConfigFile configFile = new ImplementedConfigFile();

            //When
            TestConfig result = configFile.DeserialiseText("");

            //Then
            Assert.IsNull(result);
        }

        [Test]
        public void DeserialiseRawText_EmptyObject_ReturnsDefaultConfig()
        {
            //Given
            ImplementedConfigFile configFile = new ImplementedConfigFile();

            //When
            TestConfig result = configFile.DeserialiseText("{}");

            //Then
            Assert.AreEqual(JsonConvert.SerializeObject(result), JsonConvert.SerializeObject(new TestConfig()));
        }

        [Test]
        public void DeserialiseRawText_MissingCloseQuote_ThrowsReaderException()
        {
            //Given
            TestConfig testConfig = new TestConfig();
            SetTestConfigValues(testConfig);
            string jsonConfig = JsonConvert.SerializeObject(testConfig);
            ImplementedConfigFile configFile = new ImplementedConfigFile();
            int indexOfLastCloseQuote = jsonConfig.LastIndexOf('"');
            jsonConfig = jsonConfig.Remove(indexOfLastCloseQuote, 1);

            //When & Then
            Assert.Throws<JsonReaderException>(() => configFile.DeserialiseText(jsonConfig));
        }

        private class TestConfigWithExtraEntries : TestConfig
        {
            public TestConfigWithExtraEntries() : base()
            {
                ExtraEntry = "ExtraEntry";
            }

            public string ExtraEntry { get; set; }
        }

        private void SetTestConfigValues(TestConfig config)
        {
            config.SubConfig = new TestSubConfig()
            {
                Data = "Data in the configuration"
            };
            config.Name = "Name";
            config.Description = "Description";
        }

        private class TestConfig
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public TestSubConfig SubConfig { get; set; }
        }

        private class TestSubConfig
        {
            public string Data { get; set; }
        }

        private class ImplementedConfigFile : ConfigFile<TestConfig, ImplementedConfigFile>
        {
            protected override string _ConfigFileName => throw new NotImplementedException();

            public TestConfig DeserialiseText(string text)
            {
                return DeserialiseRawText(text);
            }
        }
    }
}

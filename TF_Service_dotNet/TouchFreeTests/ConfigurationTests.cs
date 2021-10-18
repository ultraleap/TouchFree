using NUnit.Framework;
using Ultraleap.TouchFree.Library.Configuration;

namespace TouchFreeTests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void PhysicalScreenHeightIsPositive()
        {
            Assert.IsTrue(ConfigManager.PhysicalConfig.ScreenHeightM > 0);
        }

        [Test]
        public void FailingTest()
        {
            Assert.Fail();
        }
    }
}
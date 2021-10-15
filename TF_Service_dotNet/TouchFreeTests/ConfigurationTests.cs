using NUnit.Framework;
using Ultraleap.TouchFree.Service.Configuration;

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
    }
}
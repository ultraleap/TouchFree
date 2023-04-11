using Moq;
using NUnit.Framework;
using System;
using System.Net.WebSockets;
using Ultraleap.TouchFree.Library.Connections;

namespace TouchFreeTests.Connections
{
    public class ClientConnectionTests
    {
        [Theory]
        [TestCase(Compatibility.COMPATIBLE, "1.2.3", "1.2.3")]
        [TestCase(Compatibility.SERVICE_OUTDATED_WARNING, "1.2.4", "1.2.3")]
        [TestCase(Compatibility.SERVICE_OUTDATED, "1.3.0", "1.2.3")]
        [TestCase(Compatibility.CLIENT_OUTDATED_WARNING, "1.2.3", "1.3.0")]
        [TestCase(Compatibility.SERVICE_OUTDATED, "2.0.0", "1.2.3")]
        [TestCase(Compatibility.CLIENT_OUTDATED, "1.2.3", "2.0.0")]
        public void GetVersionCompatibility_VersionsPassedIn_ExpectedResultsReturned(Compatibility expectedCompatibility, string clientVersion, string serviceVersion)
        {
            // Arrange
            var clientConnection = new ClientConnection(new Mock<WebSocket>().Object, null, null, null);
            var parsedServiceVersion = new Version(serviceVersion);

            // Act
            var result = ClientConnection.GetVersionCompability(clientVersion, parsedServiceVersion);

            // Assert
            Assert.AreEqual(expectedCompatibility, result.Compatibility);
        }
    }
}

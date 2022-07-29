using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Net.WebSockets;
using Ultraleap.TouchFree.Library;
using Ultraleap.TouchFree.Library.Connections;

namespace TouchFreeTests.Connections
{
    public class ClientConnectionManagerTests
    {
        public static string[] ClientConnectionMethods = typeof(IClientConnection).GetMethods().Select(x => x.Name).Where(x => x.StartsWith("Send") && x != "SendHandPresenceEvent").ToArray();

        [TestCaseSource(nameof(ClientConnectionMethods))]
        public void SendMessageMethods_Called_CallsSimilarlyNamedMethodOnClientConnection(string clientConnectionMethod)
        {
            // Arrange
            var mockClientConnection = CreateClientConnectionMockWithOpenSocket();
            var clientConnectionManager = new ClientConnectionManager(new Mock<IHandManager>().Object);
            clientConnectionManager.AddConnection(mockClientConnection.Object);
            var methodInfo = typeof(ClientConnectionManager).GetMethod(clientConnectionMethod);
            var parameterInfo = methodInfo.GetParameters();

            if (parameterInfo.Any(x => x.ParameterType.ContainsGenericParameters))
            {
                // This handles methods with generic types and sets them to use ResponseToClient
                methodInfo = methodInfo.MakeGenericMethod(typeof(ResponseToClient));
                parameterInfo = methodInfo.GetParameters();
            }

            var parameters = parameterInfo.Select(p => Activator.CreateInstance(p.ParameterType)).ToArray();

            // Act
            methodInfo.Invoke(clientConnectionManager, parameters);

            // Assert
            Assert.True(mockClientConnection.Invocations.Any(x => x.Method.Name == clientConnectionMethod));
        }

        private Mock<IClientConnection> CreateClientConnectionMockWithOpenSocket()
        {
            var mockClientConnection = new Mock<IClientConnection>();
            var mockWebSocket = new Mock<WebSocket>();
            mockWebSocket.Setup(x => x.State).Returns(WebSocketState.Open);
            mockClientConnection.SetupGet(x => x.Socket).Returns(mockWebSocket.Object);
            return mockClientConnection;
        }

        [Test]
        public void AddConnection_SingleConnection_AddsConnectionToConnections()
        {
            // Arrange
            var mockClientConnection = CreateClientConnectionMockWithOpenSocket();
            var clientConnectionManager = new ClientConnectionManager(new Mock<IHandManager>().Object);

            // Act
            clientConnectionManager.AddConnection(mockClientConnection.Object);

            // Assert
            Assert.AreSame(mockClientConnection.Object, clientConnectionManager.clientConnections.Single());
        }

        [Test]
        public void RemoveConnection_SingleConnection_ConnectionsAreEmptyAndDisconnectFromTrackingCalled()
        {
            // Arrange
            var mockHandManager = new Mock<IHandManager>();
            var mockClientConnection = CreateClientConnectionMockWithOpenSocket();
            var clientConnectionManager = new ClientConnectionManager(mockHandManager.Object);
            clientConnectionManager.AddConnection(mockClientConnection.Object);

            // Act
            clientConnectionManager.RemoveConnection(mockClientConnection.Object.Socket);

            // Assert
            Assert.AreEqual(0, clientConnectionManager.clientConnections.Count());
            mockHandManager.Verify(x => x.DisconnectFromTracking(), Times.Once);
        }

        [Test]
        public void RemoveConnection_TwoConnections_ConnectionsHaveNonRemovedConnectionAndDisconnectFromTrackingNotCalled()
        {
            // Arrange
            var mockHandManager = new Mock<IHandManager>();
            var mockClientConnection = CreateClientConnectionMockWithOpenSocket();
            var mockSecondClientConnection = CreateClientConnectionMockWithOpenSocket();
            var clientConnectionManager = new ClientConnectionManager(mockHandManager.Object);
            clientConnectionManager.AddConnection(mockClientConnection.Object);
            clientConnectionManager.AddConnection(mockSecondClientConnection.Object);

            // Act
            clientConnectionManager.RemoveConnection(mockClientConnection.Object.Socket);

            // Assert
            Assert.AreEqual(1, clientConnectionManager.clientConnections.Count());
            Assert.AreSame(mockSecondClientConnection.Object, clientConnectionManager.clientConnections.Single());
            mockHandManager.Verify(x => x.DisconnectFromTracking(), Times.Never);
        }
    }
}

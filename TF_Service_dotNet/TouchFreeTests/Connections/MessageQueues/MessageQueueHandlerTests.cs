using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ultraleap.TouchFree.Library;
using Ultraleap.TouchFree.Library.Connections;
using Ultraleap.TouchFree.Library.Connections.MessageQueues;

namespace TouchFreeTests.Connections.MessageQueues
{
    public class MessageQueueHandlerTests
    {
        [Test]
        public void RequestIdExists_RequestIDOnObject_ReturnsTrue()
        {
            // Arrange
            JObject testObject = new JObject();
            testObject.Add("requestID", "test");

            // Act
            var result = MessageQueueHandler.RequestIdExists(testObject);

            // Assert
            Assert.AreEqual(true, result);
        }

        [Test]
        public void RequestIdExists_NoRequestIDOnObject_ReturnsFalse()
        {
            // Arrange
            JObject testObject = new JObject();

            // Act
            var result = MessageQueueHandler.RequestIdExists(testObject);

            // Assert
            Assert.AreEqual(false, result);
        }

        [Test]
        public void RequestIdExists_NoRequestIDIsEmptyOnObject_ReturnsFalse()
        {
            // Arrange
            JObject testObject = new JObject();
            testObject.Add("requestID", string.Empty);

            // Act
            var result = MessageQueueHandler.RequestIdExists(testObject);

            // Assert
            Assert.AreEqual(false, result);
        }

        [Test]
        public void AddItemToQueue_ItemAddedToQueueAndUpdateBehaviourTriggered_HandlesItemOnQueue()
        {
            // Arrange
            var updateBehaviour = new TestUpdateBehaviour();
            var mockClientConnectionManager = new Mock<IClientConnectionManager>();
            var sut = new TestMessageQueueHandler(new[] { ActionCode.HAND_DATA }, updateBehaviour, mockClientConnectionManager.Object);
            var request = new IncomingRequest(ActionCode.HAND_DATA, "", "");

            // Act
            sut.AddItemToQueue(request);
            updateBehaviour.FireUpdate();

            // Assert
            Assert.AreNotSame(request, sut.LastRequestHandleWasCalledAgainst);
            Assert.IsNotNull(sut.LastRequestHandleWasCalledAgainst);
            Assert.AreEqual(request.action, sut.LastRequestHandleWasCalledAgainst.Value.action);
            Assert.AreEqual(request.requestId, sut.LastRequestHandleWasCalledAgainst.Value.requestId);
            Assert.AreEqual(request.content, sut.LastRequestHandleWasCalledAgainst.Value.content);
        }

        [Test]
        public void AddItemToQueue_ItemAddedToQueueWithDifferentActionCode_ThrowsArgumentException()
        {
            // Arrange
            var updateBehaviour = new TestUpdateBehaviour();
            var mockClientConnectionManager = new Mock<IClientConnectionManager>();
            var sut = new TestMessageQueueHandler(new[] { ActionCode.HAND_DATA }, updateBehaviour, mockClientConnectionManager.Object);
            var request = new IncomingRequest(ActionCode.SERVICE_STATUS, "", "");

            // Act & Assert
            Assert.Throws<ArgumentException>(() => sut.AddItemToQueue(request));
        }

        private class TestMessageQueueHandler : MessageQueueHandler
        {
            public TestMessageQueueHandler(ActionCode[] actionCodes, IUpdateBehaviour _updateBehaviour, IClientConnectionManager _clientMgr) : base(_updateBehaviour, _clientMgr)
            {
                ConfiguredActionCodes = actionCodes;
            }

            public override ActionCode[] ActionCodes => ConfiguredActionCodes;
            public ActionCode[] ConfiguredActionCodes { get; set; }

            protected override void Handle(IncomingRequest _content)
            {
                LastRequestHandleWasCalledAgainst = _content;
            }

            public IncomingRequest? LastRequestHandleWasCalledAgainst { get; set; }
        }

        private class TestUpdateBehaviour : IUpdateBehaviour
        {
            public event IUpdateBehaviour.UpdateEvent OnUpdate;
            public void FireUpdate()
            {
                OnUpdate?.Invoke();
            }
        }
    }
}

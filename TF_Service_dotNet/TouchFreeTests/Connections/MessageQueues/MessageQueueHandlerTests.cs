using Moq;
using NUnit.Framework;
using System;
using Ultraleap.TouchFree.Library;
using Ultraleap.TouchFree.Library.Connections;
using Ultraleap.TouchFree.Library.Connections.MessageQueues;

namespace TouchFreeTests.Connections.MessageQueues
{
    public class MessageQueueHandlerTests
    {
        [Test]
        public void AddItemToQueue_ItemAddedToQueueAndUpdateBehaviourTriggered_HandlesItemOnQueue()
        {
            // Arrange
            var updateBehaviour = new TestUpdateBehaviour();
            var mockClientConnectionManager = new Mock<IClientConnectionManager>();
            var sut = new TestMessageQueueHandler(new[] { ActionCode.HAND_DATA }, updateBehaviour, mockClientConnectionManager.Object);
            var request = new IncomingRequest(ActionCode.HAND_DATA, "{\"requestID\": \"1\"}");

            // Act
            sut.AddItemToQueue(request);
            updateBehaviour.FireUpdate();

            // Assert
            Assert.AreNotSame(request, sut.LastRequestHandleWasCalledAgainst);
            Assert.IsNotNull(sut.LastRequestHandleWasCalledAgainst);
            Assert.AreEqual(request.ActionCode, sut.LastRequestHandleWasCalledAgainst.Value.ActionCode);
            Assert.AreEqual(request.Content, sut.LastRequestHandleWasCalledAgainst.Value.OriginalContent);
        }

        [Test]
        public void AddItemToQueue_ItemAddedToQueueWithDifferentActionCode_ThrowsArgumentException()
        {
            // Arrange
            var updateBehaviour = new TestUpdateBehaviour();
            var mockClientConnectionManager = new Mock<IClientConnectionManager>();
            var sut = new TestMessageQueueHandler(new[] { ActionCode.HAND_DATA }, updateBehaviour, mockClientConnectionManager.Object);
            var request = new IncomingRequest(ActionCode.SERVICE_STATUS, "");

            // Act & Assert
            Assert.Throws<ArgumentException>(() => sut.AddItemToQueue(request));
        }

        private class TestMessageQueueHandler : MessageQueueHandler
        {
            public TestMessageQueueHandler(ActionCode[] actionCodes, IUpdateBehaviour _updateBehaviour, IClientConnectionManager _clientMgr) : base(_updateBehaviour, _clientMgr)
            {
                ConfiguredActionCodes = actionCodes;
            }

            public override ActionCode[] HandledActionCodes => ConfiguredActionCodes;
            public ActionCode[] ConfiguredActionCodes { get; set; }

            protected override void Handle(in IncomingRequestWithId request)
            {
                LastRequestHandleWasCalledAgainst = request;
            }

            public IncomingRequestWithId? LastRequestHandleWasCalledAgainst { get; set; }

            protected override string WhatThisHandlerDoes => string.Empty;

            protected override ActionCode FailureActionCode => ActionCode.INPUT_ACTION;
        }

        private class TestUpdateBehaviour : IUpdateBehaviour
        {
            public event IUpdateBehaviour.UpdateEvent OnUpdate;
            public event IUpdateBehaviour.UpdateEvent OnSlowUpdate;

            public void FireUpdate()
            {
                OnUpdate?.Invoke();
            }
        }
    }
}

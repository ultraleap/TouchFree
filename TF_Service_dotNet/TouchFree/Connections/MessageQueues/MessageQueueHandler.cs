using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Linq;

namespace Ultraleap.TouchFree.Library.Connections.MessageQueues
{
    public abstract class MessageQueueHandler : IMessageQueueHandler
    {
        public abstract ActionCode[] HandledActionCodes { get; }

        private readonly ConcurrentQueue<IncomingRequest> queue = new();
        private readonly IUpdateBehaviour updateBehaviour;
        protected readonly IClientConnectionManager clientMgr;

        protected abstract string whatThisHandlerDoes { get; }
        protected abstract ActionCode failureActionCode { get; }

        public MessageQueueHandler(IUpdateBehaviour _updateBehaviour, IClientConnectionManager _clientMgr)
        {
            updateBehaviour = _updateBehaviour;

            updateBehaviour.OnUpdate += OnUpdate;
            clientMgr = _clientMgr;
        }

        public void AddItemToQueue(IncomingRequest request)
        {
            if (!HandledActionCodes.Contains(request.ActionCode))
            {
                throw new System.ArgumentException("Unexpected action type", nameof(request));
            }
            
            queue.Enqueue(request);
        }

        protected virtual void OnUpdate()
        {
            CheckQueue();
        }

        private void CheckQueue()
        {
            if (queue.IsEmpty) return;
            
            // Parse newly received messages
            if (!queue.TryDequeue(out var content)) return;
            HandleRequest(content);
        }

        private void HandleRequest(IncomingRequest request) =>
            request.DeserializeAndValidateRequestId()
                .Match(requestWithId => ValidateContent(requestWithId)
                        .Match(_ => Handle(requestWithId),
                            error => HandleValidationError(requestWithId, error)),
                    error => HandleRequestIdValidationError(request, error));

        private void HandleRequestIdValidationError(IncomingRequest request, Error error) =>
            clientMgr.SendErrorResponse(request, failureActionCode, new Error($"{whatThisHandlerDoes} failed: {error.Message}"));

        protected virtual void HandleValidationError(IncomingRequestWithId request, Error error) =>
            clientMgr.SendErrorResponse(request, failureActionCode, new Error($"{whatThisHandlerDoes} failed: {error.Message}"));

        protected void SendSuccessResponse(IncomingRequestWithId originalRequest, ActionCode actionCode, string successMessage = default) =>
            clientMgr.SendSuccessResponse(originalRequest, actionCode, successMessage ?? string.Empty);

        protected void SendErrorResponse(IncomingRequestWithId originalRequest, Error errorMessage) =>
            SendErrorResponse(originalRequest, errorMessage, failureActionCode);
        
        protected void SendErrorResponse(IncomingRequestWithId originalRequest, Error errorMessage, ActionCode errorActionCode) =>
            clientMgr.SendErrorResponse(originalRequest, errorActionCode, errorMessage);

        /// <summary>
        /// Optionally implemented function for validating Json content of the incoming request
        /// </summary>
        /// <returns>Result indicating validation success or error</returns>
        protected virtual Result<Empty> ValidateContent(IncomingRequestWithId request) => Result.Success;
        
        /// <summary>
        /// Handle a validated request (with validated requestId and passed ValidateContent)
        /// </summary>
        /// <param name="request">Validated request to handle</param>
        protected abstract void Handle(IncomingRequestWithId request);
    }
}

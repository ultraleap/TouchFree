using System.Collections.Concurrent;
using System.Linq;

namespace Ultraleap.TouchFree.Library.Connections.MessageQueues;

public abstract class MessageQueueHandler : IMessageQueueHandler
{
    public abstract ActionCode[] HandledActionCodes { get; }

    private readonly ConcurrentQueue<IncomingRequest> _queue = new();
    protected readonly IClientConnectionManager clientMgr;

    protected abstract string WhatThisHandlerDoes { get; }
    protected abstract ActionCode FailureActionCode { get; }

    protected MessageQueueHandler(IUpdateBehaviour updateBehaviour, IClientConnectionManager clientMgr)
    {
        updateBehaviour.OnUpdate += OnUpdate;
        this.clientMgr = clientMgr;
    }

    public void AddItemToQueue(in IncomingRequest request)
    {
        if (!HandledActionCodes.Contains(request.ActionCode))
        {
            throw new System.ArgumentException("Unexpected action type", nameof(request));
        }
            
        _queue.Enqueue(request);
    }

    protected virtual void OnUpdate() => CheckQueue();

    private void CheckQueue()
    {
        if (_queue.IsEmpty) return;
            
        // Parse newly received messages
        if (!_queue.TryDequeue(out var content)) return;
        HandleRequest(content);
    }

    private void HandleRequest(IncomingRequest request) =>
        request.DeserializeAndValidateRequestId()
            .Match(requestWithId => ValidateContent(requestWithId)
                    .Match(_ => Handle(requestWithId),
                        error => HandleValidationError(requestWithId, error)),
                error => HandleRequestIdValidationError(request, error));

    private void HandleRequestIdValidationError(in IncomingRequest request, in Error error) =>
        clientMgr.SendErrorResponse(request, FailureActionCode, new Error($"{WhatThisHandlerDoes} failed: {error.Message}"));

    protected virtual void HandleValidationError(in IncomingRequestWithId request, in Error error) =>
        clientMgr.SendErrorResponse(request, FailureActionCode, new Error($"{WhatThisHandlerDoes} failed: {error.Message}"));

    protected void SendSuccessResponse(in IncomingRequestWithId originalRequest, in ActionCode actionCode, in string successMessage = default) =>
        clientMgr.SendSuccessResponse(originalRequest, actionCode, successMessage ?? string.Empty);

    protected void SendErrorResponse(in IncomingRequestWithId originalRequest, in Error errorMessage) =>
        SendErrorResponse(originalRequest, errorMessage, FailureActionCode);
        
    protected void SendErrorResponse(in IncomingRequestWithId originalRequest, in Error errorMessage, in ActionCode errorActionCode) =>
        clientMgr.SendErrorResponse(originalRequest, errorActionCode, errorMessage);

    /// <summary>
    /// Optionally implemented function for validating Json content of the incoming request
    /// </summary>
    /// <returns>Result indicating validation success or error</returns>
    protected virtual Result<Empty> ValidateContent(in IncomingRequestWithId request) => Result.Success;
        
    /// <summary>
    /// Handle a validated request (with validated requestId and passed ValidateContent)
    /// </summary>
    /// <param name="request">Validated request to handle</param>
    protected abstract void Handle(in IncomingRequestWithId request);
}
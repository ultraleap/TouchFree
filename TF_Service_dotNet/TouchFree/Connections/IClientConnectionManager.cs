using System;
using System.Net.WebSockets;

namespace Ultraleap.TouchFree.Library.Connections;

public interface IClientConnectionManager
{
    HandPresenceEvent MissedHandPresenceEvent { get; }
    InteractionZoneEvent MissedInteractionZoneEvent { get; }
    void SendInputAction(InputAction inputAction);
    void SendHandData(HandFrame handFrame, ArraySegment<byte> lastHandData);
    void AddConnection(IClientConnection clientConnection);
    void RemoveConnection(WebSocket webSocket);
    void SendResponse<T>(T response, ActionCode actionCode);
    void HandleInteractionZoneEvent(InteractionZoneState interactionZoneState);
}

public static class ClientConnectionManagerExtensions
{
    public static readonly string SuccessString = "Success";
    public static readonly string FailureString = "Failure";
        
    public static void SendSuccessResponse(this IClientConnectionManager manager, in IncomingRequestWithId incomingRequestWithId, in ActionCode responseActionCode, string message = default)
    {
        var response = new ResponseToClient(incomingRequestWithId.RequestId, SuccessString, message ?? string.Empty, incomingRequestWithId.OriginalContent);
        manager.SendResponse(response, responseActionCode);
    }

    public static void SendErrorResponse(this IClientConnectionManager manager, in IncomingRequest incomingRequest, in ActionCode responseActionCode, in Error error)
    {
        var response = new ResponseToClient(string.Empty, FailureString, error.Message, incomingRequest.Content);
        manager.SendResponse(response, responseActionCode);
    }
        
    public static void SendErrorResponse(this IClientConnectionManager manager, IncomingRequestWithId incomingRequestWithId, ActionCode responseActionCode, Error error)
    {
        var response = new ResponseToClient(incomingRequestWithId.RequestId, FailureString, error.Message, incomingRequestWithId.OriginalContent);
        manager.SendResponse(response, responseActionCode);
    }
}
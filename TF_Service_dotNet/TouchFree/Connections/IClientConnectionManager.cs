using System;
using System.Net.WebSockets;

namespace Ultraleap.TouchFree.Library.Connections
{
    public interface IClientConnectionManager
    {
        HandPresenceEvent MissedHandPresenceEvent { get; }
        InteractionZoneEvent MissedInteractionZoneEvent { get; }
        void SendInputAction(InputAction _data);
        void SendHandData(HandFrame _data, ArraySegment<byte> lastHandData);
        void AddConnection(IClientConnection _connection);
        void RemoveConnection(WebSocket _socket);
        void SendResponse<T>(T _response, ActionCode _actionCode);
        void HandleInteractionZoneEvent(InteractionZoneState _state);
    }

    public static class ClientConnectionManagerExtensions
    {
        public static readonly string SuccessString = "Success";
        public static readonly string FailureString = "Failure";
        
        public static void SendSuccessResponse(this IClientConnectionManager manager, IncomingRequestWithId incomingRequestWithId, ActionCode responseActionCode, string message = default)
        {
            var response = new ResponseToClient(incomingRequestWithId.RequestId, SuccessString, message ?? string.Empty, incomingRequestWithId.OriginalContent);
            manager.SendResponse(response, responseActionCode);
        }

        public static void SendErrorResponse(this IClientConnectionManager manager, IncomingRequest incomingRequest, ActionCode responseActionCode, Error error)
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
}

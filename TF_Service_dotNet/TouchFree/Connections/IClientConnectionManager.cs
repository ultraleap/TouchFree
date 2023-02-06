using System;
using System.Net.WebSockets;
using Ultraleap.TouchFree.Library.Connections;

namespace Ultraleap.TouchFree.Library
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
}

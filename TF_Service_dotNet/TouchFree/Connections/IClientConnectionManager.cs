using System.Net.WebSockets;
using Ultraleap.TouchFree.Library.Connections;

namespace Ultraleap.TouchFree.Library
{
    public interface IClientConnectionManager
    {
        HandPresenceEvent MissedHandPresenceEvent { get; }
        void SendInputAction(InputAction _data);
        void SendHandData(HandFrame _data);
        void SendCloseToSwipe();
        void AddConnection(IClientConnection _connection);
        void RemoveConnection(WebSocket _socket);
        void SendResponse<T>(T _response, ActionCode _actionCode);
    }
}

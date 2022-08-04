using System.Net.WebSockets;

namespace Ultraleap.TouchFree.Library
{
    public interface IClientConnectionManager
    {
        HandPresenceEvent MissedHandPresenceEvent { get; }
        void SendInputActionToWebsocket(InputAction _data);
        void AddConnection(IClientConnection _connection);
        void RemoveConnection(WebSocket _socket);
    }
}

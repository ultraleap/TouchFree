using System.Net.WebSockets;
using Ultraleap.TouchFree.Library.Connection;

namespace Ultraleap.TouchFree.Library
{
    public interface IClientConnectionManager
    {
        HandPresenceEvent MissedHandPresenceEvent { get; }
        void SendInputActionToWebsocket(InputAction _data);
        void SendHandDataToWebsocket(HandFrame _data);
        void AddConnection(IClientConnection _connection);
        void RemoveConnection(WebSocket _socket);
        void SendConfigChangeResponse(ResponseToClient response);
        void SendConfigState(ConfigState currentConfig);
        void SendConfigFile(ConfigState currentConfig);
        void SendQuickSetupConfigFile(ConfigState currentConfig);
        void SendQuickSetupResponse(ResponseToClient response);
        void SendStatusResponse(ResponseToClient response);
        void SendHandDataStreamStateResponse(ResponseToClient response);
        void SendStatus(ServiceStatus currentConfig);
        void SendConfigFileChangeResponse(ResponseToClient response);
        void SendTrackingResponse<T>(T response, ActionCode action);
    }
}

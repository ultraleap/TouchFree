using System.Net.WebSockets;
using Ultraleap.TouchFree.Library.Connections;

namespace Ultraleap.TouchFree.Library
{
    public interface IClientConnection
    {
        WebSocket Socket { get; }

        void SendInputAction(InputAction data);
        void SendHandData(HandFrame data);
        void SendConfigChangeResponse(ResponseToClient response);
        void SendConfigFileChangeResponse(ResponseToClient response);
        void SendConfigState(ConfigState config);
        void SendConfigFile(ConfigState config);
        void SendStatusResponse(ResponseToClient response);
        void SendStatus(ServiceStatus response);
        void SendQuickSetupConfigFile(ConfigState config);
        void SendQuickSetupResponse(ResponseToClient response);
        void SendTrackingState(TrackingApiState state);
        void SendHandDataStreamStateResponse(ResponseToClient response);
        void SendHandPresenceEvent(HandPresenceEvent handsLostEvent);
    }
}

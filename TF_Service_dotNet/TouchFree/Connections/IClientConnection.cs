using System.Net.WebSockets;
using Ultraleap.TouchFree.Library.Connections;

namespace Ultraleap.TouchFree.Library
{
    public interface IClientConnection
    {
        WebSocket Socket { get; }

        void SendInputAction(InputAction data);
        void SendHandData(HandFrame data, byte[] lastHandData);
        void SendHandPresenceEvent(HandPresenceEvent handsLostEvent);
        void SendResponse<T>(T _response, ActionCode actionCode);
    }
}

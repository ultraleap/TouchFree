using System;
using System.Net.WebSockets;

namespace Ultraleap.TouchFree.Library.Connections;

public interface IClientConnection
{
    WebSocket Socket { get; }

    void SendInputAction(in InputAction inputAction);
    void SendHandData(in HandFrame handFrame, in ArraySegment<byte> lastHandData);
    void SendHandPresenceEvent(in HandPresenceEvent handPresenceEvent);
    void SendInteractionZoneEvent(in InteractionZoneEvent interactionZoneEvent);
    void SendResponse<T>(in T response, in ActionCode actionCode);
}
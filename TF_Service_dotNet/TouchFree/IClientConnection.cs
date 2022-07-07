using System.Net.WebSockets;

namespace Ultraleap.TouchFree.Library
{
    public interface IClientConnection
    {
        WebSocket Socket { get; }
    }
}

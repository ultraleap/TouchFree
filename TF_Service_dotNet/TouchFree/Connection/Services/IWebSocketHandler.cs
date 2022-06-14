using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Ultraleap.TouchFree.Library.Connection
{
    public interface IWebSocketHandler
    {
        Task HandleWebSocket(WebSocket webSocket);
    }
}

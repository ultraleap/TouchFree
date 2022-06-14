using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Library.Connection;

namespace Ultraleap.TouchFree.Service.Connection
{
    public class TouchFreeRouter
    {
        private readonly RequestDelegate next;
        private readonly IWebSocketHandler webSocketHandler;

        public TouchFreeRouter(RequestDelegate _next, IWebSocketHandler _webSocketHandler)
        {
            next = _next;
            webSocketHandler = _webSocketHandler;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                await webSocketHandler.HandleWebSocket(webSocket);
            }
            else
            {
                TouchFreeLog.WriteLine("A request was made to the server that was not an attempt to connect to a WebSocket?");
                await next(context);
            }
        }
    }
}
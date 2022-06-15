using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Ultraleap.TouchFree.Library;
using Ultraleap.TouchFree.Library.Connection;

namespace Ultraleap.TouchFree.Service.Connection
{
    public class TouchFreeRouter
    {
        private readonly RequestDelegate next;
        private readonly IWebSocketHandler webSocketHandler;
        private readonly ITouchFreeLogger logger;

        public TouchFreeRouter(RequestDelegate _next, IWebSocketHandler _webSocketHandler, ITouchFreeLogger _logger)
        {
            next = _next;
            webSocketHandler = _webSocketHandler;
            logger = _logger;
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
                logger.WriteLine("A request was made to the server that was not an attempt to connect to a WebSocket?");
                await next(context);
            }
        }
    }
}
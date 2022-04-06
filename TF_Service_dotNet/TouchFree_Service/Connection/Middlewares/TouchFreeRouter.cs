using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Ultraleap.TouchFree.Library.Configuration;

namespace Ultraleap.TouchFree.Service.Connection
{
    public class TouchFreeRouter
    {
        private readonly RequestDelegate next;
        private readonly ClientConnectionManager clientMgr;
        private readonly WebSocketReceiver receiver;
        private readonly ConfigManager configManager;

        public TouchFreeRouter(RequestDelegate _next, ClientConnectionManager _clientMgr, WebSocketReceiver _receiver, ConfigManager _configManager)
        {
            next = _next;
            clientMgr = _clientMgr;
            receiver = _receiver;
            configManager = _configManager;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();

                ClientConnection connection = new ClientConnection(webSocket, receiver, clientMgr, configManager);
                clientMgr.AddConnection(connection);

                Console.WriteLine("WebSocket Connected");

                await Receive(webSocket, async (result, buffer) =>
                {
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                        connection.OnMessage(message);
                        return;
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        clientMgr.RemoveConnection(webSocket);

                        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);

                        Console.WriteLine("Websocket Connection Closed");

                        return;
                    }
                });
            }
            else
            {
                Console.WriteLine("A request was made to the server that was not an attempt to conenct to a WebSocket?");
                await next(context);
            }
        }

        private async Task Receive(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
        {
            var buffer = new byte[1024 * 4];

            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(buffer: new ArraySegment<byte>(buffer),
                                                       cancellationToken: CancellationToken.None);

                handleMessage(result, buffer);
            }
        }
    }
}
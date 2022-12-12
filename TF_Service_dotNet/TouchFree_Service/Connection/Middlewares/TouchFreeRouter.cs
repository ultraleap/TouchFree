using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ultraleap.TouchFree.Library;
using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Library.Connections;
using Ultraleap.TouchFree.Library.Connections.MessageQueues;

namespace Ultraleap.TouchFree.Service.Connection
{
    public class TouchFreeRouter
    {
        private readonly RequestDelegate next;
        private readonly IClientConnectionManager clientMgr;
        private readonly IEnumerable<IMessageQueueHandler> messageQueueHandlers;
        private readonly IConfigManager configManager;

        public TouchFreeRouter(RequestDelegate _next, IClientConnectionManager _clientMgr, IEnumerable<IMessageQueueHandler> _messageQueueHandlers, IConfigManager _configManager)
        {
            next = _next;
            clientMgr = _clientMgr;
            messageQueueHandlers = _messageQueueHandlers;
            configManager = _configManager;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();

                ClientConnection connection = new ClientConnection(webSocket, messageQueueHandlers, clientMgr, configManager);
                clientMgr.AddConnection(connection);

                TouchFreeLog.WriteLine("WebSocket Connected");

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

                        TouchFreeLog.WriteLine("Websocket Connection Closed");

                        return;
                    }
                });
            }
            else
            {
                TouchFreeLog.WriteLine("A request was made to the server that was not an attempt to connect to a WebSocket?");
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
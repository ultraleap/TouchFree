using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Library.Connections;
using Ultraleap.TouchFree.Library.Connections.MessageQueues;

namespace Ultraleap.TouchFree.Service.Connection.Middlewares;

public class TouchFreeRouter
{
    private readonly RequestDelegate _next;
    private readonly IClientConnectionManager _clientMgr;
    private readonly IEnumerable<IMessageQueueHandler> _messageQueueHandlers;
    private readonly IConfigManager _configManager;

    public TouchFreeRouter(RequestDelegate next, IClientConnectionManager clientMgr, IEnumerable<IMessageQueueHandler> messageQueueHandlers, IConfigManager configManager)
    {
        _next = next;
        _clientMgr = clientMgr;
        _messageQueueHandlers = messageQueueHandlers;
        _configManager = configManager;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();

            ClientConnection connection = new ClientConnection(webSocket, _messageQueueHandlers, _clientMgr, _configManager);
            _clientMgr.AddConnection(connection);

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
                    _clientMgr.RemoveConnection(webSocket);

                    await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);

                    TouchFreeLog.WriteLine("Websocket Connection Closed");

                    return;
                }
            });
        }
        else
        {
            TouchFreeLog.WriteLine("A request was made to the server that was not an attempt to connect to a WebSocket?");
            await _next(context);
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
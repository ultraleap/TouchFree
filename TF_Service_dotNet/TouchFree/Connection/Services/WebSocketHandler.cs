using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Ultraleap.TouchFree.Library.Configuration;

namespace Ultraleap.TouchFree.Library.Connection
{
    public class WebSocketHandler : IWebSocketHandler
    {
        private readonly ClientConnectionManager clientMgr;
        private readonly WebSocketReceiver receiver;
        private readonly ConfigManager configManager;
        private readonly ITouchFreeLogger logger;

        public WebSocketHandler(
            ClientConnectionManager _clientMgr,
            WebSocketReceiver _receiver,
            ConfigManager _configManager,
            ITouchFreeLogger _logger)
        {
            clientMgr = _clientMgr;
            receiver = _receiver;
            configManager = _configManager;
            logger = _logger;
        }

        public async Task HandleWebSocket(WebSocket webSocket)
        {
            ClientConnection connection = new ClientConnection(webSocket, receiver, clientMgr, configManager, logger);
            clientMgr.AddConnection(connection);

            logger.WriteLine("WebSocket Connected");

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

                    logger.WriteLine("Websocket Connection Closed");

                    return;
                }
            });
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

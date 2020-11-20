using UnityEngine;

using WebSocketSharp.Server;

using Ultraleap.ScreenControl.Core;
using Ultraleap.ScreenControl.Core.ScreenControlTypes;

namespace Ultraleap.ScreenControl.Service
{
    public class WebsocketClientConnection : MonoBehaviour
    {
        private WebSocketServer wsServer = null;
        private ScreenControlWsBehaviour socketBehaviour;

        private bool websocketInitalised = false;

        public short port = 9739;

        internal WebsocketClientConnection()
        {
            InteractionManager.HandleInputAction += SendDataToWebsocket;
        }

        ~WebsocketClientConnection()
        {
            InteractionManager.HandleInputAction -= SendDataToWebsocket;
        }

        private void SetupConnection(ScreenControlWsBehaviour behaviour)
        {
            if (behaviour != null)
            {
                socketBehaviour = behaviour;
                websocketInitalised = true;
                Debug.Log("connection set up");
            }
        }

        void SendDataToWebsocket(CoreInputAction _data)
        {
            if (!websocketInitalised)
            {
                return;
            }

            if (wsServer == null ||
                !wsServer.IsListening)
            {
                wsServer = new WebSocketServer($"ws://127.0.0.1:{port}");
                wsServer.AddWebSocketService<ScreenControlWsBehaviour>("/connect", SetupConnection);

                wsServer.AllowForwardedRequest = true;
                wsServer.ReuseAddress = true;
                wsServer.Start();
            }

            socketBehaviour.SendInputAction(_data);
        }
    }
}
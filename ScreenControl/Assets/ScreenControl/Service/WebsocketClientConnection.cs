using UnityEngine;

using WebSocketSharp;
using WebSocketSharp.Server;

using Ultraleap.ScreenControl.Core;
using Ultraleap.ScreenControl.Core.ScreenControlTypes;
using Ultraleap.ScreenControl.Service.ScreenControlTypes;

namespace Ultraleap.ScreenControl.Service
{
    public class WebsocketClientConnection : MonoBehaviour
    {
        public static WebsocketClientConnection Instance;

        private WebSocketServer wsServer = null;
        private ScreenControlWsBehaviour socketBehaviour = null;
        public WebSocketReceiverHandler receiverQueue;

        private bool websocketInitalised = false;
        private bool restartServer = false;

        public short port = 9739;

        private void Awake()
        {
            Instance = this;
        }

        void OnEnable()
        {
            InitialiseServer();
        }

        internal WebsocketClientConnection()
        {
            InteractionManager.HandleInputAction += SendInputActionToWebsocket;
        }

        ~WebsocketClientConnection()
        {
            InteractionManager.HandleInputAction -= SendInputActionToWebsocket;
        }

        private void SetupConnection(ScreenControlWsBehaviour behaviour)
        {
            if (behaviour != null)
            {
                socketBehaviour = behaviour;
                socketBehaviour.clientConnection = this;
                Debug.Log("connection set up");
            }
        }

        private void InitialiseServer()
        {
            websocketInitalised = false;

            wsServer = new WebSocketServer($"ws://127.0.0.1:{port}");
            wsServer.AddWebSocketService<ScreenControlWsBehaviour>("/connect", SetupConnection);

            wsServer.AllowForwardedRequest = true;
            wsServer.ReuseAddress = true;
            wsServer.Start();

            receiverQueue = gameObject.GetComponent<WebSocketReceiverHandler>();

            if (receiverQueue == null)
            {
                receiverQueue = gameObject.AddComponent<WebSocketReceiverHandler>();
            }
        }

        void SendInputActionToWebsocket(CoreInputAction _data)
        {
            // if IsListening stops being true the server
            // has aborted / stopped, so needs remaking
            if (wsServer == null ||
                (!wsServer.IsListening && websocketInitalised))
            {
                InitialiseServer();
            }

            if (wsServer.IsListening) {
                websocketInitalised = true;
            }

            if (!websocketInitalised ||
                socketBehaviour == null ||
                socketBehaviour.ConnectionState != WebSocketState.Open)
            {
                return;
            }

            socketBehaviour.SendInputAction(_data);
        }

        public void SendConfigurationResponse(ConfigResponse _response)
        {
            socketBehaviour.SendConfigurationResponse(_response);
        }
    }
}
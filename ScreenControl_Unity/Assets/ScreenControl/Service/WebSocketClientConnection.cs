using UnityEngine;

using WebSocketSharp;
using WebSocketSharp.Server;

using Ultraleap.ScreenControl.Core;
using Ultraleap.ScreenControl.Core.ScreenControlTypes;
using Ultraleap.ScreenControl.Service.ScreenControlTypes;

namespace Ultraleap.ScreenControl.Service
{
    [RequireComponent(typeof(WebSocketReceiver)), DisallowMultipleComponent]
    public class WebSocketClientConnection : MonoBehaviour
    {
        public static WebSocketClientConnection Instance;

        private WebSocketServer wsServer = null;
        private ScreenControlWsBehaviour socketBehaviour = null;
        public WebSocketReceiver receiverQueue;

        public short port = 9739;

        private bool websocketInitalised = false;

        private void Awake()
        {
            Application.targetFrameRate = 60;

            Instance = this;
            InteractionManager.HandleInputAction += Instance.SendInputActionToWebsocket;
        }

        void OnEnable()
        {
            InitialiseServer();
        }
        void Destroy()
        {
            InteractionManager.HandleInputAction -= Instance.SendInputActionToWebsocket;
        }

        private void SetupConnection(ScreenControlWsBehaviour behaviour)
        {
            if (behaviour != null)
            {
                socketBehaviour = behaviour;
                Debug.Log("connection set up");
            }
        }

        private void InitialiseServer()
        {
            websocketInitalised = false;

            receiverQueue = GetComponent<WebSocketReceiver>();

            wsServer = new WebSocketServer($"ws://127.0.0.1:{port}");
            wsServer.AddWebSocketService<ScreenControlWsBehaviour>("/connect", SetupConnection);

            wsServer.AllowForwardedRequest = true;
            wsServer.ReuseAddress = true;
            wsServer.Start();

            // This is here so the test infrastructure has some sign that the app is ready
            Debug.Log("Service Setup Complete");
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

        public void SendHandshakeResponse(ResponseToClient _response) {
            socketBehaviour.SendConfigurationResponse(_response);
        }

        public void SendConfigurationResponse(ResponseToClient _response)
        {
            socketBehaviour.SendConfigurationResponse(_response);
        }
    }
}
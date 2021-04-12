using UnityEngine;
using System.Collections.Generic;

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
        private List<ScreenControlWsBehaviour> activeConnections = new List<ScreenControlWsBehaviour>();
        public WebSocketReceiver receiverQueue;

        public short port = 9739;

        private bool websocketInitalised = false;

        private void Awake()
        {
            Application.targetFrameRate = 60;

            Instance = this;
            InteractionManager.HandleInputAction += Instance.SendInputActionToWebsocket;
            InitialiseServer();
        }

        void OnDestroy()
        {
            InteractionManager.HandleInputAction -= Instance.SendInputActionToWebsocket;
        }

        private void SetupConnection(ScreenControlWsBehaviour behaviour)
        {
            if (behaviour != null)
            {
                activeConnections.Add(behaviour);
                Debug.Log("Connection set up");
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

            if (wsServer.IsListening)
            {
                websocketInitalised = true;
            }

            if (!websocketInitalised ||
                activeConnections == null ||
                activeConnections.Count < 1)
            {
                return;
            }

            foreach(ScreenControlWsBehaviour behaviour in activeConnections)
            {
                if (behaviour.ConnectionState == WebSocketState.Open)
                {
                    behaviour.SendInputAction(_data);
                }
            }
        }

        public void SendConfigChangeResponse(ResponseToClient _response)
        {
            foreach(ScreenControlWsBehaviour behaviour in activeConnections)
            {
                if (behaviour.ConnectionState == WebSocketState.Open)
                {
                    behaviour.SendConfigChangeResponse(_response);
                }
            }
        }

        public void SendConfigState(ConfigState _config)
        {
            foreach (ScreenControlWsBehaviour behaviour in activeConnections)
            {
                if (behaviour.ConnectionState == WebSocketState.Open)
                {
                    behaviour.SendConfigState(_config);
                }
            }
        }
    }
}
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
    public class ClientConnectionManager : MonoBehaviour
    {
        public static ClientConnectionManager Instance;

        private WebSocketServer wsServer = null;
        private List<ClientConnection> activeConnections = new List<ClientConnection>();
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

        private void OnHandFound()
        {
            foreach (ClientConnection _connection in activeConnections)
            {
                if (_connection.ConnectionState == WebSocketState.Open)
                {
                    HandPresenceEvent handFoundEvent = new HandPresenceEvent(HandPresenceState.HAND_FOUND);

                    _connection.SendHandPresenceEvent(handFoundEvent);
                }
            }
        }
        private void OnHandsLost()
        {
            foreach (ClientConnection _connection in activeConnections)
            {
                if (_connection.ConnectionState == WebSocketState.Open)
                {
                    HandPresenceEvent handsLostEvent = new HandPresenceEvent(HandPresenceState.HANDS_LOST);

                    _connection.SendHandPresenceEvent(handsLostEvent);
                }
            }
        }

        void OnDestroy()
        {
            InteractionManager.HandleInputAction -= Instance.SendInputActionToWebsocket;
        }

        private void SetupConnection(ClientConnection _connection)
        {
            if (_connection != null)
            {
                activeConnections.Add(_connection);
                Debug.Log("Connection set up");
            }
        }

        internal void RemoveConnection(ClientConnection _connection)
        {
            activeConnections.Remove(_connection);
        }

        private void InitialiseServer()
        {
            websocketInitalised = false;

            receiverQueue = GetComponent<WebSocketReceiver>();

            wsServer = new WebSocketServer($"ws://127.0.0.1:{port}");
            wsServer.AddWebSocketService<ClientConnection>("/connect", SetupConnection);

            wsServer.AllowForwardedRequest = true;
            wsServer.ReuseAddress = true;
            wsServer.Start();

            HandManager.Instance.HandFound += OnHandFound;
            HandManager.Instance.HandsLost += OnHandsLost;

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

            foreach (ClientConnection connection in activeConnections)
            {
                if (connection.ConnectionState == WebSocketState.Open)
                {
                    connection.SendInputAction(_data);
                }
            }
        }

        public void SendConfigChangeResponse(ResponseToClient _response)
        {
            foreach (ClientConnection connection in activeConnections)
            {
                if (connection.ConnectionState == WebSocketState.Open)
                {
                    connection.SendConfigChangeResponse(_response);
                }
            }
        }

        public void SendConfigState(ConfigState _config)
        {
            foreach (ClientConnection connection in activeConnections)
            {
                if (connection.ConnectionState == WebSocketState.Open)
                {
                    connection.SendConfigState(_config);
                }
            }
        }
    }
}
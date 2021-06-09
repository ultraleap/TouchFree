using System;

using UnityEngine;
using System.Collections.Generic;

using WebSocketSharp;
using WebSocketSharp.Server;

using Ultraleap.TouchFree.ServiceShared;
using Ultraleap.TouchFree.Service.ServiceTypes;

namespace Ultraleap.TouchFree.Service
{
    [RequireComponent(typeof(WebSocketReceiver)), DisallowMultipleComponent]
    public class ClientConnectionManager : MonoBehaviour
    {
        public static ClientConnectionManager Instance;

        private WebSocketServer wsServer = null;
        private List<ClientConnection> activeConnections = new List<ClientConnection>();
        public WebSocketReceiver receiverQueue;

        public event Action LostAllConnections;

        public short port = 9739;

        private bool websocketInitalised = false;

        internal Nullable<HandPresenceEvent> missedHandPresenceEvent = null;

        private void Awake()
        {
            UpdateApplicationFrameRate();
            InteractionConfig.OnConfigUpdated += UpdateApplicationFrameRate;

            Instance = this;
            InteractionManager.HandleInputAction += Instance.SendInputActionToWebsocket;
            InitialiseServer();
        }

        private void OnHandFound()
        {
            HandPresenceEvent handFoundEvent = new HandPresenceEvent(HandPresenceState.HAND_FOUND);

            foreach (ClientConnection _connection in activeConnections)
            {
                if (_connection.ConnectionState == WebSocketState.Open)
                {
                    _connection.SendHandPresenceEvent(handFoundEvent);
                }
            }

            // Cache handPresenceEvent when no clients are connected
            if (activeConnections.Count == 0)
            {
                missedHandPresenceEvent = handFoundEvent;
            }
        }
        private void OnHandsLost()
        {
            HandPresenceEvent handsLostEvent = new HandPresenceEvent(HandPresenceState.HANDS_LOST);

            foreach (ClientConnection _connection in activeConnections)
            {
                if (_connection.ConnectionState == WebSocketState.Open)
                {
                    _connection.SendHandPresenceEvent(handsLostEvent);
                }
            }

            // Cache handPresenceEvent when no clients are connected
            if (activeConnections.Count == 0)
            {
                missedHandPresenceEvent = handsLostEvent;
            }
        }

        void OnDestroy()
        {
            InteractionManager.HandleInputAction -= Instance.SendInputActionToWebsocket;
            InteractionConfig.OnConfigUpdated -= UpdateApplicationFrameRate;
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

            if (activeConnections.Count < 1)
            {
                // there are no connections
                LostAllConnections?.Invoke();
            }
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

        void SendInputActionToWebsocket(InputAction _data)
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

        public void UpdateApplicationFrameRate()
        {
            if (ConfigManager.InteractionConfig.ServiceUpdateRate > 0)
            {
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = Mathf.Clamp(ConfigManager.InteractionConfig.ServiceUpdateRate, 1, 100);
            }
            else
            {
                Application.targetFrameRate = 60;
                QualitySettings.vSyncCount = 1;
            }
        }
    }
}
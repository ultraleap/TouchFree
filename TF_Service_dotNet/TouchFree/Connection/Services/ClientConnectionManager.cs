using System;
using System.Collections.Generic;
using System.Net.WebSockets;

using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Library.ConnectionTypes;


namespace Ultraleap.TouchFree.Library.Connection
{
    public class ClientConnectionManager
    {
        // TODO:
        // * Dependency Inject the InteractionManager reference

        private List<ClientConnection> activeConnections = new List<ClientConnection>();

        public event Action LostAllConnections;

        public short port = 9739;

        internal HandPresenceEvent missedHandPresenceEvent = new HandPresenceEvent(HandPresenceState.HANDS_LOST);

        public HandManager handManager;
        private readonly ITouchFreeLogger logger;

        public ClientConnectionManager(HandManager _handManager, ITouchFreeLogger _logger)
        {
            // InteractionManager.HandleInputAction += Instance.SendInputActionToWebsocket;
            handManager = _handManager;
            logger = _logger;
            handManager.HandFound += OnHandFound;
            handManager.HandsLost += OnHandsLost;

            // This is here so the test infrastructure has some sign that the app is ready
            logger.WriteLine("Service Setup Complete");
        }

        ~ClientConnectionManager()
        {
            //InteractionManager.HandleInputAction -= Instance.SendInputActionToWebsocket;
        }

        private void OnHandFound()
        {
            HandPresenceEvent handFoundEvent = new HandPresenceEvent(HandPresenceState.HAND_FOUND);

            foreach (ClientConnection _connection in activeConnections)
            {
                if (_connection.socket.State == WebSocketState.Open)
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
                if (_connection.socket.State == WebSocketState.Open)
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

        public void AddConnection(ClientConnection _connection)
        {
            if (_connection != null)
            {
                activeConnections.Add(_connection);
                logger.WriteLine("Connection set up");
                handManager.ConnectToTracking();
            }
        }

        public void RemoveConnection(WebSocket _socket)
        {
            ClientConnection connectionToRemove = null;

            foreach (var connection in activeConnections)
            {
                if (connection.socket == _socket)
                {
                    connectionToRemove = connection;
                    break;
                }
            }

            if (connectionToRemove != null)
            {
                activeConnections.Remove(connectionToRemove);
                logger.WriteLine("Connection closed");
            }
            else
            {
                logger.WriteLine("Attempted to close a connection that was no longer active");
            }

            if (activeConnections.Count < 1)
            {
                // there are no connections
                LostAllConnections?.Invoke();
                handManager.DisconnectFromTracking();
            }
        }

        public void SendInputActionToWebsocket(InputAction _data)
        {
            if (activeConnections == null ||
                activeConnections.Count < 1)
            {
                return;
            }

            foreach (ClientConnection connection in activeConnections)
            {
                if (connection.socket.State == WebSocketState.Open)
                {
                    connection.SendInputAction(_data);
                }
            }
        }

        public void SendConfigChangeResponse(ResponseToClient _response)
        {
            foreach (ClientConnection connection in activeConnections)
            {
                if (connection.socket.State == WebSocketState.Open)
                {
                    connection.SendConfigChangeResponse(_response);
                }
            }
        }
        public void SendConfigFileChangeResponse(ResponseToClient _response)
        {
            foreach (ClientConnection connection in activeConnections)
            {
                if (connection.socket.State == WebSocketState.Open)
                {
                    connection.SendConfigFileChangeResponse(_response);
                }
            }
        }

        public void SendConfigState(ConfigState _config)
        {
            foreach (ClientConnection connection in activeConnections)
            {
                if (connection.socket.State == WebSocketState.Open)
                {
                    connection.SendConfigState(_config);
                }
            }
        }

        public void SendConfigFile(ConfigState _config)
        {
            foreach (ClientConnection connection in activeConnections)
            {
                if (connection.socket.State == WebSocketState.Open)
                {
                    connection.SendConfigFile(_config);
                }
            }
        }

        public void SendStatusResponse(ResponseToClient _response)
        {
            foreach (ClientConnection connection in activeConnections)
            {
                if (connection.socket.State == WebSocketState.Open)
                {
                    connection.SendStatusResponse(_response);
                }
            }
        }

        public void SendStatus(ServiceStatus _response)
        {
            foreach (ClientConnection connection in activeConnections)
            {
                if (connection.socket.State == WebSocketState.Open)
                {
                    connection.SendStatus(_response);
                }
            }
        }
    }
}
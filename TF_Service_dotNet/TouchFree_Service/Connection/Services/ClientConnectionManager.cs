using System;
using System.Collections.Generic;
using System.Net.WebSockets;

using Ultraleap.TouchFree.Library;
using Ultraleap.TouchFree.Service.ConnectionTypes;


namespace Ultraleap.TouchFree.Service.Connection
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

        public ClientConnectionManager(HandManager _handManager)
        {
            // InteractionManager.HandleInputAction += Instance.SendInputActionToWebsocket;
            handManager = _handManager;
            handManager.HandFound += OnHandFound;
            handManager.HandsLost += OnHandsLost;

            // This is here so the test infrastructure has some sign that the app is ready
            Console.WriteLine("Service Setup Complete");
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

        internal void AddConnection(ClientConnection _connection)
        {
            if (_connection != null)
            {
                activeConnections.Add(_connection);
                Console.WriteLine("Connection set up");
            }
        }

        internal void RemoveConnection(WebSocket _socket)
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
                Console.WriteLine("Connection closed");
            }
            else
            {
                Console.Error.WriteLine("Attempted to close a connection that was no longer active");
            }

            if (activeConnections.Count < 1)
            {
                // there are no connections
                LostAllConnections?.Invoke();
            }
        }

        internal void SendInputActionToWebsocket(InputAction _data)
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
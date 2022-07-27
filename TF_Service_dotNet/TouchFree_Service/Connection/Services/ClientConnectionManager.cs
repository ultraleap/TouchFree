using System;
using System.Collections.Generic;
using System.Net.WebSockets;

using Ultraleap.TouchFree.Library;
using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Library.Connection;


namespace Ultraleap.TouchFree.Service.Connection
{
    public class ClientConnectionManager : IClientConnectionManager
    {
        // TODO:
        // * Dependency Inject the InteractionManager reference

        private List<IClientConnection> activeConnections = new List<IClientConnection>();

        public event Action LostAllConnections;

        public short port = 9739;

        public HandPresenceEvent MissedHandPresenceEvent { get; private set; }

        public IHandManager handManager;

        public ClientConnectionManager(IHandManager _handManager)
        {
            MissedHandPresenceEvent = new HandPresenceEvent(HandPresenceState.HANDS_LOST);
            // InteractionManager.HandleInputAction += Instance.SendInputActionToWebsocket;
            handManager = _handManager;
            handManager.HandFound += OnHandFound;
            handManager.HandsLost += OnHandsLost;

            // This is here so the test infrastructure has some sign that the app is ready
            TouchFreeLog.WriteLine("Service Setup Complete");
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
                if (_connection.Socket.State == WebSocketState.Open)
                {
                    _connection.SendHandPresenceEvent(handFoundEvent);
                }
            }

            // Cache handPresenceEvent when no clients are connected
            if (activeConnections.Count == 0)
            {
                MissedHandPresenceEvent = handFoundEvent;
            }
        }

        private void OnHandsLost()
        {
            HandPresenceEvent handsLostEvent = new HandPresenceEvent(HandPresenceState.HANDS_LOST);

            foreach (ClientConnection _connection in activeConnections)
            {
                if (_connection.Socket.State == WebSocketState.Open)
                {
                    _connection.SendHandPresenceEvent(handsLostEvent);
                }
            }

            // Cache handPresenceEvent when no clients are connected
            if (activeConnections.Count == 0)
            {
                MissedHandPresenceEvent = handsLostEvent;
            }
        }

        public void AddConnection(IClientConnection _connection)
        {
            if (_connection != null)
            {
                activeConnections.Add(_connection);
                TouchFreeLog.WriteLine("Connection set up");
                handManager.ConnectToTracking();
            }
        }

        public void RemoveConnection(WebSocket _socket)
        {
            IClientConnection connectionToRemove = null;

            foreach (var connection in activeConnections)
            {
                if (connection.Socket == _socket)
                {
                    connectionToRemove = connection;
                    break;
                }
            }

            if (connectionToRemove != null)
            {
                activeConnections.Remove(connectionToRemove);
                TouchFreeLog.WriteLine("Connection closed");
            }
            else
            {
                TouchFreeLog.WriteLine("Attempted to close a connection that was no longer active");
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
                if (connection.Socket.State == WebSocketState.Open)
                {
                    connection.SendInputAction(_data);
                }
            }
        }

        public void SendHandDataToWebsocket(HandFrame _data)
        {
            if (activeConnections == null ||
                activeConnections.Count < 1)
            {
                return;
            }

            foreach (ClientConnection connection in activeConnections)
            {
                if (connection.Socket.State == WebSocketState.Open)
                {
                    connection.SendHandData(_data);
                }
            }
        }

        public void SendConfigChangeResponse(ResponseToClient _response)
        {
            foreach (ClientConnection connection in activeConnections)
            {
                if (connection.Socket.State == WebSocketState.Open)
                {
                    connection.SendConfigChangeResponse(_response);
                }
            }
        }
        public void SendConfigFileChangeResponse(ResponseToClient _response)
        {
            foreach (ClientConnection connection in activeConnections)
            {
                if (connection.Socket.State == WebSocketState.Open)
                {
                    connection.SendConfigFileChangeResponse(_response);
                }
            }
        }

        public void SendConfigState(ConfigState _config)
        {
            foreach (ClientConnection connection in activeConnections)
            {
                if (connection.Socket.State == WebSocketState.Open)
                {
                    connection.SendConfigState(_config);
                }
            }
        }

        public void SendConfigFile(ConfigState _config)
        {
            foreach (ClientConnection connection in activeConnections)
            {
                if (connection.Socket.State == WebSocketState.Open)
                {
                    connection.SendConfigFile(_config);
                }
            }
        }

        public void SendStatusResponse(ResponseToClient _response)
        {
            foreach (ClientConnection connection in activeConnections)
            {
                if (connection.Socket.State == WebSocketState.Open)
                {
                    connection.SendStatusResponse(_response);
                }
            }
        }

        public void SendStatus(ServiceStatus _response)
        {
            foreach (ClientConnection connection in activeConnections)
            {
                if (connection.Socket.State == WebSocketState.Open)
                {
                    connection.SendStatus(_response);
                }
            }
        }

        public void SendQuickSetupConfigFile(ConfigState _config)
        {
            foreach (ClientConnection connection in activeConnections)
            {
                if (connection.Socket.State == WebSocketState.Open)
                {
                    connection.SendQuickSetupConfigFile(_config);
                }
            }
        }

        public void SendQuickSetupResponse(ResponseToClient _response)
        {
            foreach (ClientConnection connection in activeConnections)
            {
                if (connection.Socket.State == WebSocketState.Open)
                {
                    connection.SendQuickSetupResponse(_response);
                }
            }
        }

        public void SendHandDataStreamStateResponse(ResponseToClient _response)
        {
            foreach (ClientConnection connection in activeConnections)
            {
                if (connection.Socket.State == WebSocketState.Open)
                {
                    connection.SendHandDataStreamStateResponse(_response);
                }
            }
        }
    }
}
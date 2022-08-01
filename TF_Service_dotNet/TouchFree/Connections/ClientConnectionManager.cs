using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;

using Ultraleap.TouchFree.Library.Configuration;

namespace Ultraleap.TouchFree.Library.Connections
{
    public class ClientConnectionManager : IClientConnectionManager
    {
        // TODO:
        // * Dependency Inject the InteractionManager reference

        private ConcurrentDictionary<Guid, IClientConnection> activeConnections = new ConcurrentDictionary<Guid, IClientConnection>();

        public IEnumerable<IClientConnection> clientConnections
        {
            get { return activeConnections.Values; }
        }

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
            HandleHandPresenceEvent(HandPresenceState.HAND_FOUND);
        }

        private void OnHandsLost()
        {
            HandleHandPresenceEvent(HandPresenceState.HANDS_LOST);
        }

        private void HandleHandPresenceEvent(HandPresenceState state)
        {
            HandPresenceEvent handsLostEvent = new HandPresenceEvent(state);

            SendMessageToWebSockets((connection) =>
            {
                connection.SendHandPresenceEvent(handsLostEvent);
            });

            // Cache handPresenceEvent when no clients are connected
            if (activeConnections.IsEmpty)
            {
                MissedHandPresenceEvent = handsLostEvent;
            }
        }

        public void AddConnection(IClientConnection _connection)
        {
            if (_connection != null)
            {
                activeConnections.TryAdd(Guid.NewGuid(), _connection);
                TouchFreeLog.WriteLine("Connection set up");
                handManager.ConnectToTracking();
            }
        }

        public void RemoveConnection(WebSocket _socket)
        {
            Guid connectionKeyToRemove = Guid.Empty;

            foreach (var connectionKvp in activeConnections)
            {
                if (connectionKvp.Value.Socket == _socket)
                {
                    connectionKeyToRemove = connectionKvp.Key;
                    break;
                }
            }

            if (connectionKeyToRemove != Guid.Empty)
            {
                _ = activeConnections.TryRemove(connectionKeyToRemove, out _);
                TouchFreeLog.WriteLine("Connection closed");
            }
            else
            {
                TouchFreeLog.WriteLine("Attempted to close a connection that was no longer active");
            }

            if (activeConnections.IsEmpty)
            {
                // there are no connections
                LostAllConnections?.Invoke();
                handManager.DisconnectFromTracking();
            }
        }

        private void SendMessageToWebSockets(Action<IClientConnection> connectionMethod)
        {
            if (activeConnections == null ||
                activeConnections.IsEmpty)
            {
                return;
            }

            foreach (IClientConnection connection in activeConnections.Values)
            {
                if (connection.Socket.State == WebSocketState.Open)
                {
                    connectionMethod(connection);
                }
                else if (connection.Socket.State != WebSocketState.Connecting)  
                {
                    // Clear down connections that have closed
                    RemoveConnection(connection.Socket);
                }
            }
        }

        public void SendInputAction(InputAction _data)
        {
            SendMessageToWebSockets((connection) =>
            {
                connection.SendInputAction(_data);
            });
        }

        public void SendHandData(HandFrame _data)
        {
            SendMessageToWebSockets((connection) =>
            {
                connection.SendHandData(_data);
            });
        }

        public void SendConfigChangeResponse(ResponseToClient _response)
        {
            SendMessageToWebSockets((connection) =>
            {
                connection.SendConfigChangeResponse(_response);
            });
        }

        public void SendConfigFileChangeResponse(ResponseToClient _response)
        {
            SendMessageToWebSockets((connection) =>
            {
                connection.SendConfigFileChangeResponse(_response);
            });
        }

        public void SendConfigState(ConfigState _config)
        {
            SendMessageToWebSockets((connection) =>
            {
                connection.SendConfigState(_config);
            });
        }

        public void SendConfigFile(ConfigState _config)
        {
            SendMessageToWebSockets((connection) =>
            {
                connection.SendConfigFile(_config);
            });
        }

        public void SendStatusResponse(ResponseToClient _response)
        {
            SendMessageToWebSockets((connection) =>
            {
                connection.SendStatusResponse(_response);
            });
        }

        public void SendStatus(ServiceStatus _response)
        {
            SendMessageToWebSockets((connection) =>
            {
                connection.SendStatus(_response);
            });
        }

        public void SendQuickSetupConfigFile(ConfigState _config)
        {
            SendMessageToWebSockets((connection) =>
            {
                connection.SendQuickSetupConfigFile(_config);
            });
        }

        public void SendQuickSetupResponse(ResponseToClient _response)
        {
            SendMessageToWebSockets((connection) =>
            {
                connection.SendQuickSetupResponse(_response);
            });
        }

        public void SendTrackingState(TrackingApiState _state)
        {
            SendMessageToWebSockets((connection) =>
            {
                connection.SendTrackingState(_state);
            });
        }

        public void SendHandDataStreamStateResponse(ResponseToClient _response)
        {
            SendMessageToWebSockets((connection) =>
            {
                connection.SendHandDataStreamStateResponse(_response);
            });
        }
    }
}
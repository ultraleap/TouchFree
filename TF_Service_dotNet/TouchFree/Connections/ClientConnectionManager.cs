using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;

using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Library.Connections.DiagnosticApi;

namespace Ultraleap.TouchFree.Library.Connections
{
    public class ClientConnectionManager : IClientConnectionManager
    {
        private readonly ConcurrentDictionary<Guid, IClientConnection> activeConnections = new ConcurrentDictionary<Guid, IClientConnection>();

        public IEnumerable<IClientConnection> ClientConnections => activeConnections.Values;

        public event Action LostAllConnections;

        public short port = 9739;

        public HandPresenceEvent MissedHandPresenceEvent { get; private set; }
        public InteractionZoneEvent MissedInteractionZoneEvent { get; private set; }

        private readonly IHandManager handManager;
        private readonly IConfigManager configManager;
        private readonly ITrackingDiagnosticApi trackingApi;

        public ClientConnectionManager(IHandManager _handManager, IConfigManager _configManager, ITrackingDiagnosticApi _trackingApi)
        {
            MissedHandPresenceEvent = new HandPresenceEvent(HandPresenceState.HANDS_LOST);
            handManager = _handManager;
            configManager = _configManager;
            trackingApi = _trackingApi;
            handManager.HandFound += OnHandFound;
            handManager.HandsLost += OnHandsLost;
            handManager.ConnectionManager.ServiceStatusChange += ConnectionStatusChange;

            // This is here so the test infrastructure has some sign that the app is ready
            TouchFreeLog.WriteLine("Service Setup Complete");
        }

        private void OnHandFound() => HandleHandPresenceEvent(HandPresenceState.HAND_FOUND);

        private void OnHandsLost() => HandleHandPresenceEvent(HandPresenceState.HANDS_LOST);

        private async void ConnectionStatusChange(TrackingServiceState state)
        {
            var deviceStatus = await trackingApi.RequestDeviceInfo();
            
            var currentStatus = new ServiceStatus(
                string.Empty, // No request id as this event is not a response to a request
                state,
                configManager.ErrorLoadingConfigFiles ? ConfigurationState.ERRORED : ConfigurationState.LOADED,
                VersionManager.Version,
                trackingApi.ApiInfo.GetValueOrDefault().ServiceVersion,
                deviceStatus.GetValueOrDefault().Serial,
                deviceStatus.GetValueOrDefault().Firmware);

            SendResponse(currentStatus, ActionCode.SERVICE_STATUS);
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

        public void HandleInteractionZoneEvent(InteractionZoneState _state)
        {
            InteractionZoneEvent interactionZoneEvent = new InteractionZoneEvent(_state);

            SendMessageToWebSockets((_connection) =>
            {
                _connection.SendInteractionZoneEvent(interactionZoneEvent);
            });

            // Cache interactionZoneEvent when no clients are connected
            if (activeConnections.IsEmpty)
            {
                MissedInteractionZoneEvent = interactionZoneEvent;
            }
        }

        public void AddConnection(IClientConnection _connection)
        {
            if (_connection != null)
            {
                activeConnections.TryAdd(Guid.NewGuid(), _connection);
                TouchFreeLog.WriteLine("Connection set up");
                handManager.ConnectionManager.Connect();
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
                handManager.ConnectionManager.Disconnect();
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

        public void SendHandData(HandFrame _data, ArraySegment<byte> lastHandData)
        {
            SendMessageToWebSockets((connection) =>
            {
                connection.SendHandData(_data, lastHandData);
            });
        }

        public void SendResponse<T>(T _response, ActionCode _actionCode)
        {
            SendMessageToWebSockets((connection) =>
            {
                connection.SendResponse(_response, _actionCode);
            });
        }
    }
}
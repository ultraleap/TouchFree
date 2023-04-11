using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Library.Connections.DiagnosticApi;

namespace Ultraleap.TouchFree.Library.Connections;

public class ClientConnectionManager : IClientConnectionManager
{
    private readonly ConcurrentDictionary<Guid, IClientConnection> _activeConnections = new();

    public IEnumerable<IClientConnection> ClientConnections => _activeConnections.Values;

    public HandPresenceEvent MissedHandPresenceEvent { get; private set; }
    public InteractionZoneEvent MissedInteractionZoneEvent { get; private set; }

    private readonly IHandManager _handManager;
    private readonly IConfigManager _configManager;
    private readonly ITrackingDiagnosticApi _trackingApi;

    public ClientConnectionManager(IHandManager handManager, IConfigManager configManager, ITrackingDiagnosticApi trackingApi)
    {
        MissedHandPresenceEvent = new HandPresenceEvent(HandPresenceState.HANDS_LOST);
        _handManager = handManager;
        _configManager = configManager;
        _trackingApi = trackingApi;
        _handManager.HandFound += OnHandFound;
        _handManager.HandsLost += OnHandsLost;
        _handManager.ConnectionManager.ServiceStatusChange += ConnectionStatusChange;

        // This is here so the test infrastructure has some sign that the app is ready
        TouchFreeLog.WriteLine("Service Setup Complete");
    }

    private void OnHandFound() => HandleHandPresenceEvent(HandPresenceState.HAND_FOUND);

    private void OnHandsLost() => HandleHandPresenceEvent(HandPresenceState.HANDS_LOST);

    private async void ConnectionStatusChange(TrackingServiceState state)
    {
        var deviceInfo = await _trackingApi.RequestDeviceInfo();
            
        var currentStatus = ServiceStatus.FromDApiTypes(string.Empty, // No request id as this event is not a response to a request
            state,
            _configManager.ErrorLoadingConfigFiles ? ConfigurationState.ERRORED : ConfigurationState.LOADED,
            VersionManager.Version,
            _trackingApi.ApiInfo,
            deviceInfo);

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
        if (_activeConnections.IsEmpty)
        {
            MissedHandPresenceEvent = handsLostEvent;
        }
    }

    public void HandleInteractionZoneEvent(InteractionZoneState interactionZoneState)
    {
        InteractionZoneEvent interactionZoneEvent = new InteractionZoneEvent(interactionZoneState);

        SendMessageToWebSockets((connection) =>
        {
            connection.SendInteractionZoneEvent(interactionZoneEvent);
        });

        // Cache interactionZoneEvent when no clients are connected
        if (_activeConnections.IsEmpty)
        {
            MissedInteractionZoneEvent = interactionZoneEvent;
        }
    }

    public void AddConnection(IClientConnection clientConnection)
    {
        if (clientConnection != null)
        {
            _activeConnections.TryAdd(Guid.NewGuid(), clientConnection);
            TouchFreeLog.WriteLine("Connection set up");
            _handManager.ConnectionManager.Connect();
        }
    }

    public void RemoveConnection(WebSocket webSocket)
    {
        Guid connectionKeyToRemove = Guid.Empty;

        foreach (var connectionKvp in _activeConnections)
        {
            if (connectionKvp.Value.Socket == webSocket)
            {
                connectionKeyToRemove = connectionKvp.Key;
                break;
            }
        }

        if (connectionKeyToRemove != Guid.Empty)
        {
            _activeConnections.TryRemove(connectionKeyToRemove, out _);
            TouchFreeLog.WriteLine("Connection closed");
        }
        else
        {
            TouchFreeLog.WriteLine("Attempted to close a connection that was no longer active");
        }

        if (_activeConnections.IsEmpty)
        {
            // there are no connections
            _handManager.ConnectionManager.Disconnect();
        }
    }

    // TODO: Change from callback style and make Send methods have 'in' parameters (can't use 'in' parameters in lambdas)
    private void SendMessageToWebSockets(Action<IClientConnection> connectionMethod)
    {
        if (_activeConnections == null ||
            _activeConnections.IsEmpty)
        {
            return;
        }

        foreach (IClientConnection connection in _activeConnections.Values)
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

    public void SendInputAction(InputAction inputAction)
    {
        SendMessageToWebSockets((connection) =>
        {
            connection.SendInputAction(inputAction);
        });
    }

    public void SendHandData(HandFrame handFrame, ArraySegment<byte> lastHandData)
    {
        SendMessageToWebSockets((connection) =>
        {
            connection.SendHandData(handFrame, lastHandData);
        });
    }

    public void SendResponse<T>(T response, ActionCode actionCode)
    {
        SendMessageToWebSockets((connection) =>
        {
            connection.SendResponse(response, actionCode);
        });
    }
}
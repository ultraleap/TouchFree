using System;
using Ultraleap.TouchFree.Library.Configuration;

namespace Ultraleap.TouchFree.Service.ConnectionTypes
{
    internal enum ActionCode
    {
        INPUT_ACTION,
        CONFIGURATION_STATE,
        CONFIGURATION_RESPONSE,
        SET_CONFIGURATION_STATE,
        REQUEST_CONFIGURATION_STATE,
        VERSION_HANDSHAKE,
        VERSION_HANDSHAKE_RESPONSE,
        HAND_PRESENCE_EVENT,
        SERVICE_STATUS,
        SERVICE_STATUS_RESPONSE,
        STATUS
    }

    public enum HandPresenceState
    {
        HAND_FOUND,
        HANDS_LOST
    }

    internal enum Compatibility
    {
        COMPATIBLE,
        SERVICE_OUTDATED,
        CLIENT_OUTDATED
    }

    public enum CameraState
    {
        NOT_CONNECTED,
        CONNECTED
    }

    public enum TrackingServiceState
    {
        UNAVAILABLE,
        RUNNING
    }

    public enum ConfigurationState
    {
        NOT_LOADED,
        LOADED,
        ERRORED
    }

    [Serializable]
    public struct ConfigState
    {
        public string requestID;
        public InteractionConfig interaction;
        public PhysicalConfig physical;

        public ConfigState(string _id, InteractionConfig _interaction, PhysicalConfig _physical)
        {
            requestID = _id;
            interaction = _interaction;
            physical = _physical;
        }
    }

    [Serializable]
    public struct ServiceStatus
    {
        public string requestID;
        public CameraState cameraState;
        public TrackingServiceState trackingServiceState;
        public ConfigurationState configurationState;

        public ServiceStatus(string _id, CameraState _cameraState, TrackingServiceState _trackingServiceState, ConfigurationState _configurationState)
        {
            requestID = _id;
            cameraState = _cameraState;
            trackingServiceState = _trackingServiceState;
            configurationState = _configurationState;
        }
    }

    [Serializable]
    public struct HandPresenceEvent
    {
        public HandPresenceState state;

        public HandPresenceEvent(HandPresenceState _state)
        {
            state = _state;
        }
    }

    [Serializable]
    public struct ResponseToClient
    {
        public string requestID;
        public string status;
        public string message;
        public string originalRequest;

        public ResponseToClient(string _id, string _status, string _msg, string _request)
        {
            requestID = _id;
            status = _status;
            message = _msg;
            originalRequest = _request;
        }
    }

    internal struct CommunicationWrapper<T>
    {
        public string action;
        public T content;

        public CommunicationWrapper(string _actionCode, T _content)
        {
            action = _actionCode;
            content = _content;
        }
    }
}
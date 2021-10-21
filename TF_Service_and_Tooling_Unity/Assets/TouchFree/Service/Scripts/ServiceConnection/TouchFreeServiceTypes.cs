using System;
using Ultraleap.TouchFree.ServiceShared;

namespace Ultraleap.TouchFree.Service.ServiceTypes
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
        CUSTOM_SETTINGS_REQUEST
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

    [Serializable]
    public struct CustomSettingsRequest
    {
        public string requestID;
        public bool multiCursorMode;

        public CustomSettingsRequest(string _id, bool _multiCursorMode)
        {
            requestID = _id;
            multiCursorMode = _multiCursorMode;
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
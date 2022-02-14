using System;
using Ultraleap.TouchFree.Library.Configuration;

namespace Ultraleap.TouchFree.Library
{
    public enum ActionCode
    {
        INPUT_ACTION,
        CONFIGURATION_STATE,
        CONFIGURATION_RESPONSE,
        SET_CONFIGURATION_STATE,
        REQUEST_CONFIGURATION_STATE,
        VERSION_HANDSHAKE,
        VERSION_HANDSHAKE_RESPONSE,
        HAND_PRESENCE_EVENT
    }

    public enum HandPresenceState
    {
        HAND_FOUND,
        HANDS_LOST,
        PROCESSED // Used only by receivers of touchfree data
    }

    public enum Compatibility
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

    public struct CommunicationWrapper<T>
    {
        public string action;
        public T content;

        public CommunicationWrapper(string _actionCode, T _content)
        {
            action = _actionCode;
            content = _content;
        }
    }

    // Struct: ResponseCallback
    // Used by <MessageReceiver> to wait for a <WebSocketResponse> from the Service. Owns an action
    // with a <WebSocketResponse> as a parameter to allow users to deal with failed
    // <WebSocketResponses>. Stores a timestamp of its creation so the response has the ability to
    // timeout if not seen within a reasonable timeframe.
    public struct ResponseCallback
    {
        public int timestamp;
        public Action<ResponseToClient> callback;

        public ResponseCallback(int _timestamp, Action<ResponseToClient> _callback)
        {
            timestamp = _timestamp;
            callback = _callback;
        }
    }

    // Struct: ConfigChangeRequest
    // Used to request the current state of the configuration on the Service. This is received as
    // a <ConfigState> which should be linked to a <ConfigStateCallback> via requestID to make
    // use of the data received.
    public struct ConfigChangeRequest
    {
        public string requestID;

        public ConfigChangeRequest(string _id)
        {
            requestID = _id;
        }
    }

    // Struct: ConfigStateCallback
    // Used by <MessageReceiver> to wait for a <ConfigState> from the Service. Owns an action
    // with a <ConfigState> as a parameter to allow users to make use of the new
    // <ConfigState>. Stores a timestamp of its creation so the response has the ability to
    // timeout if not seen within a reasonable timeframe.
    public struct ConfigStateCallback
    {
        public int timestamp;
        public Action<ConfigState> callback;

        public ConfigStateCallback(int _timestamp, Action<ConfigState> _callback)
        {
            timestamp = _timestamp;
            callback = _callback;
        }
    }
}
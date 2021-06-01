using System;

using Ultraleap.TouchFree.Tooling.Configuration;

namespace Ultraleap.TouchFree.Tooling.Connection
{
    // Enum: ActionCode
    // INPUT_ACTION - Represents standard interaction data
    // CONFIGURATION_STATE - Represents a collection of configurations from the Service
    // CONFIGURATION_RESPONSE - Represents a Success/Failure response from a SET_CONFIGURATION_STATE
    // SET_CONFIGURATION_STATE - Represents a request to set new configuration files on the Service
    // REQUEST_CONFIGURATION_STATE - Represents a request to receive a current CONFIGURATION_STATE from the Service
    internal enum ActionCode
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

    // Enum: HandPresenceState
    // HAND_FOUND - Sent when the first hand is found when no hand has been present for a moment
    // HANDS_LOST - Sent when the last observed hand is lost, meaning no more hands are observed
    // PROCESSED - Used locally to indicate that no change in state is awaiting processing. See its
    //             use in <MessageReciever> for more details.
    public enum HandPresenceState
    {
        HAND_FOUND,
        HANDS_LOST,
        PROCESSED
    }

    // Enum: Compatibility
    // COMPATIBLE - The API versions are considered compatible
    // SERVICE_OUTDATED - The API versions are considered incompatible as Service is older than Client
    // CLIENT_OUTDATED - The API versions are considered incompatible as Client is older than Service
    internal enum Compatibility
    {
        COMPATIBLE,
        SERVICE_OUTDATED,
        CLIENT_OUTDATED
    }

    // Struct HandPresenceEvent
    // This struct is the format events relating to the presence of hands (a hand being found or all
    // hands being lost) are passed across in from the Service.
    [Serializable]
    public struct HandPresenceEvent
    {
        public HandPresenceState state;

        public HandPresenceEvent(HandPresenceState _state)
        {
            state = _state;
        }
    }

    // Struct: ConfigState
    // This is the structure of data received when requesting the current state of the configuration files
    // from the Service.
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

    // Struct: WebSocketResponse
    // The structure seen when the Service responds to a request. This is to verify whether it was
    // successful or not and will include the original request if it fails, to allow for
    //  troubleshooting.
    [Serializable]
    public struct WebSocketResponse
    {
        public string requestID;
        public string status;
        public string message;
        public string originalRequest;

        public WebSocketResponse(string _id, string _status, string _msg, string _request)
        {
            requestID = _id;
            status = _status;
            message = _msg;
            originalRequest = _request;
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
        public Action<WebSocketResponse> callback;

        public ResponseCallback(int _timestamp, Action<WebSocketResponse> _callback)
        {
            timestamp = _timestamp;
            callback = _callback;
        }
    }

    // Struct: ConfigChangeRequest
    // Used to request the current state of the configuration on the Service. This is received as
    // a <ConfigState> which should be linked to a <ConfigStateCallback> via requestID to make
    // use of the data received.
    [Serializable]
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
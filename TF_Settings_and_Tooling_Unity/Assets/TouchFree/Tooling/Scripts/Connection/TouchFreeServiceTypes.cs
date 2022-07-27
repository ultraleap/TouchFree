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
    // VERSION_HANDSHAKE - Represents an outgoing message from Tooling to Service, attempting to compare API versions for compatibility
    // HAND_PRESENCE_EVENT - Represents the result coming in from the Service
    // REQUEST_SERVICE_STATUS - Represents a request to receive a current SERVICE_STATUS from the Service
    // SERVICE_STATUS_RESPONSE - Represents a Failure response from a REQUEST_SERVICE_STATUS
    // SERVICE_STATUS - Represents information about the current state of the Service
    // QUICK_SETUP - Represents a request to carry out a quick setup
    // QUICK_SETUP_CONFIG - Represents a response from a quick setup with an updated configuration
    // QUICK_SETUP_RESPONSE - Represents a Success/Failure response from quick setup that has not completed
    // GET_TRACKING_STATE - Represents a request to receive the current state of the tracking settings
    // GET_TRACKING_STATE_RESPONSE - Represents a response from a GET_TRACKING_STATE with the current state of the tracking settings
    // SET_TRACKING_STATE - Represents a request to set the current state of the tracking settings
    // SET_TRACKING_STATE_RESPONSE - Represents a response from a SET_TRACKING_STATE with the updated state of the tracking settings
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

        REQUEST_SERVICE_STATUS,
        SERVICE_STATUS_RESPONSE,
        SERVICE_STATUS,

        REQUEST_CONFIGURATION_FILE,
        CONFIGURATION_FILE_STATE,
        SET_CONFIGURATION_FILE,
        CONFIGURATION_FILE_RESPONSE,

        QUICK_SETUP,
        QUICK_SETUP_CONFIG,
        QUICK_SETUP_RESPONSE,

        GET_TRACKING_STATE,
        GET_TRACKING_STATE_RESPONSE,
        SET_TRACKING_STATE,
        SET_TRACKING_STATE_RESPONSE,
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

    // Enum: TrackingServiceState
    // UNAVAILABLE - The TouchFree services is not connected to the tracking service
    // NO_CAMERA - The TouchFree service is connected to the tracking service but there is not a camera connected
    // CONNECTED - The TouchFree service is connected to the tracking service
    public enum TrackingServiceState
    {
        UNAVAILABLE,
        NO_CAMERA,
        CONNECTED
    }

    // Enum: ConfigurationState
    // NOT_LOADED - The TouchFree configuration has not been loaded
    // LOADED - The TouchFree configuration has successfully been loaded
    // ERRORED - The TouchFree configuration errored on load
    public enum ConfigurationState
    {
        NOT_LOADED,
        LOADED,
        ERRORED
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

    // Class: ServiceStatus
    // This data structure is used to receive service status.
    //
    // When receiving a configuration from the Service this structure contains ALL status data
    [Serializable]
    public struct ServiceStatus
    {
        // Variable: requestID
        public string requestID;
        // Variable: trackingServiceState
        public TrackingServiceState trackingServiceState;
        // Variable: configurationState
        public ConfigurationState configurationState;

        public ServiceStatus(string _id, TrackingServiceState _trackingServiceState, ConfigurationState _configurationState)
        {
            requestID = _id;
            trackingServiceState = _trackingServiceState;
            configurationState = _configurationState;
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

    // Struct: ServiceStatusRequest
    // Used to request the current state of the configuration on the Service. This is received as
    // a <ServiceStatus> which should be linked to a <ServiceStatusCallback> via requestID to make
    // use of the data received.
    [Serializable]
    public struct ServiceStatusRequest
    {
        public string requestID;

        public ServiceStatusRequest(string _id)
        {
            requestID = _id;
        }
    }

    // Struct: QuickSetupRequest
    // Used to request the current state of the configuration on the Service. This is received as
    // a <ConfigState> which should be linked to a <ConfigStateCallback> via requestID to make
    // use of the data received.
    [Serializable]
    public struct QuickSetupRequest
    {
        public string requestID;
        public string position;

        public QuickSetupRequest(string _id, QuickSetupPosition _position)
        {
            requestID = _id;
            position = _position.ToString();
        }
    }

    // Struct: ServiceStatusCallback
    // Used by <MessageReceiver> to wait for a <ServiceStatus> from the Service. Owns an action
    // with a <ServiceStatus> as a parameter to allow users to make use of the new
    // <ServiceStatus>. Stores a timestamp of its creation so the response has the ability to
    // timeout if not seen within a reasonable timeframe.
    public struct ServiceStatusCallback
    {
        public int timestamp;
        public Action<ServiceStatus> callback;

        public ServiceStatusCallback(int _timestamp, Action<ServiceStatus> _callback)
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
    // Enum: QuickSetupPosition
    // The position used for quick setup
    public enum QuickSetupPosition
    {
        Top,
        Bottom
    }

    // struct: SuccessWrapper
    // Type extension for <TrackingStateResponse> to capture the success state, clarifying message and response content.
    public struct SuccessWrapper<T>
    {
        // Variable: succeeded
        bool succeeded;

        // Variable: msg
        string msg;

        // Variable: content
        T? content;
    }

    // Class: TrackingStateResponse
    // Type of the response from a GET/SET tracking state request.
    public struct TrackingStateResponse
    {
        // Variable: requestID
        string requestID;

        // Variable: mask
        SuccessWrapper<MaskData>? mask;

        // Variable: cameraOrientation
        SuccessWrapper<bool>? cameraReversed;

        // Variable: allowImages
        SuccessWrapper<bool>? allowImages;

        // Variable: analyticsEnabled
        SuccessWrapper<bool>? analyticsEnabled;
    }

    // Class: TrackingState
    // Used to construct a SET_TRACKING_STATE request.
    public class TrackingState
    {
        // Variable: requestID
        string requestID;
        // Variable: mask
        MaskData? mask;
        // Variable: cameraOrientation
        bool? cameraReversed;
        // Variable: allowImages
        bool? allowImages;
        // Variable: analyticsEnabled
        bool? analyticsEnabled;

        public TrackingState(string _id,
                       MaskData? _mask,
                       bool? _cameraReversed,
                       bool? _allowImages,
                       bool? _analyticsEnabled)
        {
            this.requestID = _id;
            this.mask = _mask;
            this.cameraReversed = _cameraReversed;
            this.allowImages = _allowImages;
            this.analyticsEnabled = _analyticsEnabled;
        }
    }

    public struct MaskData {
        public float lower;
        public float upper;
        public float right;
        public float left;
    }


    // class: SimpleRequest
    // Used to make a basic request to the service. To be used with <CommunicationWrapper> to create a more complex request.
    public struct SimpleRequest
    {
        // Variable: requestID
        string requestID;

        SimpleRequest(string _id)
        {
            this.requestID = _id;
        }
    }

    // Class: TrackingStateCallback
    // Used by <MessageReceiver> to wait for a <TrackingStateResponse> from the Service. Owns a callback with a
    // <TrackingStateResponse> as a parameter. Stores a timestamp of its creation so the response has the ability to
    // timeout if not seen within a reasonable timeframe.
    public struct TrackingStateCallback
    {
        // Variable: timestamp
        float timestamp;

        // Variable: callback
        Action<TrackingStateResponse> callback;

        public TrackingStateCallback(float _timestamp, Action<TrackingStateResponse> _callback)
        {
            this.timestamp = _timestamp;
            this.callback = _callback;
        }
    }
}
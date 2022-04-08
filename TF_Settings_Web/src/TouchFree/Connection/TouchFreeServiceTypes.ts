import {
    InteractionConfigFull,
    InteractionConfig,
    PhysicalConfig
} from '../Configuration/ConfigurationTypes';
import {
    ConfigurationState,
    InputOverride,
    TrackingServiceState
} from '../TouchFreeToolingTypes';

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
export enum ActionCode {
    INPUT_ACTION = "INPUT_ACTION",

    CONFIGURATION_STATE = "CONFIGURATION_STATE",
    CONFIGURATION_RESPONSE = "CONFIGURATION_RESPONSE",
    SET_CONFIGURATION_STATE = "SET_CONFIGURATION_STATE",
    REQUEST_CONFIGURATION_STATE = "REQUEST_CONFIGURATION_STATE",

    VERSION_HANDSHAKE = "VERSION_HANDSHAKE",
    VERSION_HANDSHAKE_RESPONSE = "VERSION_HANDSHAKE_RESPONSE",

    HAND_PRESENCE_EVENT = "HAND_PRESENCE_EVENT",

    REQUEST_SERVICE_STATUS = "REQUEST_SERVICE_STATUS",
    SERVICE_STATUS_RESPONSE = "SERVICE_STATUS_RESPONSE",
    SERVICE_STATUS = "SERVICE_STATUS",

    REQUEST_CONFIGURATION_FILE = "REQUEST_CONFIGURATION_FILE",
    CONFIGURATION_FILE_STATE = "CONFIGURATION_FILE_STATE",
    SET_CONFIGURATION_FILE = "SET_CONFIGURATION_FILE",
    CONFIGURATION_FILE_RESPONSE = "CONFIGURATION_FILE_RESPONSE",

    INPUT_OVERRIDE = "INPUT_OVERRIDE"
}

// Enum: HandPresenceState
// HAND_FOUND - Sent when the first hand is found when no hand has been present for a moment
// HANDS_LOST - Sent when the last observed hand is lost, meaning no more hands are observed
// PROCESSED - Used locally to indicate that no change in state is awaiting processing. See its
//             use in <MessageReciever> for more details.
export enum HandPresenceState {
    HAND_FOUND,
    HANDS_LOST,
    PROCESSED,
}

// Enum: Compatibility
// COMPATIBLE - The API versions are considered compatible
// SERVICE_OUTDATED - The API versions are considered incompatible as Service is older than Tooling
// TOOLING_OUTDATED - The API versions are considered incompatible as Tooling is older than Service
export enum Compatibility {
    COMPATIBLE,
    SERVICE_OUTDATED,
    TOOLING_OUTDATED
}

export class HandPresenceEvent {
    state: HandPresenceState;

    constructor(_state: HandPresenceState) {
        this.state = _state;
    }
}

// Class: PartialConfigState
// This data structure is used to send requests for changes to configuration or to configuration files.
//
// When sending a configuration to the Service the structure can be comprised of either partial or complete objects.
export class PartialConfigState {
    // Variable: requestID
    requestID: string;
    // Variable: interaction
    interaction: Partial<InteractionConfig> | null;
    // Variable: physical
    physical: Partial<PhysicalConfig> | null;

    constructor(_id: string, _interaction: Partial<InteractionConfig> | null, _physical: Partial<PhysicalConfig> | null) {
        this.requestID = _id;
        this.interaction = _interaction;
        this.physical = _physical;
    }
}

// Class: ConfigState
// This data structure is used when receiving configuration data representing the state of the service
// or its config files.
//
// When receiving a configuration from the Service this structure contains ALL configuration data
export class ConfigState {
    // Variable: requestID
    requestID: string;
    // Variable: interaction
    interaction: InteractionConfigFull;
    // Variable: physical
    physical: PhysicalConfig;

    constructor(_id: string, _interaction: InteractionConfigFull, _physical: PhysicalConfig) {
        this.requestID = _id;
        this.interaction = _interaction;
        this.physical = _physical;
    }
}

export class InputOverrideRequest {
    requestID: string;
    inputOverride: InputOverride;

    constructor(_id: string,
                _input: InputOverride) {
        this.requestID = _id;
        this.inputOverride = _input;
    }
}

// class: ConfigChangeRequest
// Used to request the current state of the configuration on the Service. This is received as
// a <ConfigState> which should be linked to a <ConfigStateCallback> via requestID to make
// use of the data received.
export class ConfigChangeRequest {
    // Variable: requestID
    requestID: string;

    constructor(_id: string) {
        this.requestID = _id;
    }
}

// Class: ConfigStateCallback
// Used by <MessageReceiver> to wait for a <ConfigState> from the Service. Owns a callback
// with a <ConfigState> as a parameter to allow users to make use of the new
// <ConfigStateResponse>. Stores a timestamp of its creation so the response has the ability to
// timeout if not seen within a reasonable timeframe.
export class ConfigStateCallback {
    // Variable: timestamp
    timestamp: number;
    // Variable: callback
    callback: (detail: ConfigState) => void;

    constructor(_timestamp: number, _callback: (detail: ConfigState) => void) {
        this.timestamp = _timestamp;
        this.callback = _callback;
    }
}

// Class: ServiceStatus
// This data structure is used to receive service status.
//
// When receiving a configuration from the Service this structure contains ALL status data
export class ServiceStatus {
    // Variable: requestID
    requestID: string;
    // Variable: trackingServiceState
    trackingServiceState: TrackingServiceState | null;
    // Variable: configurationState
    configurationState: ConfigurationState | null;

    constructor(_id: string, _trackingServiceState: TrackingServiceState | null, _configurationState: ConfigurationState | null) {
        this.requestID = _id;
        this.trackingServiceState = _trackingServiceState;
        this.configurationState = _configurationState;
    }
}

// class: ServiceStatusRequest
// Used to request the current state of the status of the Service. This is received as
// a <ServiceStatus> which should be linked to a <ServiceStatusCallback> via requestID to make
// use of the data received.
export class ServiceStatusRequest {
    // Variable: requestID
    requestID: string;

    constructor(_id: string) {
        this.requestID = _id;
    }
}

// Class: ServiceStatusCallback
// Used by <MessageReceiver> to wait for a <ServiceStatus> from the Service. Owns a callback
// with a <ServiceStatus> as a parameter to allow users to make use of the new
// <ServiceStatusResponse>. Stores a timestamp of its creation so the response has the ability to
// timeout if not seen within a reasonable timeframe.
export class ServiceStatusCallback {
    // Variable: timestamp
    timestamp: number;
    // Variable: callback
    callback: (detail: ServiceStatus) => void;

    constructor(_timestamp: number, _callback: (detail: ServiceStatus) => void) {
        this.timestamp = _timestamp;
        this.callback = _callback;
    }
}

// Class: WebSocketResponse
// The structure seen when the Service responds to a request. This is to verify whether it was
// successful or not and will include the original request if it fails, to allow for
// troubleshooting.
export class WebSocketResponse {
    // Variable: requestID
    requestID: string;
    // Variable: status
    status: string;
    // Variable: message
    message: string;
    // Variable: originalRequest
    originalRequest: string;

    constructor(_id: string, _status: string, _msg: string, _request: string) {
        this.requestID = _id;
        this.status = _status;
        this.message = _msg;
        this.originalRequest = _request;
    }
}

// Class: ResponseCallback
// Used by <MessageReceiver> to wait for a <WebSocketResponse> from the Service. Owns a callback
// with a <WebSocketResponse> as a parameter to allow users to deal with failed
// <WebSocketResponses>. Stores a timestamp of its creation so the response has the ability to
// timeout if not seen within a reasonable timeframe.
export class ResponseCallback {
    // Variable: timestamp
    timestamp: number;
    // Variable: callback
    callback: (detail: WebSocketResponse) => void;

    constructor(_timestamp: number, _callback: (detail: WebSocketResponse) => void) {
        this.timestamp = _timestamp;
        this.callback = _callback;
    }
}

// Class: CommunicationWrapper
// A container structure used by <ServiceConnection> to interpret incoming data to its appropriate
// subtypes based on the <action> and pass the <content> on to the appropriate handler.
export class CommunicationWrapper<T> {
    // Variable: action
    action: ActionCode;
    // Variable: content
    content: T;

    constructor(_actionCode: ActionCode, _content: T) {
        this.action = _actionCode;
        this.content = _content;
    }
}
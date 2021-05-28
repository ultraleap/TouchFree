import {
    InteractionConfig,
    PhysicalConfig
} from '../Configuration/ConfigurationTypes';

// Enum: ActionCode
// INPUT_ACTION - Represents standard interaction data
// CONFIGURATION_STATE - Represents a collection of configurations from the Service
// CONFIGURATION_RESPONSE - Represents a Success/Failure response from a SET_CONFIGURATION_STATE
// SET_CONFIGURATION_STATE - Represents a request to set new configuration files on the Service
// REQUEST_CONFIGURATION_STATE - Represents a request to receive a current CONFIGURATION_STATE from the Service
// VERSION_HANDSHAKE - Represents an outgoing message from Client to Service, attempting to compare API versions for compatibility
// REQUEST_CONFIGURATION_STATE - Represents the result coming in from the Service
export enum ActionCode {
    INPUT_ACTION = "INPUT_ACTION",
    CONFIGURATION_STATE = "CONFIGURATION_STATE",
    CONFIGURATION_RESPONSE = "CONFIGURATION_RESPONSE",
    SET_CONFIGURATION_STATE = "SET_CONFIGURATION_STATE",
    REQUEST_CONFIGURATION_STATE = "REQUEST_CONFIGURATION_STATE",
    VERSION_HANDSHAKE = "VERSION_HANDSHAKE",
    VERSION_HANDSHAKE_RESPONSE = "VERSION_HANDSHAKE_RESPONSE",
    HAND_PRESENCE_EVENT = "HAND_PRESENCE_EVENT",
}

// Enum: HandPresenceState
// HAND_FOUND - Sent when the first hand is found when no hand has been present for a moment
// HANDS_LOST - Sent when the last observed hand is lost, meaning no more hands are observed
// PROCESSED - Used locally to indicate that no change in state is awaiting processing. See its
//             use in <MessageReciever> for more details.
export enum HandPresenceState
{
    HAND_FOUND,
    HANDS_LOST,
    PROCESSED,
}

// Enum: Compatibility
// COMPATIBLE - The API versions are considered compatible
// SERVICE_OUTDATED - The API versions are considered incompatible as Service is older than Client
// CLIENT_OUTDATED - The API versions are considered incompatible as Client is older than Service
export enum Compatibility {
    COMPATIBLE,
    SERVICE_OUTDATED,
    CLIENT_OUTDATED
}

export class HandPresenceEvent
{
    state: HandPresenceState;

    constructor(_state: HandPresenceState)
    {
        this.state = _state;
    }
}

// Class: ConfigState
// This data structure is used in both sending and receiving configuration data.
//
// When sending a configuration to the Service the structure can comprise of either partial or complete objects.
//
// When receiving a configuration from the Service this structure contains ALL configuration data
export class ConfigState {
    // Variable: requestID
    requestID: string;
    // Variable: interaction
    interaction: Partial<InteractionConfig> | null;
    // Variable: physical
    physical: Partial<PhysicalConfig> | null;

    constructor(_id: string, _interaction: Partial<InteractionConfig> | null, _physical: Partial<PhysicalConfig> | null){
        this.requestID = _id;
        this.interaction = _interaction;
        this.physical = _physical;
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
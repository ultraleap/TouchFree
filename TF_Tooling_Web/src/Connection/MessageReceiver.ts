import {
    ConfigState,
    ConfigStateCallback,
    HandPresenceState,
    ResponseCallback,
    ServiceStatus,
    ServiceStatusCallback,
    TrackingState,
    TrackingStateCallback,
    TrackingStateResponse,
    WebSocketResponse
} from './TouchFreeServiceTypes';
import {
    BitmaskFlags,
    ConvertInputAction,
    InputType,
    TouchFreeInputAction,
    WebsocketInputAction,
} from '../TouchFreeToolingTypes';
import { InputActionManager } from '../Plugins/InputActionManager';
import { ConnectionManager } from './ConnectionManager';

// Class: MessageReceiver
// Handles the receiving of messages from the Service in an ordered manner.
// Distributes the results of the messages to the respective managers.
export class MessageReceiver {
    // Group: Variables

    // Variable: callbackClearTimer
    // The amount of time between checks of <responseCallbacks> to eliminate expired
    // <ResponseCallbacks>. Used in <ClearUnresponsiveCallbacks>.
    callbackClearTimer: number = 300;

    // Variable: update Rate
    // How many times per second to process <WebSocketResponse> & <TouchFreeInputActions>
    updateRate: number = 60;

    // Calculated on construction for use in setting the update interval
    private updateDuration: number;

    // Variable: actionCullToCount
    // How many non-essential <TouchFreeInputActions> should the <actionQueue> be trimmed *to* per
    // frame. This is used to ensure the Tooling can keep up with the Events sent over the
    // WebSocket.
    actionCullToCount: number = 2;

    // Variable: actionQueue
    // A queue of <TouchFreeInputActions> that have been received from the Service.
    actionQueue: Array<WebsocketInputAction> = [];

    // Variable: responseQueue
    // A queue of <WebSocketResponses> that have been received from the Service.
    responseQueue: Array<WebSocketResponse> = [];

    // Variable: responseCallbacks
    // A dictionary of unique request IDs and <ResponseCallbacks> that represent requests that are awaiting response from the Service.
    responseCallbacks: { [id: string]: ResponseCallback; } = {};

    // Variable: configStateQueue
    // A queue of <ConfigState> that have been received from the Service.
    configStateQueue: Array<ConfigState> = [];

    // Variable: configStateCallbacks
    // A dictionary of unique request IDs and <ConfigStateCallback> that represent requests that are awaiting response from the Service.
    configStateCallbacks: { [id: string]: ConfigStateCallback; } = {};

    // Variable: serviceStatusQueue
    // A queue of <ServiceStatus> that have been received from the Service.
    serviceStatusQueue: Array<ServiceStatus> = [];

    // Variable: serviceStatusCallbacks
    // A dictionary of unique request IDs and <ServiceStatusCallback> that represent requests that are awaiting response from the Service.
    serviceStatusCallbacks: { [id: string]: ServiceStatusCallback; } = {};

    lastStateUpdate: HandPresenceState;

    // Variable: trackingStateQueue
    // A queue of <TrackingStates> that have been received from the Service.
    trackingStateQueue: Array<TrackingStateResponse> = [];

    // Variable: trackingSettingsStateCallbacks
    // A dictionary of unique request IDs and <TrackingStateCallback> that represent requests that are awaiting response from the Service.
    trackingStateCallbacks: { [id: string]: TrackingStateCallback; } = {};

    // Variable: callbackClearInterval
    // Stores the reference number for the interval running <ClearUnresponsiveCallbacks>, allowing
    // it to be cleared.
    private callbackClearInterval: number;

    // Variable: updateInterval
    // Stores the reference number for the interval running <Update>, allowing it to be cleared.
    private updateInterval: number;

    // Used to ensure UP events are sent at the correct position relative to the previous
    // MOVE event.
    // This is required due to the culling of events from the actionQueue in CheckForAction.
    lastKnownCursorPosition: Array<number> = [0, 0];

    // Group: Functions

    // Function: constructor
    // Starts the two regular intervals managed for this (running <ClearUnresponsiveCallbacks> on an
    // interval of <callbackClearTimer> and <Update> on an interval of updateDuration
    constructor() {
        this.lastStateUpdate = HandPresenceState.PROCESSED;
        this.updateDuration = (1 / this.updateRate) * 1000;

        this.callbackClearInterval = setInterval(
            this.ClearUnresponsivePromises as TimerHandler,
            this.callbackClearTimer);

        this.updateInterval = setInterval(
            this.Update.bind(this) as TimerHandler,
            this.updateDuration);
    }

    // Function: Update
    // Update function. Checks all queues for messages to handle. Run on an interval
    // started during the constructor
    Update(): void {
        this.CheckForResponse();
        this.CheckForConfigState();
        this.CheckForServiceStatus();
        this.CheckForTrackingStateResponse();
        this.CheckForAction();
    }

    // Function: CheckForResponse
    // Used to check the <responseQueue> for a <WebSocketResponse>. Sends it to <HandleResponse> if
    // there is one.
    CheckForResponse(): void {
        let response: WebSocketResponse | undefined = this.responseQueue.shift();

        if (response !== undefined) {
            this.HandleResponse(response);
        }
    }

    // Function: HandleResponse
    // Checks the dictionary of <responseCallbacks> for a matching request ID. If there is a
    // match, calls the callback action in the matching <ResponseCallback>.
    HandleResponse(_response: WebSocketResponse): void {
        if (this.responseCallbacks !== undefined) {
            for (let key in this.responseCallbacks) {
                if (key === _response.requestID) {
                    // This is logged to aid users in debugging
                    console.log(_response.message);
                    this.responseCallbacks[key].callback(_response);
                    delete this.responseCallbacks[key];
                    return;
                }
            };
        }

        console.log("Received a WebSocketResponse that did not match a callback." +
            "This is the content of the response: \n Response ID: " + _response.requestID +
            "\n Status: " + _response.status + "\n Message: " + _response.message +
            "\n Original request - " + _response.originalRequest);
    }

    // Function: CheckForConfigState
    // Used to check the <configStateQueue> for a <ConfigState>. Sends it to <HandleConfigState> if there is one.
    CheckForConfigState(): void {
        let configState: ConfigState | undefined = this.configStateQueue.shift();

        if (configState !== undefined) {
            this.HandleConfigState(configState);
        }
    }

    // Function: HandleConfigState
    // Checks the dictionary of <configStateCallbacks> for a matching request ID. If there is a
    // match, calls the callback action in the matching <ConfigStateCallback>.
    HandleConfigState(_configState: ConfigState): void {
        if (this.configStateCallbacks !== undefined) {
            for (let key in this.configStateCallbacks) {
                if (key === _configState.requestID) {
                    this.configStateCallbacks[key].callback(_configState);
                    delete this.configStateCallbacks[key];
                    return;
                }
            };
        }
    }

    // Function: CheckForServiceStatus
    // Used to check the <serviceStatusQueue> for a <ServiceStatus>. Sends it to <HandleServiceStatus> if there is one.
    CheckForServiceStatus(): void {
        let serviceStatus: ServiceStatus | undefined = this.serviceStatusQueue.shift();

        if (serviceStatus !== undefined) {
            this.HandleServiceStatus(serviceStatus);
        }
    }

    // Function: HandleServiceStatus
    // Checks the dictionary of <serviceStatusCallbacks> for a matching request ID. If there is a
    // match, calls the callback action in the matching <ServiceStatusCallback>.
    HandleServiceStatus(_serviceStatus: ServiceStatus): void {
        if (this.serviceStatusCallbacks !== undefined) {
            for (let key in this.serviceStatusCallbacks) {
                if (key === _serviceStatus.requestID) {
                    this.serviceStatusCallbacks[key].callback(_serviceStatus);
                    delete this.serviceStatusCallbacks[key];
                    return;
                }
            };
        }
    }

    // Function: CheckForTrackingStateResponse
    // Used to check the <trackingStateQueue> for a <TrackingStateResponse>. Sends it to <HandleTrackingStateResponse> if there is one.
    CheckForTrackingStateResponse(): void {
        const trackingStateResponse: TrackingStateResponse | undefined = this.trackingStateQueue.shift();

        if (trackingStateResponse !== undefined) {
            this.HandleTrackingStateResponse(trackingStateResponse);
        }
    }

    // Function: HandleTrackingStateResponse
    // Checks the dictionary of <trackingStateCallbacks> for a matching request ID. If there is a
    // match, calls the callback action in the matching <TrackingStateCallback>.
    HandleTrackingStateResponse(trackingStateResponse: TrackingStateResponse): void {
        if (this.trackingStateCallbacks !== undefined) {
            for (let key in this.trackingStateCallbacks) {
                if (key === trackingStateResponse.requestID) {
                    this.trackingStateCallbacks[key].callback(trackingStateResponse);
                    delete this.trackingStateCallbacks[key];
                    return;
                }
            };
        }
    }

    // Function: CheckForAction
    // Checks <actionQueue> for valid <TouchFreeInputActions>. If there are too many in the queue,
    // clears out non-essential <TouchFreeInputActions> down to the number specified by
    // <actionCullToCount>. If any remain, sends the oldest <TouchFreeInputAction> to
    // <InputActionManager> to handle the action.
    // UP <InputType>s have their positions set to the last known position to ensure
    // input events trigger correctly.
    CheckForAction(): void {
        while (this.actionQueue.length > this.actionCullToCount) {
            if (this.actionQueue[0] !== undefined) {
                // Stop shrinking the queue if we have a 'key' input event
                if (this.actionQueue[0].InteractionFlags & BitmaskFlags.MOVE ||
                    this.actionQueue[0].InteractionFlags & BitmaskFlags.NONE) {
                    // We want to ignore non-move results
                    this.actionQueue.shift();
                } else {
                    break;
                }
            }
        }

        let action: WebsocketInputAction | undefined = this.actionQueue.shift();

        if (action !== undefined) {

            // Parse newly received messages & distribute them
            let converted: TouchFreeInputAction = ConvertInputAction(action);

            //Cache or use the lastKnownCursorPosition. Copy the array to ensure it is not a reference
            if (converted.InputType !== InputType.UP) {
                this.lastKnownCursorPosition = Array.from(converted.CursorPosition);
            }
            else {
                converted.CursorPosition = Array.from(this.lastKnownCursorPosition);
            }

            // Wrapping the function in a timeout of 0 seconds allows the dispatch to be asynchronous
            setTimeout(() => {
                InputActionManager.HandleInputAction(converted);
            });
        }

        if (this.lastStateUpdate !== HandPresenceState.PROCESSED) {
            ConnectionManager.HandleHandPresenceEvent(this.lastStateUpdate);
            this.lastStateUpdate = HandPresenceState.PROCESSED;
        }
    }

    // Function: ClearUnresponsiveCallbacks
    // Waits for <callbackClearTimer> seconds and clears all <ResponseCallbacks> that are
    // expired from <responseCallbacks>.
    ClearUnresponsivePromises(): void {
        let lastClearTime: number = Date.now();

        if (this.responseCallbacks !== undefined) {
            for (let key in this.responseCallbacks) {
                if (this.responseCallbacks[key].timestamp < lastClearTime) {
                    delete this.responseCallbacks[key];
                } else {
                    break;
                }
            };
        }

        if (this.configStateCallbacks !== undefined) {
            for (let key in this.configStateCallbacks) {
                if (this.configStateCallbacks[key].timestamp < lastClearTime) {
                    delete this.configStateCallbacks[key];
                } else {
                    break;
                }
            };
        }
    }
}
import { HandDataManager } from '../Plugins/HandDataManager';
import { InputActionManager } from '../Plugins/InputActionManager';
import TouchFree from '../TouchFree';
import {
    BitmaskFlags,
    ConvertInputAction,
    InputType,
    TouchFreeInputAction,
    WebsocketInputAction,
} from '../TouchFreeToolingTypes';
import { ConnectionManager } from './ConnectionManager';
import {
    ConfigState,
    ConfigStateCallback,
    HandPresenceState,
    TouchFreeRequestCallback,
    ResponseCallback,
    ServiceStatus,
    ServiceStatusCallback,
    TouchFreeRequest,
    TrackingStateCallback,
    TrackingStateResponse,
    WebSocketResponse,
} from './TouchFreeServiceTypes';

/**
 * Receives messages from the service and distributes them
 * to respective managers for handling.
 * 
 * @internal
 */
export class MessageReceiver {

    /**
     * The amount of time between response callback handling to eliminate unhandled callbacks.
     * Prevents a performance death spiral scenario.
     */
    callbackClearTimer = 300;

    /**
     * How many times per second to process {@link WebSocketResponse} and {@link TouchFreeInputAction} requests.
     */
    updateRate = 60;

    /**
     * Duration (in seconds) of update interval - inverse of {@link updateRate}
     */
    private updateDuration: number;

    /**
     * How many non-essential {@link TouchFreeInputAction}s should the {@link actionQueue}
     * be trimmed *to* per frame. This is used to ensure the Tooling can keep up with the
     * Events sent over the WebSocket.
     */
    actionCullToCount = 2;

    /**
     * A queue of {@link TouchFreeInputAction}s that have been received from the Service.
     */
    actionQueue: Array<WebsocketInputAction> = [];

    /**
     * The latest `HandFrame` that has been received from the Service.
     */
    latestHandDataItem?: ArrayBuffer = undefined;

    /**
     * A queue of {@link WebSocketResponse}s that have been received from the Service.
     */
    responseQueue: Array<WebSocketResponse> = [];

    /**
     * A dictionary of unique request IDs and {@link ResponseCallback}s that represent requests
     * awaiting a response from the Service.
     */
    responseCallbacks: { [id: string]: ResponseCallback } = {};

    /**
     * A queue of {@link ConfigState} that have been received from the Service.
     */
    configStateQueue: Array<ConfigState> = [];

    /**
     * A dictionary of unique request IDs and {@link ConfigStateCallback} that represent requests
     * awaiting a response from the Service.
     */
    configStateCallbacks: { [id: string]: ConfigStateCallback } = {};

    /**
     * A queue of {@link ServiceStatus} that have been received from the Service.
     */
    serviceStatusQueue: Array<ServiceStatus> = [];

    /**
     * A dictionary of unique request IDs and {@link ServiceStatusCallback} that represent requests
     * awaiting a response from the Service.
     */
    serviceStatusCallbacks: { [id: string]: ServiceStatusCallback } = {};

    /**
     * The last hand presence state update received from the Service.
     */
    lastStateUpdate: HandPresenceState;

    /**
     * A queue of `TrackingStates` that have been received from the Service.
     */
    trackingStateQueue: Array<TrackingStateResponse> = [];

    /**
     * A dictionary of unique request IDs and {@link TrackingStateCallback} that represent requests
     * that are awaiting response from the Service.
     */
    trackingStateCallbacks: { [id: string]: TrackingStateCallback } = {};

    /**
     * Stores the reference number for the interval running {@link ClearUnresponsivePromises}, allowing
     * it to be cleared.
     */
    private callbackClearInterval: number;

    /**
     * Stores the reference number for the interval running {@link Update}, allowing it to be cleared.
     */
    private updateInterval: number;

    /**
     * Used to ensure UP events are sent at the correct position relative to the previous MOVE event.
     * This is required due to the culling of events from the {@link actionQueue} in {@link CheckForAction}.
     */
    lastKnownCursorPosition: Array<number> = [0, 0];

    /**
     * Starts the two regular intervals - {@link ClearUnresponsivePromises} (at {@link callbackClearTimer})
     * and {@link Update} (at {@link updateRate})
     */
    constructor() {
        this.lastStateUpdate = HandPresenceState.PROCESSED;
        this.updateDuration = (1 / this.updateRate) * 1000;

        this.callbackClearInterval = setInterval(
            this.ClearUnresponsivePromises as TimerHandler,
            this.callbackClearTimer
        );

        this.updateInterval = setInterval(this.Update.bind(this) as TimerHandler, this.updateDuration);
    }

    /**
     * Checks all queues for messages to handle.
     */
    Update(): void {
        this.CheckForResponse();
        this.CheckForConfigState();
        this.CheckForServiceStatus();
        this.CheckForTrackingStateResponse();
        this.CheckForAction();
        this.CheckForHandData();
    }

    /**
     * Checks {@link responseQueue} for a single {@link WebSocketResponse} and handles it.
     */
    CheckForResponse(): void {
        const response: WebSocketResponse | undefined = this.responseQueue.shift();

        if (response !== undefined) {
            const responseResult = MessageReceiver.HandleCallbackList(response, this.responseCallbacks);
            
            switch(responseResult)
            {
                case 'NoCallbacksFound':
                    console.warn(
                        'Received a WebSocketResponse that did not match a callback.' +
                            'This is the content of the response: \n Response ID: ' +
                            response.requestID +
                            '\n Status: ' +
                            response.status +
                            '\n Message: ' +
                            response.message +
                            '\n Original request - ' +
                            response.originalRequest
                    );
                    break;
                case 'Success':
                    if(response.message) {
                        // This is logged to aid users in debugging
                        console.log('Successfully received WebSocketResponse from TouchFree:\n' + response.message);
                    }
                    break;
            }
        }
    }

    /**
     * Checks {@link configStateQueue} for a single {@link configState} and handles it.
     */
    CheckForConfigState(): void {
        const configState: ConfigState | undefined = this.configStateQueue.shift();

        if (configState !== undefined) {
            const configResult = MessageReceiver.HandleCallbackList(configState, this.configStateCallbacks);
            switch(configResult)
            {
                case 'NoCallbacksFound':
                    console.warn('Received a ConfigState message that did not match a callback.');
                    break;
                case 'Success':
                    // no-op
                    break;
            }
        }
    }

    /**
     * Checks a callback dictionary for a request id and handles invoking the callback.
     * 
     * @param callbackResult Callback data
     * @param callbacks Callback dictionary to check
     * @returns String literal result representing success or what went wrong
     */
    private static HandleCallbackList<T extends TouchFreeRequest>(
        callbackResult: T,
        callbacks: { [id: string]: TouchFreeRequestCallback<T> }
    ): 'Success' | 'NoCallbacksFound' {

        for (const key in callbacks) {
            if (key === callbackResult.requestID) {
                callbacks[key].callback(callbackResult);
                delete callbacks[key];
                return 'Success';
            }
        }

        return 'NoCallbacksFound';
    }

    /**
     * Checks {@link serviceStatusQueue} for a single {@link ServiceStatus} and handles it.
     */
    CheckForServiceStatus(): void {
        const serviceStatus: ServiceStatus | undefined = this.serviceStatusQueue.shift();

        if (serviceStatus !== undefined) {
            const callbackResult = MessageReceiver.HandleCallbackList(serviceStatus, this.serviceStatusCallbacks);

            switch(callbackResult)
            {
                // If callback didn't happen for known reasons, we can be sure it's an independent status event rather
                // than a request response
                // TODO: Send/handle this request from service differently from normal response so we can be sure it's an independent event
                case 'NoCallbacksFound':
                    // If service state is null we didn't get info about it from this message
                    if (serviceStatus.trackingServiceState !== null) {
                        TouchFree.DispatchEvent('OnTrackingServiceStateChange', serviceStatus.trackingServiceState);
                    }
                    break;
                case 'Success':
                    // no-op
                    break;
            }
        }
    }

    /**
     * Checks {@link trackingStateQueue} for a single {@link TrackingStateResponse} and handles it.
     */
    CheckForTrackingStateResponse(): void {
        const trackingStateResponse: TrackingStateResponse | undefined = this.trackingStateQueue.shift();

        if (trackingStateResponse !== undefined) {
            this.HandleTrackingStateResponse(trackingStateResponse);
        }
    }

    /**
     * Checks {@link trackingStateCallbacks} for a request id and handles invoking the callback.
     */
    HandleTrackingStateResponse(trackingStateResponse: TrackingStateResponse): void {
        if (this.trackingStateCallbacks !== undefined) {
            // TODO: use `HandleCallbackList`
            for (const key in this.trackingStateCallbacks) {
                if (key === trackingStateResponse.requestID) {
                    this.trackingStateCallbacks[key].callback(trackingStateResponse);
                    delete this.trackingStateCallbacks[key];
                    return;
                }
            }
        }
    }

    /**
     * Checks {@link actionQueue} for a single {@link TouchFreeInputAction} and handles it.
     * 
     * @remarks
     * If there are too many in the queue, clears out non-essential {@link TouchFreeInputAction}
     * down to the number specified by {@link actionCullToCount}.
     * If any remain, sends the oldest {@link TouchFreeInputAction} to {@link InputActionManager}
     * to handle the action. Actions with UP {@link InputType} have their positions set to
     * {@link lastKnownCursorPosition} to ensure input events trigger correctly.
     */
    CheckForAction(): void {
        while (this.actionQueue.length > this.actionCullToCount) {
            if (this.actionQueue[0] !== undefined) {
                // Stop shrinking the queue if we have a 'key' input event
                if (
                    this.actionQueue[0].InteractionFlags & BitmaskFlags.MOVE ||
                    this.actionQueue[0].InteractionFlags & BitmaskFlags.NONE_INPUT
                ) {
                    // We want to ignore non-move results
                    this.actionQueue.shift();
                } else {
                    break;
                }
            }
        }

        const action: WebsocketInputAction | undefined = this.actionQueue.shift();

        if (action !== undefined) {
            // Parse newly received messages & distribute them
            const converted: TouchFreeInputAction = ConvertInputAction(action);

            // Cache or use the lastKnownCursorPosition. Copy the array to ensure it is not a reference
            if (converted.InputType !== InputType.UP) {
                this.lastKnownCursorPosition = Array.from(converted.CursorPosition);
            } else {
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

    /**
     * Checks {@link latestHandDataItem} and if the `HandFrame` is not undefined sends it to
     * {@link HandDataManager} to handle the frame.
     */
    CheckForHandData(): void {
        const handFrame = this.latestHandDataItem;

        if (handFrame) {
            this.latestHandDataItem = undefined;
            // Wrapping the function in a timeout of 0 seconds allows the dispatch to be asynchronous
            setTimeout(() => {
                HandDataManager.HandleHandFrame(handFrame);
            });
        }
    }

    /**
     * Clear {@link responseCallbacks} that have been around for more than {@link callbackClearTimer}.
     */
    ClearUnresponsivePromises(): void {
        const lastClearTime: number = Date.now();

        MessageReceiver.ClearUnresponsiveItems(lastClearTime, this.responseCallbacks);
        MessageReceiver.ClearUnresponsiveItems(lastClearTime, this.configStateCallbacks);
        MessageReceiver.ClearUnresponsiveItems(lastClearTime, this.serviceStatusCallbacks);
    }

    private static ClearUnresponsiveItems<T>(
        lastClearTime: number,
        callbacks: { [id: string]: TouchFreeRequestCallback<T> }
    ) {
        if (callbacks !== undefined) {
            for (const key in callbacks) {
                if (callbacks[key].timestamp < lastClearTime) {
                    delete callbacks[key];
                } else {
                    break;
                }
            }
        }
    }
}

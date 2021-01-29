import {
    WebSocketResponse,
    ResponseCallback
} from './ScreenControlServiceTypes';
import {
    ClientInputAction, ConvertInputAction, InputType, WebsocketInputAction
} from '../ScreenControlTypes';
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

    // Variable: actionCullToCount
    // How many non-essential <ClientInputActions> should the <actionQueue> be trimmed *to* per
    // frame. This is used to ensure the Client can keep up with the Events sent over the
    // WebSocket.
    actionCullToCount: number = 2;

    // Variable: actionQueue
    // A queue of <ClientInputActions> that have been received from the Service.
    actionQueue: Array<WebsocketInputAction> = [];

    // Variable: responseQueue
    // A queue of <WebSocketResponses> that have been received from the Service.
    responseQueue: Array<WebSocketResponse> = [];

    // Variable: responseCallbacks
    // A dictionary of unique request IDs and <ResponseCallbacks> that represent requests that are awaiting response from the Service.
    responseCallbacks: Record<string, ResponseCallback> = {};

    callbackClearInterval: number;

    updateInterval: number;

    // Group: Functions
    // Function: Start
    // Unity's initialization function. Used to begin the <ClearUnresponsiveCallbacks> coroutine.
    constructor() {
        this.callbackClearInterval = setInterval(
            this.ClearUnresponsivePromises as TimerHandler,
            this.callbackClearTimer * 1000);

        this.updateInterval = setInterval(
            this.Update as TimerHandler,
            60 * 1000);
    }

    // Function: Update
    // Update function. Checks all queues for messages to handle. Run on an interval
    // started during the constructor
    Update(): void {
        this.CheckForResponse();
        this.CheckForAction();
    }

    // Function: CheckForResponse
    // Used to check the <responseQueue> for a <WebSocketResponse>. Sends it to <HandleResponse> if
    // there is one.
    CheckForResponse(): void {
        var response = this.responseQueue.shift();

        if (response != null) {
            this.HandleResponse(response);
        }
    }

    // Function: HandleResponse
    // Checks the dictionary of <responseCallbacks> for a matching request ID. If there is a
    // match, calls the callback action in the matching <ResponseCallback>.
    HandleResponse(_response: WebSocketResponse): void {
        if (this.responseQueue.length > 0) {
            this.CheckForResponse();
        }
    }

    // Function: CheckForAction
    // Checks <actionQueue> for valid <ClientInputActions>. If there are too many in the queue,
    // clears out non-essential <ClientInputActions> down to the number specified by
    // <actionCullToCount>. If any remain, sends the oldest <ClientInputAction> to
    // <serviceConnection> to handle the action.
    CheckForAction(): void {
        var action: WebsocketInputAction | undefined;

        while (this.actionQueue.length > this.actionCullToCount) {
            action = this.actionQueue.shift();

            if (action != undefined) {
                // Stop shrinking the queue if we have a 'key' input event
                if (!(action.interactionFlags && InputType.MOVE)) {
                    // We don't want to ignore non-move results
                    this.actionQueue.unshift(action);
                    break;
                }
            }
        }

        action = this.actionQueue.shift();

        if (action != undefined) {
            // Parse newly received messages & distribute them
            var converted: ClientInputAction = ConvertInputAction(action);

            ConnectionManager.HandleInputAction(converted);
        }
    }

    // Function: ClearUnresponsiveCallbacks
    // Waits for <callbackClearTimer> seconds and clears all <ResponseCallbacks> that are
    // expired from <responseCallbacks>.
    ClearUnresponsivePromises(): void {
        while (true) {
            var lastClearTime: number = Date.now();

            var keys: Array<keyof typeof Object> = Object.keys(this.responseCallbacks) as Array<keyof typeof Object>;

            for (var i: number = 0; i < keys.length; i++) {
                var key = keys[i];

                if (this.responseCallbacks[key].timestamp < lastClearTime) {
                    delete this.responseCallbacks[key];
                }
                else {
                    break;
                }
            };
        }
    }
}
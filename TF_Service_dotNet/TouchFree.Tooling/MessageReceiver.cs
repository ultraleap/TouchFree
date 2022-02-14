using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Ultraleap.TouchFree.Library;

namespace Ultraleap.TouchFree.Tooling
{
    public class MessageReceiver
    {
        // Group: Variables

        // Variable: callbackClearTimer
        // The amount of time between checks of <responseCallbacks> to eliminate expired
        // <ResponseCallbacks>. Used in <ClearUnresponsiveCallbacks>.
        const int callbackClearTimer = 300; // 5 minutes

        // Variable: actionCullToCount
        // How many non-essential <ClientInputActions> should the <actionQueue> be trimmed *to* per
        // frame. This is used to ensure the Client can keep up with the Events sent over the
        // WebSocket.
        public int actionCullToCount = 2;

        // Variable: actionQueue
        // A queue of <ClientInputActions> that have been received from the Service.
        public ConcurrentQueue<ClientInputAction> actionQueue = new ConcurrentQueue<ClientInputAction>();

        // Variable: responseQueue
        // A queue of <ResponseToClients> that have been received from the Service.
        public ConcurrentQueue<ResponseToClient> responseQueue = new ConcurrentQueue<ResponseToClient>();

        // Variable: responseCallbacks
        // A dictionary of unique request IDs and <ResponseCallbacks> that represent requests that are awaiting response from the Service.
        public Dictionary<string, ResponseCallback> responseCallbacks = new Dictionary<string, ResponseCallback>();

        // Variable: configStateQueue
        // A queue of <ConfigState> that have been received from the Service.
        public ConcurrentQueue<ConfigState> configStateQueue = new ConcurrentQueue<ConfigState>();

        // Variable: configStateCallbacks
        // A dictionary of unique request IDs and <ConfigStateCallbacks> that represent requests that are awaiting response from the Service.
        public Dictionary<string, ConfigStateCallback> configStateCallbacks = new Dictionary<string, ConfigStateCallback>();

        // Used to store HandPresenceState changes as they are recieved and emit messages
        // appropriately. "PROCESSED" when there are no unprocessed changes.
        internal HandPresenceState handState = HandPresenceState.PROCESSED;

        // Used to ensure UP events are sent at the correct position relative to the previous
        // MOVE event.
        // This is required due to the culling of events from the actionQueue in CheckForAction.
        WebSocketVector2 lastKnownCursorPosition = new WebSocketVector2();

        // Group: Functions

        // Function: Start
        // Unity's initialization function. Used to begin the <ClearUnresponsiveCallbacks> coroutine.
        public MessageReceiver()
        {
            // TODO: call this either on message or on message wrapped in a rate limiter (this looks like it already has some rate limiting)
            ClearUnresponsiveCallbacks();
        }

        // Function: Update
        // Unity's update function. Checks all queues for messages to handle.
        public void Update()
        {
            // TODO: call this either on message or on message wrapped in a rate limiter
            CheckForResponse();
            CheckForConfigState();
            CheckForAction();
        }

        // Function: CheckForResponse
        // Used to check the <responseQueue> for a <ResponseToClient>. Sends it to <HandleResponse> if there is one.
        void CheckForResponse()
        {
            ResponseToClient response;

            if (responseQueue.TryPeek(out response))
            {
                // Parse newly received messages
                responseQueue.TryDequeue(out response);
                HandleResponse(response);
            }
        }

        // Function: HandleResponse
        // Checks the dictionary of <responseCallbacks> for a matching request ID. If there is a
        // match, calls the callback action in the matching <ResponseCallback>.
        void HandleResponse(ResponseToClient _response)
        {
            foreach (KeyValuePair<string, ResponseCallback> callback in responseCallbacks)
            {
                if (callback.Key == _response.requestID)
                {
                    callback.Value.callback.Invoke(_response);
                    responseCallbacks.Remove(callback.Key);
                    return;
                }
            }

            Debug.WriteLine("Received a ResponseToClient that did not match a callback." +
                "This is the content of the response: \n Response ID: " + _response.requestID +
                "\n Status: " + _response.status + "\n Message: " + _response.message +
                "\n Original request - " + _response.originalRequest);
        }

        // Function: CheckForConfigState
        // Used to check the <configStateQueue> for a <ConfigState>. Sends it to <HandleConfigState> if there is one.
        void CheckForConfigState()
        {
            ConfigState configState;

            if (configStateQueue.TryPeek(out configState))
            {
                // Parse newly received messages
                configStateQueue.TryDequeue(out configState);
                HandleConfigState(configState);
            }
        }

        /// <summary>
        /// Checks the dictionary of <cref>configStateCallbacks</cref> for a matching request ID. If there is a
        /// match, calls the callback action in the matching <cref>ConfigStateCallback</cref>.
        /// </summary>
        /// <param name="_configState">The configuration state received from the websocket</param>
        void HandleConfigState(ConfigState _configState)
        {
            foreach (KeyValuePair<string, ConfigStateCallback> callback in configStateCallbacks)
            {
                if (callback.Key == _configState.requestID)
                {
                    callback.Value.callback.Invoke(_configState);
                    responseCallbacks.Remove(callback.Key);
                    break;
                }
            }
        }

        /// <summary>
        /// Checks <cref>actionQueue</cref> for valid <cref>ClientInputAction</cref>s. If there are too many in the queue,
        /// clears out non-essential <cref>ClientInputActions</cref> down to the number specified by
        /// <cref>actionCullToCount</cref>. If any remain, sends the oldest <cref>ClientInputAction</cref> to
        /// <cref>ClientInputActionManager</cref> to distribute the action.
        /// UP <cref>InputType</cref>s have their positions set to the last known position to ensure
        /// input events trigger correctly.
        /// </summary>
        void CheckForAction()
        {
            ClientInputAction action = new ClientInputAction();

            while (actionQueue.Count > actionCullToCount)
            {
                if (actionQueue.TryPeek(out action))
                {
                    // Stop shrinking the queue if we have a 'key' input event
                    if (action.InputType != InputType.MOVE)
                    {
                        break;
                    }

                    // We want to shrink the queue, dequeue the element and ignore it
                    actionQueue.TryDequeue(out action);
                }
            }

            if (actionQueue.TryPeek(out action))
            {
                // Parse newly received messages
                actionQueue.TryDequeue(out action);

                // Cache or use the lastKnownCursorPosition
                if (action.InputType != InputType.UP)
                {
                    lastKnownCursorPosition = action.CursorPosition;
                }
                else
                {
                    action.CursorPosition = lastKnownCursorPosition;
                }

                InputActionManager.Instance.SendInputAction(action);
            }

            if (handState != HandPresenceState.PROCESSED)
            {
                ConnectionManager.HandleHandPresenceEvent(handState);
                handState = HandPresenceState.PROCESSED;
            }
        }

        // Group: Coroutine Functions

        // Function: ClearUnresponsiveCallbacks
        // Waits for <callbackClearTimer> seconds and clears all <ResponseCallbacks> that are
        // expired from <responseCallbacks>. Also clears all <ConfigStateCallback> that are
        // expired from <configStateCallbacks>.
        async Task ClearUnresponsiveCallbacks()
        {
            await Task.Delay(callbackClearTimer * 1000);

            while (true)
            {
                int lastClearTime = System.DateTime.Now.Millisecond;

                List<string> keys = new List<string>(responseCallbacks.Keys);

                foreach (string key in keys)
                {
                    if (responseCallbacks[key].timestamp < lastClearTime)
                    {
                        responseCallbacks.Remove(key);
                    }
                    else
                    {
                        break;
                    }
                }

                keys = new List<string>(configStateCallbacks.Keys);

                foreach (string key in keys)
                {
                    if (configStateCallbacks[key].timestamp < lastClearTime)
                    {
                        configStateCallbacks.Remove(key);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;

namespace Ultraleap.ScreenControl.Client.Connection
{
    // Class: MessageReceiver
    // Handles the receiving of messages from the Service in an ordered manner.
    // Distributes the results of the messages to the respective managers.
    [DisallowMultipleComponent]
    public class MessageReceiver : MonoBehaviour
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
        // A queue of <WebSocketResponses> that have been received from the Service.
        public ConcurrentQueue<WebSocketResponse> responseQueue = new ConcurrentQueue<WebSocketResponse>();

        // Variable: responseCallbacks
        // A dictionary of unique request IDs and <ResponseCallbacks> that represent requests that are awaiting response from the Service.
        public Dictionary<string, ResponseCallback> responseCallbacks = new Dictionary<string, ResponseCallback>();

        // Variable: configStateQueue
        // A queue of <ConfigStateResponse> that have been received from the Service.
        public ConcurrentQueue<ConfigStateResponse> configStateQueue = new ConcurrentQueue<ConfigStateResponse>();

        // Variable: configStateCallbacks
        // A dictionary of unique request IDs and <ConfigurationStateCallbacks> that represent requests that are awaiting response from the Service.
        public Dictionary<string, ConfigurationStateCallback> configStateCallbacks = new Dictionary<string, ConfigurationStateCallback>();

        // Group: Functions

        // Function: Start
        // Unity's initialization function. Used to begin the <ClearUnresponsiveCallbacks> coroutine.
        void Start()
        {
            StartCoroutine(ClearUnresponsiveCallbacks());
        }

        // Function: Update
        // Unity's update function. Checks all queues for messages to handle.
        void Update()
        {
            CheckForResponse();
            CheckForConfigState();
            CheckForAction();
        }

        // Function: CheckForResponse
        // Used to check the <responseQueue> for a <WebSocketResponse>. Sends it to <HandleResponse> if there is one.
        void CheckForResponse()
        {
            WebSocketResponse response;

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
        void HandleResponse(WebSocketResponse _response)
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

            Debug.LogWarning("Received a WebSocketResponse that did not match a callback." +
                "This is the content of the response: \n Response ID: " + _response.requestID +
                "\n Original request - " + _response.originalRequest +
                "\n Status: " + _response.status + "\n Message: " + _response.message);
        }

        // Function: CheckForConfigState
        // Used to check the <configStateQueue> for a <ConfigStateResponse>. Sends it to <HandleConfigState> if there is one.
        void CheckForConfigState()
        {
            ConfigStateResponse configState;

            if (configStateQueue.TryPeek(out configState))
            {
                // Parse newly received messages
                configStateQueue.TryDequeue(out configState);
                HandleConfigState(configState);
            }
        }

        // Function: HandleConfigState
        // Checks the dictionary of <configStateCallbacks> for a matching request ID. If there is a
        // match, calls the callback action in the matching <ConfigurationStateCallback>.
        void HandleConfigState(ConfigStateResponse _configState)
        {
            foreach (KeyValuePair<string, ConfigurationStateCallback> callback in configStateCallbacks)
            {
                if (callback.Key == _configState.requestID)
                {
                    callback.Value.callback.Invoke(_configState);
                    responseCallbacks.Remove(callback.Key);
                    break;
                }
            }
        }

        // Function: CheckForAction
        // Checks <actionQueue> for valid <ClientInputActions>. If there are too many in the queue,
        // clears out non-essential <ClientInputActions> down to the number specified by
        // <actionCullToCount>. If any remain, sends the oldest <ClientInputAction> to
        // <serviceConnection> to handle the action.
        void CheckForAction()
        {
            ClientInputAction action;
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
                ConnectionManager.HandleInputAction(action);
            }
        }

        // Group: Coroutine Functions

        // Function: ClearUnresponsiveCallbacks
        // Waits for <callbackClearTimer> seconds and clears all <ResponseCallbacks> that are
        // expired from <responseCallbacks>.
        IEnumerator ClearUnresponsiveCallbacks()
        {
            WaitForSeconds waitTime = new WaitForSeconds(callbackClearTimer);

            while (true)
            {
                int lastClearTime = System.DateTime.Now.Millisecond;

                yield return waitTime;

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
            }
        }
    }
}
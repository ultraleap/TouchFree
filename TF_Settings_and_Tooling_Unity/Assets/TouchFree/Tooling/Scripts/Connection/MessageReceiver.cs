using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;
using System;

namespace Ultraleap.TouchFree.Tooling.Connection
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
        // How many non-essential <InputActions> should the <actionQueue> be trimmed *to* per
        // frame. This is used to ensure the Client can keep up with the Events sent over the
        // WebSocket.
        public int actionCullToCount = 2;

        // Variable: actionQueue
        // A queue of <InputActions> that have been received from the Service.
        public ConcurrentQueue<InputAction> actionQueue = new ConcurrentQueue<InputAction>();

        // Variable: responseQueue
        // A queue of <WebSocketResponses> that have been received from the Service.
        public ConcurrentQueue<WebSocketResponse> responseQueue = new ConcurrentQueue<WebSocketResponse>();

        // Variable: responseCallbacks
        // A dictionary of unique request IDs and <ResponseCallbacks> that represent requests that are awaiting response from the Service.
        public Dictionary<string, ResponseCallback> responseCallbacks = new Dictionary<string, ResponseCallback>();

        // Variable: configStateQueue
        // A queue of <ConfigState> that have been received from the Service.
        public ConcurrentQueue<ConfigState> configStateQueue = new ConcurrentQueue<ConfigState>();

        // Variable: configStateCallbacks
        // A dictionary of unique request IDs and <ConfigStateCallbacks> that represent requests that are awaiting response from the Service.
        public Dictionary<string, ConfigStateCallback> configStateCallbacks = new Dictionary<string, ConfigStateCallback>();

        // Variable: serviceStatusQueue
        // A queue of <ServiceStatus> that have been received from the Service.
        public ConcurrentQueue<ServiceStatus> serviceStatusQueue = new ConcurrentQueue<ServiceStatus>();

        // Variable: serviceStatusCallbacks
        // A dictionary of unique request IDs and <ServiceStatusCallback> that represent requests that are awaiting response from the Service.
        public Dictionary<string, ServiceStatusCallback> serviceStatusCallbacks = new Dictionary<string, ServiceStatusCallback>();

        // Used to store HandPresenceState changes as they are recieved and emit messages
        // appropriately. "PROCESSED" when there are no unprocessed changes.
        internal HandPresenceState handState = HandPresenceState.PROCESSED;

        // Used to ensure UP events are sent at the correct position relative to the previous
        // MOVE event.
        // This is required due to the culling of events from the actionQueue in CheckForAction.
        Vector2 lastKnownCursorPosition = new Vector2();

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
            CheckQueue<WebSocketResponse>(responseQueue, HandleResponse);
            CheckQueue<ConfigState>(configStateQueue, HandleConfigState);

            CheckForAction();
        }

        void CheckQueue<T>(ConcurrentQueue<T> queue, Action<T> handler)
        {
            T queueItem;

            if (queue.TryPeek(out queueItem))
            {
                queue.TryDequeue(out queueItem);
                handler.Invoke(queueItem);
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
                "\n Status: " + _response.status + "\n Message: " + _response.message +
                "\n Original request - " + _response.originalRequest);
        }

        // Function: HandleConfigState
        // Checks the dictionary of <configStateCallbacks> for a matching request ID. If there is a
        // match, calls the callback action in the matching <ConfigStateCallback>.
        void HandleConfigState(ConfigState _configState)
        {
            foreach (KeyValuePair<string, ConfigStateCallback> callback in configStateCallbacks)
            {
                if (callback.Key == _configState.requestID)
                {
                    callback.Value.callback.Invoke(_configState);
                    configStateCallbacks.Remove(callback.Key);
                    break;
                }
            }
        }

        // Function: HandleServiceStatus
        // Checks the dictionary of <serviceStatusCallbacks> for a matching request ID. If there is a
        // match, calls the callback action in the matching <ServiceStatusCallback>.
        void HandleServiceStatus(ServiceStatus _serviceStatus)
        {
            foreach (KeyValuePair<string, ServiceStatusCallback> callback in serviceStatusCallbacks)
            {
                if (callback.Key == _serviceStatus.requestID)
                {
                    callback.Value.callback.Invoke(_serviceStatus);
                    serviceStatusCallbacks.Remove(callback.Key);
                    break;
                }
            }
        }

        // Function: CheckForAction
        // Checks <actionQueue> for valid <InputActions>. If there are too many in the queue,
        // clears out non-essential <InputActions> down to the number specified by
        // <actionCullToCount>. If any remain, sends the oldest <InputAction> to
        // <InputActionManager> to distribute the action.
        // UP <InputType>s have their positions set to the last known position to ensure
        // input events trigger correctly.
        void CheckForAction()
        {
            InputAction action = new InputAction();

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
        // expired from <configStateCallbacks>. Also clears all <ServiceStatusCallback> that are
        // expired from <serviceStatusCallbacks>.
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

                keys = new List<string>(serviceStatusCallbacks.Keys);

                foreach (string key in keys)
                {
                    if (serviceStatusCallbacks[key].timestamp < lastClearTime)
                    {
                        serviceStatusCallbacks.Remove(key);
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
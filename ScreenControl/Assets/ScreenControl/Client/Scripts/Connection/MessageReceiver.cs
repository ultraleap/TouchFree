using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;

namespace Ultraleap.ScreenControl.Client.Connection
{
    // Class: MessageReceiver
    // Handles the receiving of messages from the Service in an ordered manner.
    // Distributes the results of the messages to the respective managers.
    public class MessageReceiver : MonoBehaviour
    {
        // Group: Variables

        // Variable: callbackClearTimer
        // A timer in seconds to check <responseCallbacks> for outdated <ResponseCallback>s.
        const int callbackClearTimer = 300; // 5 minutes

        // Variable: serviceConnection
        // A private reference to the <serviceConnection> that created this <MessageReceiver>.
        ServiceConnection serviceConnection;

        // Variable: actionCullToCount
        // How many non-essential <ClientInputAction>s should <actionQueue> be trimmed to per frame.
        // This is used to ensure the Client can keep up with the Events sent over the sWebSocket.
        public int actionCullToCount = 2;

        // Variable: actionQueue
        // A queue of <ClientInputAction>s that have been received from the Service.
        public ConcurrentQueue<ClientInputAction> actionQueue = new ConcurrentQueue<ClientInputAction>();

        // Variable: responseQueue
        // A queue of <WebSocketResponse>s that have been received from the Service.
        public ConcurrentQueue<WebSocketResponse> responseQueue = new ConcurrentQueue<WebSocketResponse>();

        // Variable: responseCallbacks
        // A dictinary of unique request IDs and <ResponseCallback>s that represent requests that are awaiting response from the Service.
        public Dictionary<string, ResponseCallback> responseCallbacks = new Dictionary<string, ResponseCallback>();

        // Group: Functions

        // Function: SetWSConnection
        // Used to set the <serviceConnection> reference to the <ServiceConnection> that created this instance.
        public void SetWSConnection(ServiceConnection _connection)
        {
            serviceConnection = _connection;
        }

        // Function: Start
        // Unitys initialization function. Used to begin the <ClearUnresponsiveCallbacks> coroutine that handles the clearing of
        // expired elements in <responseCallbacks>.
        void Start()
        {
            StartCoroutine(ClearUnresponsiveCallbacks());
        }

        // Function: Update
        // Unitys update function. Checks all queues for messages to handle.
        void Update()
        {
            CheckForResponse();
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
            foreach(KeyValuePair<string, ResponseCallback> callback in responseCallbacks)
            {
                if(callback.Key == _response.requestID)
                {
                    callback.Value.callback.Invoke(_response);
                    responseCallbacks.Remove(callback.Key);
                    break;
                }
            }
        }

        // Function: CheckForAction
        // Checks <actionQueue> for valid <ClientInputAction>s. If there are too many in the queue, 
        // clears out as many non-essential <ClientInputAction>s as required by <actionCullToCount> and
        // sends the oldest <ClientInputAction> that remains to <serviceConnection> to handle the action.
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
                serviceConnection.HandleInputAction(action);
            }
        }

        // Group: Coroutine Functions

        // Function: ClearUnresponsiveCallbacks
        // Waits for <callbackClearTimer> seconds and clears all <responseCallbacks> that are expired.
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
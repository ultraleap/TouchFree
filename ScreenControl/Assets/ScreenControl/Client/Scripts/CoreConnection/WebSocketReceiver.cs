using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;

using Ultraleap.ScreenControl.Client.ScreenControlTypes;

namespace Ultraleap.ScreenControl.Client
{
    public class WebSocketReceiver : MonoBehaviour
    {
        const int callbackClearTimer = 300; // 5 minutes
        WebSocketCoreConnection coreConnection;
        public int actionCullToCount = 2;

        public ConcurrentQueue<ClientInputAction> actionQueue = new ConcurrentQueue<ClientInputAction>();
        public ConcurrentQueue<WebSocketResponse> responseQueue = new ConcurrentQueue<WebSocketResponse>();

        public Dictionary<string, ResponseCallback> responseCallbacks = new Dictionary<string, ResponseCallback>();

        public void SetWSConnection(WebSocketCoreConnection _connection)
        {
            coreConnection = _connection;
        }

        void Start()
        {
            StartCoroutine(ClearUnresponsiveCallbacks());
        }

        IEnumerator ClearUnresponsiveCallbacks()
        {
            WaitForSeconds waitTime = new WaitForSeconds(callbackClearTimer);

            while (true)
            {
                int lastClearTime = System.DateTime.Now.Millisecond;

                yield return waitTime;

                List<string> keys = new List<string>(responseCallbacks.Keys);

                foreach(string key in keys)
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

        void Update()
        {
            CheckForResponse();
            CheckForAction();
        }

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

        public void HandleResponse(WebSocketResponse _response)
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
                coreConnection.HandleInputAction(action);
            }
        }
    }
}
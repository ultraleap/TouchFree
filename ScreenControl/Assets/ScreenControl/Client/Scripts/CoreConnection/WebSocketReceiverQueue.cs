using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Ultraleap.ScreenControl.Client.ScreenControlTypes;
using System.Collections.Concurrent;

namespace Ultraleap.ScreenControl.Client
{
    public class WebSocketReceiverQueue : MonoBehaviour
    {
        public WebSocketCoreConnection coreConnection;
        public int actionCullToCount = 2;

        public ConcurrentQueue<ClientInputAction> actionQueue = new ConcurrentQueue<ClientInputAction>();
        public ConcurrentQueue<ConfigResponse> responseQueue = new ConcurrentQueue<ConfigResponse>();

        void Update()
        {
            CheckForResponse();
            CheckForAction();
        }

        void CheckForResponse()
        {
            ConfigResponse response;
            if (responseQueue.TryPeek(out response))
            {
                // Parse newly received messages
                responseQueue.TryDequeue(out response);
                coreConnection.HandleConfigResponse(response);
            }
        }

        void CheckForAction()
        {
            ClientInputAction action;
            while (actionQueue.Count > actionCullToCount)
            {
                if (actionQueue.TryPeek(out action))
                {
                    // Stop shrinking the queue if we havea 'key' input event
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
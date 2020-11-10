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

        public ConcurrentQueue<ClientInputAction> receiveQueue = new ConcurrentQueue<ClientInputAction>();

        void Update()
        {
            ClientInputAction action;
            while (receiveQueue.TryPeek(out action))
            {
                // Parse newly received messages
                receiveQueue.TryDequeue(out action);
                coreConnection.HandleInputAction(action);

                // Stop going through the queue this frame if we have just handled a 'key' input event
                if (action.InputType != InputType.MOVE)
                {
                    break;
                }
            }
        }
    }
}
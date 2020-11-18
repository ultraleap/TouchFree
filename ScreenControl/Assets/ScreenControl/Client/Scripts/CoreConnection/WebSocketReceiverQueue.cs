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
        public int queueCullLimit = 2;

        public ConcurrentQueue<ClientInputAction> receiveQueue = new ConcurrentQueue<ClientInputAction>();

        void Update()
        {
            ClientInputAction action;
            while (receiveQueue.Count > queueCullLimit)
            {
                receiveQueue.TryDequeue(out action);

                // Stop skipping through the queue this frame if we have just handled a 'key' input event
                if (action.InputType != InputType.MOVE)
                {
                    coreConnection.HandleInputAction(action);
                    return;
                }
            }

            // Parse newly received messages
            receiveQueue.TryDequeue(out action);
            coreConnection.HandleInputAction(action);
        }
    }
}
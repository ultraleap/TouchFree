using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Concurrent;

namespace Ultraleap.ScreenControl.Service
{
    public class WebSocketReceiverQueue : MonoBehaviour
    {
        public WebsocketClientConnection clientConnection;

        public ConcurrentQueue<string> setConfigQueue = new ConcurrentQueue<string>();

        void Update()
        {
            string content;
            if (setConfigQueue.TryPeek(out content))
            {
                // Parse newly received messages
                setConfigQueue.TryDequeue(out content);
                clientConnection.SetConfigState(content);
            }
        }
    }
}
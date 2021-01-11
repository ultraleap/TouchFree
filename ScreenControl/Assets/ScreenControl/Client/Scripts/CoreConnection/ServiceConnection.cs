using System;
using System.Text.RegularExpressions;
using UnityEngine;

using WebSocketSharp;

namespace Ultraleap.ScreenControl.Client.Connection
{
    // Class: ServiceConnection
    // This represents a connection to a ScreenControlService. It should be created by a
    // <ConnectionManager> to ensure there is only one active connection at a time. The sending
    // and receiving of data to the client is handled here as well as the creation of a
    // <WebSocketReceiver> to ensure the data is handled properly.
    public class ServiceConnection
    {
        // Group: Variables

        // Delegate: ClientInputActionEvent
        // An Action to distribute a <ClientInputAction> via the <TransmitInputAction> event listener.
        public delegate void ClientInputActionEvent(ClientInputAction _inputData);
        // Variable: TransmitInputAction
        // An event for transmitting <ClientInputAction> that are received via the <webSocket> to be listened to.
        public event ClientInputActionEvent TransmitInputAction;

        // Variable: webSocket
        // A reference to the websocket we are connected to.
        WebSocket webSocket;

        // Variable: receiver
        // A reference to the receiver that handles ordered queueing of data received via the <webSocket>.
        WebSocketReceiver receiver;

        // Group: Functions

        // Function: ServiceConnection
        // The initializer for <ServiceConnection> that can be given a different IP Address and Port to connect to
        // on initialization. This initializer also sets up the <receiver> for future use and the listener for the 
        // receiving of messages.
        internal ServiceConnection(string _ip = "127.0.0.1", string _port = "9739")
        {
            webSocket = new WebSocket($"ws://{_ip}:{_port}/connect");
            WebSocketSharp.Net.Cookie cookie = new WebSocketSharp.Net.Cookie(VersionInfo.API_HEADER_NAME, VersionInfo.ApiVersion.ToString());
            webSocket.SetCookie(cookie);

            webSocket.OnMessage += (sender, e) =>
            {
                OnMessage(e);
            };

            webSocket.Connect();

            receiver = ConnectionManager.Instance.gameObject.AddComponent<WebSocketReceiver>();
            receiver.SetWSConnection(this);
        }

        // Function: Disconnect
        // Can be used to force the connection to the <webSocket> to be closed. Also destroys the <receiver>.
        public void Disconnect()
        {
            if (webSocket != null)
            {
                webSocket.Close();
            }

            if (receiver != null)
            {
                WebSocketReceiver.Destroy(receiver);
            }
        }

        // Function: OnMessage
        // The first point of contact for new messages received, these are sorted into appropriate types based on their
        // <ActionCodes> and added to queues on the <receiver>.
        public void OnMessage(MessageEventArgs _message)
        {
            string rawData = _message.Data;

            // Find key areas of the rawData, the "action" and the "content"
            Match match = Regex.Match(rawData, "{\"action\":\"([\\w\\d_]+?)\",\"content\":({.+?})}$");

            // "action" = match.Groups[1] // "content" = match.Groups[2]
            ActionCodes action = (ActionCodes)Enum.Parse(typeof(ActionCodes), match.Groups[1].ToString());
            string content = match.Groups[2].ToString();

            switch (action)
            {
                case ActionCodes.INPUT_ACTION:
                    WebsocketInputAction wsInput = JsonUtility.FromJson<WebsocketInputAction>(content);
                    ClientInputAction cInput = new ClientInputAction(wsInput);
                    receiver.actionQueue.Enqueue(cInput);
                    break;

                case ActionCodes.CONFIGURATION_STATE:
                    break;

                case ActionCodes.CONFIGURATION_RESPONSE:
                    WebSocketResponse response = JsonUtility.FromJson<WebSocketResponse>(content);
                    receiver.responseQueue.Enqueue(response);
                    break;
            }
        }

        // Function: HandleInputAction
        // Called by the <receiver> to relay a <ClientInputAction> that has ben received to any listeners of <TransmitInputAction>.
        public void HandleInputAction(ClientInputAction _action)
        {
            TransmitInputAction?.Invoke(_action);
        }

        // Function: SendMessage
        // Used to send or request information from the Service via the <webSocket>. To be given a pre-made <_message> and <_requestID>.
        // Provides an asynchronous <WebSocketResponse> via the <_callback> parameter.
        internal void SendMessage(string _message, string _requestID, Action<WebSocketResponse> _callback)
        {
            if (_requestID == "")
            {
                if (_callback != null)
                {
                    WebSocketResponse response = new WebSocketResponse("", "Failure", "Request failed. This is due to a missing or invalid requestID", _message);
                    _callback.Invoke(response);
                }

                Debug.LogError("Request failed. This is due to a missing or invalid requestID");
                return;
            }

            if (_callback != null)
            {
                receiver.responseCallbacks.Add(_requestID, new ResponseCallback(DateTime.Now.Millisecond, _callback));
            }

            webSocket.Send(_message);
        }
    }
}
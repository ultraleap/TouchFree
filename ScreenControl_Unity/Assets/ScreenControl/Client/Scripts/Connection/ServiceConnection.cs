using System;
using System.Text.RegularExpressions;
using UnityEngine;

using WebSocketSharp;

namespace Ultraleap.ScreenControl.Client.Connection
{
    // Class: ServiceConnection
    // This represents a connection to a ScreenControl Service. It should be created by a
    // <ConnectionManager> to ensure there is only one active connection at a time. The sending
    // and receiving of data to the client is handled here as well as the creation of a
    // <MessageReceiver> to ensure the data is handled properly.
    public class ServiceConnection
    {
        // Group: Variables

        // Variable: webSocket
        // A reference to the websocket we are connected to.
        WebSocket webSocket;

        // Group: Functions

        // Function: ServiceConnection
        // The constructor for <ServiceConnection> that can be given a different IP Address and Port
        // to connect to on construction. This constructor also sets up the <receiver> for future
        // use and redirects incoming messages to <OnMessage>.
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
        }

        // Function: Disconnect
        // Can be used to force the connection to the <webSocket> to be closed. Also destroys the <receiver>.
        public void Disconnect()
        {
            if (webSocket != null)
            {
                webSocket.Close();
            }
        }

        // Function: OnMessage
        // The first point of contact for new messages received, these are sorted into appropriate types based on their
        // <ActionCode> and added to queues on the <receiver>.
        public void OnMessage(MessageEventArgs _message)
        {
            string rawData = _message.Data;

            // Find key areas of the rawData, the "action" and the "content"
            Match match = Regex.Match(rawData, "{\"action\":\"([\\w\\d_]+?)\",\"content\":({.+?})}$");

            // "action" = match.Groups[1] // "content" = match.Groups[2]
            ActionCode action = (ActionCode)Enum.Parse(typeof(ActionCode), match.Groups[1].ToString());
            string content = match.Groups[2].ToString();

            switch (action)
            {
                case ActionCode.INPUT_ACTION:
                    WebsocketInputAction wsInput = JsonUtility.FromJson<WebsocketInputAction>(content);
                    ClientInputAction cInput = new ClientInputAction(wsInput);
                    ConnectionManager.messageReceiver.actionQueue.Enqueue(cInput);
                    break;

                case ActionCode.CONFIGURATION_STATE:
                    break;

                case ActionCode.CONFIGURATION_RESPONSE:
                    WebSocketResponse response = JsonUtility.FromJson<WebSocketResponse>(content);
                    ConnectionManager.messageReceiver.responseQueue.Enqueue(response);
                    break;
            }
        }

        // Function: SendMessage
        // Used internally to send or request information from the Service via the <webSocket>. To
        // be given a pre-made _message and _requestID. Provides an asynchronous <WebSocketResponse>
        // via the _callback parameter.
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
                ConnectionManager.messageReceiver.responseCallbacks.Add(_requestID, new ResponseCallback(DateTime.Now.Millisecond, _callback));
            }

            webSocket.Send(_message);
        }
    }
}
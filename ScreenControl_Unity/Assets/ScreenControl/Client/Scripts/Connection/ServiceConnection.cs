using System;
using System.Text.RegularExpressions;
using UnityEngine;

using WebSocketSharp;

namespace Ultraleap.ScreenControl.Client.Connection
{
    // Class: ServiceConnection
    // This represents a connection to a ScreenControl Service. It should be created by a
    // <ConnectionManager> to ensure there is only one active connection at a time. The sending
    // and receiving of data to the client is handled here.
    public class ServiceConnection
    {
        // Group: Variables

        // Variable: webSocket
        // A reference to the websocket we are connected to.
        WebSocket webSocket;

        // Variable: handshakeCompleted
        // Used internally in this class to know if the Version compatibility handshake with
        // the server has successfully completed.
        private Boolean handshakeCompleted;

        // Group: Functions

        // Function: ServiceConnection
        // The constructor for <ServiceConnection> that can be given a different IP Address and Port
        // to connect to on construction. This constructor also redirects incoming messages to
        // <OnMessage>. Once the websocket connection opens, a handshake request is sent with this
        // Client's API version number. The service will not send data over an open connection
        // until this handshake is completed succesfully.
        internal ServiceConnection(string _ip = "127.0.0.1", string _port = "9739")
        {
            handshakeCompleted = false;
            webSocket = new WebSocket($"ws://{_ip}:{_port}/connect");

            webSocket.OnMessage += (sender, e) =>
            {
                OnMessage(e);
            };

            webSocket.OnOpen += (sender, e) =>
            {
                // Send a handshake message with the API version of this client
                string guid = Guid.NewGuid().ToString();

                string handshakeMessage = "{";
                handshakeMessage += $"\"action\": \"{ActionCode.VERSION_HANDSHAKE.ToString()}\",";
                handshakeMessage += "\"content\": {";
                handshakeMessage += $"\"requestID\": \"{guid}\",";
                handshakeMessage += $"\"{VersionInfo.API_HEADER_NAME}\": \"{VersionInfo.ApiVersion}\"";
                handshakeMessage += "}}";

                SendMessage(handshakeMessage, guid, ConnectionResultCallback);
            };

            webSocket.Connect();
        }

        // Function: ConnectionResultCallback
        // Passed into <SendMessage> as part of connecting to ScreenControl Service, handles the
        // result of the Version Checking handshake.
        private void ConnectionResultCallback(WebSocketResponse response)
        {
            // if failed, console log
            // if succeeded, "complete" connecting and allow message sending/recieving
            if (response.status == "Success")
            {
                handshakeCompleted = true;
            }
            else
            {
                Debug.Log($"Connection to Service failed. Details:\n{response.message}");
            }
        }

        // Function: Disconnect
        // Can be used to force the connection to the <webSocket> to be closed.
        public void Disconnect()
        {
            if (webSocket != null)
            {
                webSocket.Close();
            }
        }

        // Function: OnMessage
        // The first point of contact for new messages received, these are sorted into appropriate
        // types based on their <ActionCode> and added to queues on the <ConnectionManager's>
        // <MessageReceiver>.
        public void OnMessage(MessageEventArgs _message)
        {
            string rawData = _message.Data;

            // Find key areas of the rawData, the "action" and the "content"
            Match match = Regex.Match(rawData, "{\"action\": ?\"([\\w\\d_]+?)\",\"content\": ?({.+?})}$");

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
                    ConfigState configState = JsonUtility.FromJson<ConfigState>(content);
                    ConnectionManager.messageReceiver.configStateQueue.Enqueue(configState);
                    break;
                case ActionCode.CONFIGURATION_RESPONSE:
                case ActionCode.VERSION_HANDSHAKE_RESPONSE:
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

        // Function: RequestConfigState
        // Used internally to request a <ConfigState> from the Service via the <webSocket>.
        // Provides an asynchronous <ConfigState> via the _callback parameter.
        internal void RequestConfigState(Action<ConfigState> _callback)
        {
            string requestID = Guid.NewGuid().ToString();
            ConfigChangeRequest request = new ConfigChangeRequest(requestID);

            CommunicationWrapper<ConfigChangeRequest> message =
                new CommunicationWrapper<ConfigChangeRequest>(ActionCode.REQUEST_CONFIGURATION_STATE.ToString(), request);

            string jsonMessage = JsonUtility.ToJson(message);

            if (_callback != null)
            {
                ConnectionManager.messageReceiver.configStateCallbacks.Add(requestID, new ConfigStateCallback(DateTime.Now.Millisecond, _callback));
            }

            webSocket.Send(jsonMessage);
        }
    }
}
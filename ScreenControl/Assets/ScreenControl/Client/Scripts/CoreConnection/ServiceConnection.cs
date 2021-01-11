using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

using WebSocketSharp;

using Ultraleap.ScreenControl.Client.ScreenControlTypes;

namespace Ultraleap.ScreenControl.Client.Connection
{
    public class ServiceConnection
    {
        public delegate void ClientInputActionEvent(ClientInputAction _inputData);
        public event ClientInputActionEvent TransmitInputAction;

        WebSocket ws;
        WebSocketReceiver receiverQueue;

        internal ServiceConnection(string _ip = "127.0.0.1", string _port = "9739")
        {
            ws = new WebSocket($"ws://{_ip}:{_port}/connect");
            WebSocketSharp.Net.Cookie cookie = new WebSocketSharp.Net.Cookie(VersionInfo.API_HEADER_NAME, VersionInfo.ApiVersion.ToString());
            ws.SetCookie(cookie);

            ws.OnMessage += (sender, e) =>
            {
                OnMessage(e);
            };

            ws.Connect();

            receiverQueue = ConnectionManager.Instance.gameObject.AddComponent<WebSocketReceiver>();
            receiverQueue.SetWSConnection(this);
        }

        public void Disconnect()
        {
            if (ws != null)
            {
                ws.Close();
            }

            if (receiverQueue != null)
            {
                WebSocketReceiver.Destroy(receiverQueue);
            }
        }

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
                    receiverQueue.actionQueue.Enqueue(cInput);
                    break;

                case ActionCodes.CONFIGURATION_STATE:
                    break;

                case ActionCodes.CONFIGURATION_RESPONSE:
                    WebSocketResponse response = JsonUtility.FromJson<WebSocketResponse>(content);
                    receiverQueue.responseQueue.Enqueue(response);
                    break;
            }
        }

        public void HandleInputAction(ClientInputAction _action)
        {
            TransmitInputAction?.Invoke(_action);
        }

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
                receiverQueue.responseCallbacks.Add(_requestID, new ResponseCallback(DateTime.Now.Millisecond, _callback));
            }

            ws.Send(_message);
        }
    }
}
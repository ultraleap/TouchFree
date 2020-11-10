using System;
using UnityEngine;
using System.Text.RegularExpressions;

using WebSocketSharp;

using Ultraleap.ScreenControl.Client.ScreenControlTypes;

namespace Ultraleap.ScreenControl.Client
{
    internal enum ActionCodes
    {
        INPUT_ACTION,
        CONFIGURATION_STATE,
        CONFIGURATION_RESPONSE
    }

    public class WebSocketCoreConnection : CoreConnection
    {
        WebSocket ws;
        WebSocketReceiverQueue receiverQueue;

        internal WebSocketCoreConnection(string _ip = "127.0.0.1", string _port = "9739")
        {
            ws = new WebSocket($"ws://{_ip}:{_port}/connect");
            WebSocketSharp.Net.Cookie cookie = new WebSocketSharp.Net.Cookie(VersionInfo.API_HEADER_NAME, VersionInfo.ApiVersion.ToString());
            ws.SetCookie(cookie);

            ws.OnMessage += (sender, e) =>
            {
                HandleCommunication(e.Data);
            };
            ws.Connect();

            receiverQueue = ConnectionManager.Instance.gameObject.AddComponent<WebSocketReceiverQueue>();
            receiverQueue.coreConnection = this;
        }

        public override void Disconnect()
        {
            if (ws != null)
            {
                ws.Close();
            }

            if(receiverQueue != null)
            {
                WebSocketReceiverQueue.Destroy(receiverQueue);
            }
        }

        public void HandleCommunication(string _rawData)
        {
            // Find key areas of the rawData, the "action" and the "content"
            var match = Regex.Match(_rawData, "{\"action\":\"([\\w\\d_]+?)\",\"content\":({.+?})}");

            // "action" = match.Groups[1] | "content" = match.Groups[2]
            ActionCodes action = (ActionCodes)Enum.Parse(typeof(ActionCodes), match.Groups[1].ToString());
            string content = match.Groups[2].ToString();

            switch (action)
            {
                case ActionCodes.INPUT_ACTION:
                    WebsocketInputAction wsInput = JsonUtility.FromJson<WebsocketInputAction>(content);
                    ClientInputAction cInput = new ClientInputAction(wsInput);
                    receiverQueue.receiveQueue.Enqueue(cInput);
                    break;
                case ActionCodes.CONFIGURATION_STATE:
                    break;
                case ActionCodes.CONFIGURATION_RESPONSE:
                    break;
            }
        }

        public void HandleInputAction(ClientInputAction _action)
        {
            RelayInputAction(_action);
        }
    }
}
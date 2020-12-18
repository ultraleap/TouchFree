using System;
using UnityEngine;
using System.Text.RegularExpressions;
using WebSocketSharp;

using Ultraleap.ScreenControl.Client.ScreenControlTypes;

namespace Ultraleap.ScreenControl.Client
{
    public class WebSocketCoreConnection : CoreConnection
    {
        WebSocket ws;
        WebSocketReceiver receiverQueue;

        internal WebSocketCoreConnection(string _ip = "127.0.0.1", string _port = "9739")
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

        public override void Disconnect()
        {
            if (ws != null)
            {
                ws.Close();
            }

            if(receiverQueue != null)
            {
                WebSocketReceiver.Destroy(receiverQueue);
            }
        }

        public void OnMessage(MessageEventArgs _message)
        {
            string rawData = _message.Data;

            // Find key areas of the rawData, the "action" and the "content"
            var match = Regex.Match(rawData, "{\"action\":\"([\\w\\d_]+?)\",\"content\":({.+?})}$");

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
            RelayInputAction(_action);
        }

        public void SetConfigState(PhysicalConfig _physical, Action<WebSocketResponse> _callback = null)
        {
            SetConfigState(null, _physical, _callback);
        }

        public void SetConfigState(InteractionConfig _interaction, Action<WebSocketResponse> _callback = null)
        {
            SetConfigState(_interaction, null, _callback);
        }

        public void SetConfigState(InteractionConfig _interaction, PhysicalConfig _physical, Action<WebSocketResponse> _callback = null)
        {
            string action = ActionCodes.SET_CONFIGURATION_STATE.ToString();
            Guid requestGUID = Guid.NewGuid();
            string requestID = requestGUID.ToString();

            string jsonContent = "";
            jsonContent += "{\"action\":\"";
            jsonContent += action + "\",\"content\":{\"requestID\":\"";
            jsonContent += requestID + "\",";

            if(_interaction != null)
            {
                jsonContent += SerializeInteractionConfig(_interaction);
            }

            if (_physical != null)
            {
                jsonContent += SerializePhysicalConfig(_physical);
            }

            // last element added was final so remove the comma
            jsonContent = jsonContent.Remove(jsonContent.Length - 1);

            jsonContent += "}}";

            SendMessage(jsonContent, requestID, _callback);
        } 

        string SerializeInteractionConfig(InteractionConfig _interaction)
        {
            string newContent = "";

            if (_interaction.configValues.Count > 0 || _interaction.HoverAndHold.configValues.Count > 0)
            {
                newContent += "\"interaction\":{";

                foreach (var value in _interaction.configValues)
                {
                    newContent += JsonUtilities.ConvertToJson(value.Key, value.Value);
                    newContent += ",";
                }

                if (_interaction.HoverAndHold.configValues.Count > 0)
                {
                    newContent += "\"HoverAndHold\":{";

                    foreach (var value in _interaction.HoverAndHold.configValues)
                    {
                        newContent += JsonUtilities.ConvertToJson(value.Key, value.Value);
                        newContent += ",";
                    }

                    // last element added was last in the list so remove the comma
                    newContent = newContent.Remove(newContent.Length - 1);
                    newContent += "},";
                }

                // last element added was last in the list so remove the comma
                newContent = newContent.Remove(newContent.Length - 1);

                newContent += "},";
            }

            return newContent;
        }

        string SerializePhysicalConfig(PhysicalConfig _physical)
        {
            string newContent = "";

            if (_physical.configValues.Count > 0)
            {
                if (_physical.configValues.Count > 0)
                {
                    newContent += ",\"physical\":{";

                    foreach (var value in _physical.configValues)
                    {
                        newContent += JsonUtilities.ConvertToJson(value.Key, value.Value);
                        newContent += ",";
                    }

                    // last element added was last in the list so remove the comma
                    newContent = newContent.Remove(newContent.Length - 1);
                    newContent += "},";
                }
            }

            return newContent;
        }

        internal void SendMessage(string _message, string _requestID, Action<WebSocketResponse> _callback)
        {
            if(_requestID == "")
            {
                if(_callback != null)
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
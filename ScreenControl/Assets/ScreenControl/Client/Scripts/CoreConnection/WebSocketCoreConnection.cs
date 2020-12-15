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
        WebSocketReceiverQueue receiverQueue;

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
                    ConfigResponse response = JsonUtility.FromJson<ConfigResponse>(content);
                    receiverQueue.responseQueue.Enqueue(response);
                    break;
            }
        }

        public void HandleInputAction(ClientInputAction _action)
        {
            RelayInputAction(_action);
        }

        public void HandleConfigResponse(ConfigResponse _response)
        {
            switch (_response.status)
            {
                case "Success":
                    Debug.Log("Response!!");
                    break;
                case "Failed":
                    Debug.LogError("Request " + _response.requestID + " failed with result: " + _response.message);
                    break;
            }
        }

        public void SetConfigState(InteractionConfig _interaction = null, PhysicalConfig _physical = null)
        {
            string action = ActionCodes.SET_CONFIGURATION_STATE.ToString();
            Guid requestGUID = Guid.NewGuid();
            string requestID = requestGUID.ToString();

            string setContent = "";
            setContent += "{\"action\":\"";
            setContent += action + "\",\"content\":{\"requestID\":\"";
            setContent += requestID + "\",";

            if(_interaction != null)
            {
                if (_interaction.configValues.Count > 0 || _interaction.HoverAndHold.configValues.Count > 0)
                {
                    setContent += "\"interaction\":{";

                    foreach(var value in _interaction.configValues)
                    {
                        setContent += JsonUtilities.ConvertToJson(value.Key, value.Value);
                        setContent += ",";
                    }

                    if(_interaction.HoverAndHold.configValues.Count > 0)
                    {
                        setContent += "\"HoverAndHold\":{";

                        foreach (var value in _interaction.HoverAndHold.configValues)
                        {
                            setContent += JsonUtilities.ConvertToJson(value.Key, value.Value);
                            setContent += ",";
                        }

                        // last element added was last in the list so remove the comma
                        setContent = setContent.Remove(setContent.Length - 1);
                        setContent += "},";
                    }

                    // last element added was last in the list so remove the comma
                    setContent = setContent.Remove(setContent.Length - 1);

                    setContent += "},";
                }
            }

            if (_physical != null)
            {
                if (_physical.configValues.Count > 0)
                {
                    if (_physical.configValues.Count > 0)
                    {
                        setContent += ",\"physical\":{";

                        foreach (var value in _physical.configValues)
                        {
                            setContent += JsonUtilities.ConvertToJson(value.Key, value.Value);
                            setContent += ",";
                        }

                        // last element added was last in the list so remove the comma
                        setContent = setContent.Remove(setContent.Length - 1);
                        setContent += "},";
                    }
                }
            }

            // last element added was final so remove the comma
            setContent = setContent.Remove(setContent.Length - 1);

            setContent += "}}";

            SendMessage(setContent);
        }        

        public void SendMessage(string _message)
        {
            ws.Send(_message);
        }
    }
}
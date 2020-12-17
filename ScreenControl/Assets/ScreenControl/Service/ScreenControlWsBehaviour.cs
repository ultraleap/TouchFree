using System;

using UnityEngine;

using WebSocketSharp;
using System.Text.RegularExpressions;
using WebSocketSharp.Server;

using Ultraleap.ScreenControl.Core.ScreenControlTypes;
using Ultraleap.ScreenControl.Service.ScreenControlTypes;

namespace Ultraleap.ScreenControl.Service
{
    internal class ScreenControlWsBehaviour : WebSocketBehavior
    {
        public WebSocketClientConnection clientConnection;

        public void SendInputAction(CoreInputAction _data)
        {
            WebsocketInputAction converted = new WebsocketInputAction(_data);

            CommunicationWrapper<WebsocketInputAction> message =
                new CommunicationWrapper<WebsocketInputAction>(ActionCodes.INPUT_ACTION.ToString(), converted);

            string jsonMessage = JsonUtility.ToJson(message);

            Send(jsonMessage);
        }

        public void SendConfigurationResponse(ConfigResponse _response)
        {
            CommunicationWrapper<ConfigResponse> message =
                new CommunicationWrapper<ConfigResponse>(ActionCodes.CONFIGURATION_RESPONSE.ToString(), _response);

            string jsonMessage = JsonUtility.ToJson(message);

            Send(jsonMessage);
        }

        protected override void OnOpen()
        {
            var cookies = Context.CookieCollection;

            if (cookies.Count > 0)
            {
                string cookieApiVersion = cookies[0].Value;

                if (cookieApiVersion != null &&
                        GetVersionCompability(cookieApiVersion, VersionInfo.ApiVersion) == Compatibility.COMPATIBLE)
                {
                    Debug.Log("Websocket Connection opened successfully");
                }
                else
                {
                    if (cookieApiVersion == null)
                    {
                        Debug.LogError("No API version header was provided on connect!");
                    }
                    else
                    {
                        string errorMsg = $"Client API version of {cookieApiVersion} was incompatible with the Service's Core API Version of {VersionInfo.ApiVersion}";
                        Debug.LogError(errorMsg);
                        Close(CloseStatusCode.PolicyViolation, errorMsg);
                    }
                }
            }
            return;
        }

        protected override void OnClose(CloseEventArgs eventArgs)
        {
            Debug.Log("Websocket Connection closed");
        }

        private Compatibility GetVersionCompability(string _clientVersion, Version _coreVersion)
        {
            Version clientVersionParsed = new Version(_clientVersion);

            if (clientVersionParsed.Major < _coreVersion.Major ||
                clientVersionParsed.Minor < _coreVersion.Minor)
            {
                return Compatibility.CLIENT_OUTDATED;
            }
            else if (clientVersionParsed.Major > _coreVersion.Major ||
                     clientVersionParsed.Minor > _coreVersion.Minor)
            {
                return Compatibility.CORE_OUTDATED;
            }

            if (clientVersionParsed.Build > _coreVersion.Build)
            {
                return Compatibility.CORE_OUTDATED;
            }

            return Compatibility.COMPATIBLE;
        }

        protected override void OnMessage(MessageEventArgs _message)
        {
            string rawData = _message.Data;

            // Find key areas of the rawData, the "action" and the "content"
            var match = Regex.Match(rawData, "{\"action\":\"([\\w\\d_]+?)\",\"content\":({.+?})}$");

            // "action" = match.Groups[1] // "content" = match.Groups[2]
            ActionCodes action = (ActionCodes)Enum.Parse(typeof(ActionCodes), match.Groups[1].ToString());
            string content = match.Groups[2].ToString();

            switch (action)
            {
                case ActionCodes.SET_CONFIGURATION_STATE:
                    clientConnection.receiverQueue.setConfigQueue.Enqueue(content);
                    break;
                case ActionCodes.REQUEST_CONFIGURATION_STATE:
                    Debug.LogError("Handling " + action + " is not yet implemented.");
                    break;
                default:
                    Debug.LogError("Received a " + action + " action. This is not a valid request.");
                    break;
            }
        }
    }
}
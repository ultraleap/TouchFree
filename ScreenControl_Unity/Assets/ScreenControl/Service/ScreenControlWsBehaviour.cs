using System;

using UnityEngine;

using WebSocketSharp;
using System.Text.RegularExpressions;
using WebSocketSharp.Server;

using Ultraleap.ScreenControl.Core.ScreenControlTypes;
using Ultraleap.ScreenControl.Service.ScreenControlTypes;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Ultraleap.ScreenControl.Service
{
    internal class ScreenControlWsBehaviour : WebSocketBehavior
    {
        public WebSocketClientConnection clientConnection;
        private Boolean HandshakeCompleted;

        public void SendInputAction(CoreInputAction _data)
        {
            if (!HandshakeCompleted)
            {
                // Long-term we shouldn't get this far until post-handshake, but the systems should
                // be designed cohesively when the Service gets its polish
                return;
            }

            WebsocketInputAction converted = new WebsocketInputAction(_data);

            CommunicationWrapper<WebsocketInputAction> message =
                new CommunicationWrapper<WebsocketInputAction>(ActionCode.INPUT_ACTION.ToString(), converted);

            string jsonMessage = JsonUtility.ToJson(message);

            Send(jsonMessage);
        }

        public void SendHandshakeResponse(ResponseToClient _response)
        {
            CommunicationWrapper<ResponseToClient> message =
                new CommunicationWrapper<ResponseToClient>(
                    ActionCode.VERSION_HANDSHAKE_RESPONSE.ToString(),
                    _response);

            string jsonMessage = JsonUtility.ToJson(message);

            Send(jsonMessage);
        }

        public void SendConfigurationResponse(ResponseToClient _response)
        {
            CommunicationWrapper<ResponseToClient> message =
                new CommunicationWrapper<ResponseToClient>(ActionCode.CONFIGURATION_RESPONSE.ToString(), _response);

            string jsonMessage = JsonUtility.ToJson(message);

            Send(jsonMessage);
        }

        protected override void OnOpen()
        {
            Debug.Log("Websocket Connection opened");

            HandshakeCompleted = false;
        }

        protected override void OnClose(CloseEventArgs eventArgs)
        {
            Debug.Log("Websocket Connection closed");

            HandshakeCompleted = false;
        }

        private Compatibility GetVersionCompability(string _clientVersion, Version _coreVersion)
        {
            Version clientVersionParsed = new Version(_clientVersion);

            if (clientVersionParsed.Major < _coreVersion.Major)
            {
                return Compatibility.CLIENT_OUTDATED;
            }
            else if (clientVersionParsed.Major > _coreVersion.Major)
            {
                return Compatibility.SERVICE_OUTDATED;
            }

            else if (clientVersionParsed.Minor < _coreVersion.Minor)
            {
                return Compatibility.CLIENT_OUTDATED;
            }
            else if (clientVersionParsed.Minor > _coreVersion.Minor)
            {
                return Compatibility.SERVICE_OUTDATED;
            }

            if (clientVersionParsed.Build > _coreVersion.Build)
            {
                return Compatibility.SERVICE_OUTDATED;
            }

            return Compatibility.COMPATIBLE;
        }

        protected override void OnMessage(MessageEventArgs _message)
        {
            string rawData = _message.Data;

            // Find key areas of the rawData, the "action" and the "content"
            var match = Regex.Match(rawData, "{\"action\": ?\"([\\w\\d_]+?)\",\"content\": ?({.+?})}$");

            // "action" = match.Groups[1] // "content" = match.Groups[2]
            ActionCode action = (ActionCode)Enum.Parse(typeof(ActionCode), match.Groups[1].ToString());
            string content = match.Groups[2].ToString();

            // New case for version Handshake
            // if anything comes in BEFORE version handshake, respond w/ an error

            if (!HandshakeCompleted)
            {
                ProcessHandshake(action, content);
                return;
            }

            switch (action)
            {
                case ActionCode.SET_CONFIGURATION_STATE:
                    clientConnection.receiverQueue.setConfigQueue.Enqueue(content);
                    break;
                case ActionCode.REQUEST_CONFIGURATION_STATE:
                    Debug.LogError("Handling " + action + " is not yet implemented.");
                    break;
                case ActionCode.INPUT_ACTION:
                case ActionCode.CONFIGURATION_STATE:
                case ActionCode.CONFIGURATION_RESPONSE:
                    Debug.LogError("Received a " + action + " action. This action is not expected on the Service.");
                    break;
                default:
                    Debug.LogError("Received a " + action + " action. This action is not recognised.");
                    break;
            }
        }

        protected void ProcessHandshake(ActionCode action, string requestContent)
        {
            JObject contentObj = JsonConvert.DeserializeObject<JObject>(requestContent);
            ResponseToClient response = new ResponseToClient("", "Success", "", requestContent);

            if (!contentObj.ContainsKey("requestID") || contentObj.GetValue("requestID").ToString() == "")
            {
                // Validation has failed because there is no valid requestID
                response.status = "Failure";
                response.message = "Handshaking failed. This is due to a missing or invalid requestID";
                Debug.LogError("Handshaking failed. This is due to a missing or invalid requestID");
                SendHandshakeResponse(response);
                return;
            }

            response.requestID = contentObj["requestID"].Value<string>();

            if (action != ActionCode.VERSION_HANDSHAKE)
            {
                // Send back immediate error
                // "Request made before Handshake completed"
                // Handshake hasn't been completed so other requests cannot be processed
                response.status = "Failure";
                response.message = "Request Rejected: Requests cannot be processed until handshaking is complete.";
                Debug.LogError("Request Rejected: Requests cannot be processed until handshaking is complete.");
                SendHandshakeResponse(response);
                return;
            }

            if (!contentObj.ContainsKey(VersionInfo.API_HEADER_NAME))
            {
                // Send back immediate error
                // "Request made before Handshake completed"
                // Handshake hasn't been completed so other requests cannot be processed
                response.status = "Failure";
                response.message = "Handshaking Failed: No API Version supplied.";
                Debug.LogError("Handshaking Failed: No API Version supplied.");
                SendHandshakeResponse(response);
                return;
            }

            string clientApiVersion = (string)contentObj[VersionInfo.API_HEADER_NAME];
            Compatibility compatibility = GetVersionCompability(clientApiVersion, VersionInfo.ApiVersion);

            switch (compatibility)
            {
                case Compatibility.COMPATIBLE:
                    HandshakeCompleted = true;
                    response.status = "Success";
                    response.message = "Handshake Successful";
                    Debug.Log("Handshake Successful");
                    SendHandshakeResponse(response);
                    return;
                case Compatibility.CLIENT_OUTDATED:
                    // Construct and send an error response

                    response.message = "Handshake Failed: Client is outdated relative to Service.";
                    Debug.LogError("Handshake Failed: Client is outdated relative to Service.");
                    break;
                case Compatibility.SERVICE_OUTDATED:
                    // Construct and send an error response
                    response.message = "Handshake Failed: Service is outdated relative to Client.";
                    Debug.LogError("Handshake Failed: Service is outdated relative to Client.");
                    break;
            }

            response.status = "Failure";
            SendHandshakeResponse(response);
            return;
        }
    }
}
using System;

using UnityEngine;

using WebSocketSharp;
using System.Text.RegularExpressions;
using WebSocketSharp.Server;

using Ultraleap.TouchFree.ServiceShared;
using Ultraleap.TouchFree.Service.ServiceTypes;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Ultraleap.TouchFree.Service
{
    internal class ClientConnection : WebSocketBehavior
    {
        private bool HandshakeCompleted;

        public void SendInputAction(InputAction _data)
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

            ClientConnectionManager.Instance.InputActionSend(jsonMessage);

            Send(jsonMessage);
        }

        public void SendHandPresenceEvent(HandPresenceEvent _response)
        {
            CommunicationWrapper<HandPresenceEvent> message =
                new CommunicationWrapper<HandPresenceEvent>(
                    ActionCode.HAND_PRESENCE_EVENT.ToString(),
                    _response);

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

        public void SendConfigChangeResponse(ResponseToClient _response)
        {
            CommunicationWrapper<ResponseToClient> message =
                new CommunicationWrapper<ResponseToClient>(ActionCode.CONFIGURATION_RESPONSE.ToString(), _response);

            string jsonMessage = JsonUtility.ToJson(message);

            Send(jsonMessage);
        }

        public void SendConfigState(ConfigState _configState)
        {
            CommunicationWrapper<ConfigState> message =
                new CommunicationWrapper<ConfigState>(ActionCode.CONFIGURATION_STATE.ToString(), _configState);

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
            ClientConnectionManager.Instance.RemoveConnection(this);
        }

        private void SendInitialHandState()
        {
            this.SendHandPresenceEvent(ClientConnectionManager.Instance.missedHandPresenceEvent);
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
            var match = Regex.Match(rawData, "{\\s*?\"action\"\\s*?:\\s*?\"([\\w\\d_]+?)\"\\s*?,\\s*?\"content\"\\s*?:\\s*?({.+?})\\s*?}$");

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
                    ClientConnectionManager.Instance.receiverQueue.configChangeQueue.Enqueue(content);
                    break;
                case ActionCode.REQUEST_CONFIGURATION_STATE:
                    ClientConnectionManager.Instance.receiverQueue.configStateRequestQueue.Enqueue(content);
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
                // Send back immediate error: Handshake hasn't been completed so other requests
                // cannot be processed
                response.status = "Failure";
                response.message = "Request Rejected: Requests cannot be processed until handshaking is complete.";
                Debug.LogError("Request Rejected: Requests cannot be processed until handshaking is complete.");
                SendHandshakeResponse(response);
                return;
            }

            if (!contentObj.ContainsKey(VersionInfo.API_HEADER_NAME))
            {
                // Send back immediate error: Cannot compare version number w/o a version number
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
                    SendInitialHandState();
                    return;
                case Compatibility.CLIENT_OUTDATED:
                    response.message = "Handshake Failed: Client is outdated relative to Service.";
                    Debug.LogError("Handshake Failed: Client is outdated relative to Service.");
                    break;
                case Compatibility.SERVICE_OUTDATED:
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
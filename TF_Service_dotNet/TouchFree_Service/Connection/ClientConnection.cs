using System;
using System.Net.WebSockets;
using System.Text.RegularExpressions;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Text;

using Ultraleap.TouchFree.Library;
using Ultraleap.TouchFree.Service.ConnectionTypes;
using Ultraleap.TouchFree.Library.Configuration;

namespace Ultraleap.TouchFree.Service.Connection
{
    public class ClientConnection : IClientConnection
    {
        public WebSocket Socket
        {
            get
            {
                return socket;
            }
        }

        private readonly WebSocket socket;
        private bool HandshakeCompleted;
        private readonly WebSocketReceiver receiver;
        private readonly IClientConnectionManager clientMgr;
        private readonly IConfigManager configManager;

        public ClientConnection(WebSocket _socket, WebSocketReceiver _receiver, IClientConnectionManager _clientMgr, IConfigManager _configManager)
        {
            socket = _socket;
            receiver = _receiver;
            clientMgr = _clientMgr;
            configManager = _configManager;
            HandshakeCompleted = false;

            TouchFreeLog.WriteLine("Websocket Connection opened");
        }

        public void SendInputAction(InputAction _data)
        {
            if (!HandshakeCompleted)
            {
                // Long-term we shouldn't get this far until post-handshake, but the systems should
                // be designed cohesively when the Service gets its polish
                return;
            }

            WebsocketInputAction converted = new WebsocketInputAction(_data);

            SendResponse(converted, ActionCode.INPUT_ACTION);
        }

        public void SendHandPresenceEvent(HandPresenceEvent _response)
        {
            SendResponse(_response, ActionCode.HAND_PRESENCE_EVENT);
        }

        public void SendHandshakeResponse(ResponseToClient _response)
        {
            SendResponse(_response, ActionCode.VERSION_HANDSHAKE_RESPONSE);
        }

        public void SendConfigChangeResponse(ResponseToClient _response)
        {
            SendResponse(_response, ActionCode.CONFIGURATION_RESPONSE);
        }

        public void SendConfigFileChangeResponse(ResponseToClient _response)
        {
            SendResponse(_response, ActionCode.CONFIGURATION_FILE_CHANGE_RESPONSE);
        }

        public void SendConfigState(ConfigState _configState)
        {
            SendResponse(_configState, ActionCode.CONFIGURATION_STATE);
        }

        public void SendConfigFile(ConfigState _configState)
        {
            SendResponse(_configState, ActionCode.CONFIGURATION_FILE_STATE);
        }

        public void SendStatusResponse(ResponseToClient _response)
        {
            SendResponse(_response, ActionCode.SERVICE_STATUS_RESPONSE);
        }

        public void SendStatus(ServiceStatus _status)
        {
            SendResponse(_status, ActionCode.SERVICE_STATUS);
        }

        public void SendQuickSetupConfigFile(ConfigState _configState)
        {
            SendResponse(_configState, ActionCode.QUICK_SETUP_CONFIG);
        }

        public void SendQuickSetupResponse(ResponseToClient _response)
        {
            SendResponse(_response, ActionCode.QUICK_SETUP_RESPONSE);
        }

        internal void SendTrackingResponse(ResponseToClient _response, ActionCode _action)
        {
            if (_action == ActionCode.GET_TRACKING_STATE) {
                SendResponse(_response, ActionCode.GET_TRACKING_STATE_RESPONSE);
            } else if (_action == ActionCode.SET_TRACKING_STATE) {
                SendResponse(_response, ActionCode.SET_TRACKING_STATE_RESPONSE);
            }
        }

        internal void SendResponse<T>(T _response, ActionCode actionCode)
        {
            CommunicationWrapper<T> message =
                new CommunicationWrapper<T>(actionCode.ToString(), _response);

            string jsonMessage = JsonConvert.SerializeObject(message);

            Socket.SendAsync(
                Encoding.UTF8.GetBytes(jsonMessage),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None);
        }

        private void SendInitialHandState()
        {
            this.SendHandPresenceEvent(clientMgr.MissedHandPresenceEvent);
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
                return Compatibility.CLIENT_OUTDATED_WARNING;
            }
            else if (clientVersionParsed.Minor > _coreVersion.Minor)
            {
                return Compatibility.SERVICE_OUTDATED;
            }

            if (clientVersionParsed.Build > _coreVersion.Build)
            {
                return Compatibility.SERVICE_OUTDATED_WARNING;
            }

            return Compatibility.COMPATIBLE;
        }

        internal void OnMessage(string _message)
        {
            // Find key areas of the rawData, the "action" and the "content"
            var match = Regex.Match(_message, "{\\s*?\"action\"\\s*?:\\s*?\"([\\w\\d_]+?)\"\\s*?,\\s*?\"content\"\\s*?:\\s*?({.+?})\\s*?}$");

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

            // We don't handle after-the-fact Handshake Requests here. We may wish to
            // if / when we anticipate externals building their own Tooling clients.

            switch (action)
            {
                case ActionCode.REQUEST_SERVICE_STATUS:
                    receiver.requestServiceStatusQueue.Enqueue(content);
                    break;
                case ActionCode.QUICK_SETUP:
                    receiver.quickSetupQueue.Enqueue(content);
                    break;

                case ActionCode.SET_CONFIGURATION_STATE:
                    receiver.configChangeQueue.Enqueue(content);
                    break;
                case ActionCode.REQUEST_CONFIGURATION_STATE:
                    receiver.configStateRequestQueue.Enqueue(content);
                    break;

                case ActionCode.SET_CONFIGURATION_FILE:
                    receiver.configFileChangeQueue.Enqueue(content);
                    break;
                case ActionCode.REQUEST_CONFIGURATION_FILE:
                    receiver.configFileRequestQueue.Enqueue(content);
                    break;

                case ActionCode.GET_TRACKING_STATE:
                case ActionCode.SET_TRACKING_STATE:
                    IncomingRequest req = new IncomingRequest(action, null, content);
                    receiver.trackingApiChangeQueue.Enqueue(req);
                    break;

                case ActionCode.INPUT_ACTION:
                case ActionCode.CONFIGURATION_STATE:
                case ActionCode.CONFIGURATION_RESPONSE:
                case ActionCode.VERSION_HANDSHAKE_RESPONSE:
                case ActionCode.HAND_PRESENCE_EVENT:
                case ActionCode.SERVICE_STATUS_RESPONSE:
                case ActionCode.SERVICE_STATUS:
                case ActionCode.CONFIGURATION_FILE_STATE:
                case ActionCode.CONFIGURATION_FILE_CHANGE_RESPONSE:
                case ActionCode.QUICK_SETUP_RESPONSE:
                case ActionCode.GET_TRACKING_STATE_RESPONSE:
                case ActionCode.SET_TRACKING_STATE_RESPONSE:
                    TouchFreeLog.ErrorWriteLine("Received a " + action + " action. This action is not expected on the Service.");
                    break;
                default:
                    TouchFreeLog.ErrorWriteLine("Received a " + action + " action. This action is not recognised.");
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
                TouchFreeLog.ErrorWriteLine(response.message);
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
                TouchFreeLog.ErrorWriteLine(response.message);
                SendHandshakeResponse(response);
                return;
            }

            if (!contentObj.ContainsKey(VersionInfo.API_HEADER_NAME))
            {
                // Send back immediate error: Cannot compare version number w/o a version number
                response.status = "Failure";
                response.message = "Handshaking Failed: No API Version supplied.";
                TouchFreeLog.ErrorWriteLine(response.message);
                SendHandshakeResponse(response);
                return;
            }

            string clientApiVersion = (string)contentObj[VersionInfo.API_HEADER_NAME];
            Compatibility compatibility = GetVersionCompability(clientApiVersion, VersionInfo.ApiVersion);

            string configurationWarning = string.Empty;

            if (!configManager.AreConfigsInGoodState())
            {
                configurationWarning = " Configuration is in a bad state. Please update the configuration via TouchFree Settings";
            }

            switch (compatibility)
            {
                case Compatibility.COMPATIBLE:
                    HandshakeCompleted = true;
                    response.status = "Success";
                    response.message = "Handshake Successful." + configurationWarning;
                    TouchFreeLog.WriteLine(response.message);
                    SendHandshakeResponse(response);
                    SendInitialHandState();
                    return;
                case Compatibility.CLIENT_OUTDATED_WARNING:
                    HandshakeCompleted = true;
                    response.status = "Success";
                    response.message = "Handshake Warning: Client is outdated relative to Service." + configurationWarning;
                    TouchFreeLog.WriteLine(response.message);
                    SendHandshakeResponse(response);
                    SendInitialHandState();
                    return;
                case Compatibility.SERVICE_OUTDATED_WARNING:
                    HandshakeCompleted = true;
                    response.status = "Success";
                    response.message = "Handshake Warning: Service is outdated relative to Client." + configurationWarning;
                    TouchFreeLog.WriteLine(response.message);
                    SendHandshakeResponse(response);
                    SendInitialHandState();
                    return;
                case Compatibility.CLIENT_OUTDATED:
                    response.message = "Handshake Failed: Client is outdated relative to Service." + configurationWarning;
                    TouchFreeLog.ErrorWriteLine(response.message);
                    break;
                case Compatibility.SERVICE_OUTDATED:
                    response.message = "Handshake Failed: Service is outdated relative to Client." + configurationWarning;
                    TouchFreeLog.ErrorWriteLine(response.message);
                    break;
            }

            response.status = "Failure";
            SendHandshakeResponse(response);
            return;
        }
    }
}
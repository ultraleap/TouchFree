using System;
using System.Net.WebSockets;
using System.Text.RegularExpressions;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Text;

using Ultraleap.TouchFree.Library;
using Ultraleap.TouchFree.Service.ConnectionTypes;

namespace Ultraleap.TouchFree.Service.Connection
{
    internal class ClientConnection
    {
        public WebSocket socket;
        private bool HandshakeCompleted;
        private readonly WebSocketReceiver receiver;
        private readonly ClientConnectionManager clientMgr;

        public ClientConnection(WebSocket _socket, WebSocketReceiver _receiver, ClientConnectionManager _clientMgr)
        {
            socket = _socket;
            receiver = _receiver;
            clientMgr = _clientMgr;
            HandshakeCompleted = false;

            Console.WriteLine("Websocket Connection opened");
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

            CommunicationWrapper<WebsocketInputAction> message =
                new CommunicationWrapper<WebsocketInputAction>(ActionCode.INPUT_ACTION.ToString(), converted);

            string jsonMessage = JsonConvert.SerializeObject(message);

            Send(jsonMessage);
        }

        public void SendHandPresenceEvent(HandPresenceEvent _response)
        {
            CommunicationWrapper<HandPresenceEvent> message =
                new CommunicationWrapper<HandPresenceEvent>( ActionCode.HAND_PRESENCE_EVENT.ToString(), _response);

            string jsonMessage = JsonConvert.SerializeObject(message);

            Send(jsonMessage);
        }

        public void SendHandshakeResponse(ResponseToClient _response)
        {
            CommunicationWrapper<ResponseToClient> message =
                new CommunicationWrapper<ResponseToClient>(ActionCode.VERSION_HANDSHAKE_RESPONSE.ToString(), _response);

            string jsonMessage = JsonConvert.SerializeObject(message);

            Send(jsonMessage);
        }

        public void SendConfigChangeResponse(ResponseToClient _response)
        {
            CommunicationWrapper<ResponseToClient> message =
                new CommunicationWrapper<ResponseToClient>(ActionCode.CONFIGURATION_RESPONSE.ToString(), _response);

            string jsonMessage = JsonConvert.SerializeObject(message);

            Send(jsonMessage);
        }

        public void SendConfigFileChangeResponse(ResponseToClient _response)
        {
            CommunicationWrapper<ResponseToClient> message =
                new CommunicationWrapper<ResponseToClient>(ActionCode.CONFIGURATION_FILE_RESPONSE.ToString(), _response);

            string jsonMessage = JsonConvert.SerializeObject(message);

            Send(jsonMessage);
        }

        public void SendConfigState(ConfigState _configState)
        {
            CommunicationWrapper<ConfigState> message =
                new CommunicationWrapper<ConfigState>(ActionCode.CONFIGURATION_STATE.ToString(), _configState);

            string jsonMessage = JsonConvert.SerializeObject(message);

            Send(jsonMessage);
        }

        public void SendStatusResponse(ResponseToClient _response)
        {
            CommunicationWrapper<ResponseToClient> message =
                new CommunicationWrapper<ResponseToClient>(ActionCode.SERVICE_STATUS_RESPONSE.ToString(), _response);

            string jsonMessage = JsonConvert.SerializeObject(message);

            Send(jsonMessage);
        }

        public void SendStatus(ServiceStatus _status)
        {
            CommunicationWrapper<ServiceStatus> message =
                new CommunicationWrapper<ServiceStatus>(ActionCode.SERVICE_STATUS.ToString(), _status);

            string jsonMessage = JsonConvert.SerializeObject(message);

            Send(jsonMessage);
        }

        private void Send(string message)
        {
            socket.SendAsync(
                Encoding.UTF8.GetBytes(message),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None);
        }

        private void SendInitialHandState()
        {
            this.SendHandPresenceEvent(clientMgr.missedHandPresenceEvent);
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

            switch (action)
            {
                case ActionCode.SET_CONFIGURATION_STATE:
                    receiver.configChangeQueue.Enqueue(content);
                    break;
                case ActionCode.REQUEST_SERVICE_STATUS:
                    receiver.requestServiceStatusQueue.Enqueue(content);
                    break;
                case ActionCode.REQUEST_CONFIGURATION_STATE:
                    receiver.configStateRequestQueue.Enqueue(content);
                    break;
                case ActionCode.INPUT_ACTION:
                case ActionCode.CONFIGURATION_STATE:
                case ActionCode.CONFIGURATION_RESPONSE:
                    Console.Error.WriteLine("Received a " + action + " action. This action is not expected on the Service.");
                    break;
                default:
                    Console.Error.WriteLine("Received a " + action + " action. This action is not recognised.");
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
                Console.Error.WriteLine(response.message);
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
                Console.Error.WriteLine(response.message);
                SendHandshakeResponse(response);
                return;
            }

            if (!contentObj.ContainsKey(VersionInfo.API_HEADER_NAME))
            {
                // Send back immediate error: Cannot compare version number w/o a version number
                response.status = "Failure";
                response.message = "Handshaking Failed: No API Version supplied.";
                Console.Error.WriteLine(response.message);
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
                    Console.WriteLine(response.message);
                    SendHandshakeResponse(response);
                    SendInitialHandState();
                    return;
                case Compatibility.CLIENT_OUTDATED_WARNING:
                    HandshakeCompleted = true;
                    response.status = "Success";
                    response.message = "Handshake Warning: Client is outdated relative to Service.";
                    Console.WriteLine(response.message);
                    SendHandshakeResponse(response);
                    SendInitialHandState();
                    return;
                case Compatibility.SERVICE_OUTDATED_WARNING:
                    HandshakeCompleted = true;
                    response.status = "Success";
                    response.message = "Handshake Warning: Service is outdated relative to Client.";
                    Console.WriteLine(response.message);
                    SendHandshakeResponse(response);
                    SendInitialHandState();
                    return;
                case Compatibility.CLIENT_OUTDATED:
                    response.message = "Handshake Failed: Client is outdated relative to Service.";
                    Console.Error.WriteLine(response.message);
                    break;
                case Compatibility.SERVICE_OUTDATED:
                    response.message = "Handshake Failed: Service is outdated relative to Client.";
                    Console.Error.WriteLine(response.message);
                    break;
            }

            response.status = "Failure";
            SendHandshakeResponse(response);
            return;
        }
    }
}